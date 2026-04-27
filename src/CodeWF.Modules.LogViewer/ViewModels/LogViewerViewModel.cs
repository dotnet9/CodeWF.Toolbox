using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CodeWF.Core.IServices;
using CodeWF.Modules.LogViewer.Models;
using Lang.Avalonia;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Text;

namespace CodeWF.Modules.LogViewer.ViewModels;

public class LogViewerViewModel : ReactiveObject, IDisposable
{
    private const int BufferSize = 64 * 1024;
    private const int InitialIndexLineCount = 600;
    private const int MaxDisplayedLineChars = 20_000;
    private const int IndexNotifyLineBatch = 5_000;

    private static readonly FilePickerFileType LogFilePickerFileType =
        new("Log files") { Patterns = ["*.log", "*.txt", "*.trace", "*.out", "*.err"] };

    private readonly IFileChooserService _fileChooserService;
    private readonly List<long> _lineOffsets = [];
    private readonly object _indexLock = new();
    private readonly SemaphoreSlim _indexGate = new(1, 1);
    private CancellationTokenSource? _loadCts;
    private FileSystemWatcher? _watcher;
    private string? _activeFilePath;
    private long _fileLength;
    private long _indexedBytes;
    private long _tailPosition;
    private bool _isIndexComplete;
    private bool _isDisposed;
    private int _fileChangePending;
    private double _firstLineIndex;
    private bool _followTail = true;

    public LogViewerViewModel(IFileChooserService fileChooserService)
    {
        _fileChooserService = fileChooserService;
        OpenFileCommand = ReactiveCommand.CreateFromTask(OpenFileAsync);
        ReloadCommand = ReactiveCommand.CreateFromTask(ReloadAsync);
        JumpToTailCommand = ReactiveCommand.Create(JumpToTail);
        StatusText = T(Localization.LogViewerView.ReadyStatus);
    }

    public ObservableCollection<LogLine> VisibleLines { get; } = [];

    public event EventHandler? ScrollToTailRequested;

    public string FilePath
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = string.Empty;

    public string StatusText
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsLogOpened
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public int IndexedLineCount
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public int ViewportLineCount
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = 260;

    public double FirstLineIndex
    {
        get => _firstLineIndex;
        set
        {
            var normalized = Math.Clamp(Math.Round(value), 0, ScrollMaximum);
            if (Math.Abs(_firstLineIndex - normalized) < 0.1)
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _firstLineIndex, normalized);
            if (FollowTail)
            {
                _followTail = false;
                this.RaisePropertyChanged(nameof(FollowTail));
            }

            RefreshVisibleLinesFromIndex();
        }
    }

    public double ScrollMaximum
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool FollowTail
    {
        get => _followTail;
        set
        {
            this.RaiseAndSetIfChanged(ref _followTail, value);
            if (value)
            {
                _ = ShowTailPreviewAsync();
            }
        }
    }

    public ReactiveCommand<Unit, Unit> OpenFileCommand { get; }

    public ReactiveCommand<Unit, Unit> ReloadCommand { get; }

    public ReactiveCommand<Unit, Unit> JumpToTailCommand { get; }

    private async Task OpenFileAsync()
    {
        var files = await _fileChooserService.OpenFileAsync(
            T(Localization.LogViewerView.OpenLogFileDialogTitle),
            false,
            [LogFilePickerFileType, FilePickerFileTypes.All]);
        if (files is not { Count: > 0 } || string.IsNullOrWhiteSpace(files[0]))
        {
            return;
        }

        await LoadFileAsync(files[0]);
    }

    private async Task ReloadAsync()
    {
        if (string.IsNullOrWhiteSpace(_activeFilePath))
        {
            return;
        }

        await LoadFileAsync(_activeFilePath);
    }

    private async Task LoadFileAsync(string path)
    {
        CancelActiveWork();

        _activeFilePath = path;
        FilePath = path;
        IsLogOpened = true;
        VisibleLines.Clear();
        StatusText = T(Localization.LogViewerView.OpeningStatus);

        _loadCts = new CancellationTokenSource();
        var token = _loadCts.Token;

        InitializeIndex(path);
        SetupWatcher(path);
        UpdateMetrics(T(Localization.LogViewerView.OpeningStatus));

        if (_fileLength == 0)
        {
            StatusText = T(Localization.LogViewerView.EmptyFileStatus);
            return;
        }

        if (FollowTail)
        {
            await ShowTailPreviewAsync();
        }
        else
        {
            await IndexToLineCountAsync(InitialIndexLineCount, token);
            RefreshVisibleLinesFromIndex();
        }

        StartBackgroundIndexing();
    }

    private void InitializeIndex(string path)
    {
        _fileLength = GetFileLength(path);
        _tailPosition = _fileLength;
        _indexedBytes = 0;
        _isIndexComplete = _fileLength == 0;

        lock (_indexLock)
        {
            _lineOffsets.Clear();
            if (_fileLength > 0)
            {
                _lineOffsets.Add(0);
            }
        }
    }

    private async Task ShowTailPreviewAsync()
    {
        var path = _activeFilePath;
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return;
        }

        var token = _loadCts?.Token ?? CancellationToken.None;
        try
        {
            var lines = await Task.Run(() => ReadLastLines(path, ViewportLineCount, token), token);
            ReplaceVisibleLines(lines);
            _tailPosition = GetFileLength(path);
            UpdateMetrics(_isIndexComplete
                ? T(Localization.LogViewerView.MonitoringStatus)
                : T(Localization.LogViewerView.TailPreviewStatus));
            KeepTailInView();
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task IndexToLineCountAsync(int lineCount, CancellationToken token)
    {
        await _indexGate.WaitAsync(token);
        try
        {
            if (GetIndexedLineCount() >= lineCount || _indexedBytes >= _fileLength)
            {
                return;
            }

            IndexRangeCore(_indexedBytes, _fileLength, lineCount, token);
        }
        finally
        {
            _indexGate.Release();
        }

        await Dispatcher.UIThread.InvokeAsync(() => UpdateMetrics(T(Localization.LogViewerView.IndexingStatus)));
    }

    private void StartBackgroundIndexing()
    {
        var token = _loadCts?.Token;
        if (token == null)
        {
            return;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                await ContinueIndexAsync(token.Value);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                await Dispatcher.UIThread.InvokeAsync(() => StatusText = ex.Message);
            }
        }, token.Value);
    }

    private async Task ContinueIndexAsync(CancellationToken token)
    {
        await _indexGate.WaitAsync(token);
        try
        {
            while (!token.IsCancellationRequested)
            {
                var path = _activeFilePath;
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                {
                    return;
                }

                _fileLength = GetFileLength(path);
                if (_indexedBytes >= _fileLength)
                {
                    _isIndexComplete = true;
                    break;
                }

                _isIndexComplete = false;
                IndexRangeCore(_indexedBytes, _fileLength, null, token);
            }
        }
        finally
        {
            _indexGate.Release();
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            UpdateMetrics(T(Localization.LogViewerView.MonitoringStatus));
            if (FollowTail && _isIndexComplete)
            {
                JumpToTail();
                return;
            }

            if (FollowTail)
            {
                KeepTailInView();
            }
        });
    }

    private void IndexRangeCore(long startOffset, long endOffset, int? stopWhenLineCountReaches, CancellationToken token)
    {
        var path = _activeFilePath;
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path) || startOffset >= endOffset)
        {
            return;
        }

        EnsureFirstOffset(endOffset);

        using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete,
            BufferSize,
            FileOptions.SequentialScan);
        stream.Position = startOffset;

        var buffer = new byte[BufferSize];
        var position = startOffset;
        var linesSinceNotify = 0;

        while (position < endOffset)
        {
            token.ThrowIfCancellationRequested();

            var bytesToRead = (int)Math.Min(buffer.Length, endOffset - position);
            var read = stream.Read(buffer, 0, bytesToRead);
            if (read <= 0)
            {
                break;
            }

            var chunkStart = position;
            for (var i = 0; i < read; i++)
            {
                if (buffer[i] != (byte)'\n')
                {
                    continue;
                }

                var nextLineOffset = chunkStart + i + 1;
                if (nextLineOffset >= endOffset)
                {
                    continue;
                }

                AddLineOffset(nextLineOffset);
                linesSinceNotify++;
            }

            position += read;
            _indexedBytes = position;

            if (linesSinceNotify >= IndexNotifyLineBatch)
            {
                linesSinceNotify = 0;
                Dispatcher.UIThread.Post(() =>
                {
                    UpdateMetrics(T(Localization.LogViewerView.IndexingStatus));
                    if (!FollowTail)
                    {
                        RefreshVisibleLinesFromIndex();
                    }
                });
            }

            if (stopWhenLineCountReaches.HasValue && GetIndexedLineCount() >= stopWhenLineCountReaches.Value)
            {
                break;
            }
        }

        _indexedBytes = Math.Max(_indexedBytes, position);
        _isIndexComplete = _indexedBytes >= endOffset;
    }

    private void RefreshVisibleLinesFromIndex()
    {
        var path = _activeFilePath;
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            VisibleLines.Clear();
            return;
        }

        var start = (int)Math.Clamp(FirstLineIndex, 0, int.MaxValue);
        var lines = ReadIndexedLines(path, start, ViewportLineCount);
        ReplaceVisibleLines(lines);
        UpdateMetrics(_isIndexComplete
            ? T(Localization.LogViewerView.MonitoringStatus)
            : T(Localization.LogViewerView.IndexingStatus));
    }

    private void JumpToTail()
    {
        if (!_isIndexComplete)
        {
            _ = ShowTailPreviewAsync();
            return;
        }

        var start = Math.Max(0, GetIndexedLineCount() - ViewportLineCount);
        _firstLineIndex = start;
        this.RaisePropertyChanged(nameof(FirstLineIndex));
        RefreshVisibleLinesFromIndex();
        KeepTailInView();
    }

    private List<LogLine> ReadIndexedLines(string path, int startLineIndex, int count)
    {
        List<long> offsets;
        lock (_indexLock)
        {
            offsets = _lineOffsets.Skip(startLineIndex).Take(count).ToList();
        }

        var lines = new List<LogLine>(offsets.Count);
        for (var i = 0; i < offsets.Count; i++)
        {
            var text = ReadLineAt(path, offsets[i]);
            lines.Add(new LogLine((startLineIndex + i + 1).ToString("N0"), text));
        }

        return lines;
    }

    private List<LogLine> ReadLastLines(string path, int count, CancellationToken token)
    {
        var length = GetFileLength(path);
        if (length == 0)
        {
            return [];
        }

        var offsets = new List<long>();
        using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete,
            BufferSize,
            FileOptions.RandomAccess);
        var buffer = new byte[BufferSize];
        var position = length;

        while (position > 0 && offsets.Count < count)
        {
            token.ThrowIfCancellationRequested();

            var readSize = (int)Math.Min(BufferSize, position);
            position -= readSize;
            stream.Position = position;
            var read = stream.Read(buffer, 0, readSize);

            for (var i = read - 1; i >= 0 && offsets.Count < count; i--)
            {
                if (buffer[i] != (byte)'\n')
                {
                    continue;
                }

                var lineStart = position + i + 1;
                if (lineStart < length)
                {
                    offsets.Add(lineStart);
                }
            }
        }

        if (offsets.Count < count)
        {
            offsets.Add(0);
        }

        return offsets
            .Distinct()
            .OrderBy(offset => offset)
            .Select(offset => new LogLine(string.Empty, ReadLineAt(path, offset)))
            .ToList();
    }

    private static string ReadLineAt(string path, long offset)
    {
        using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete,
            BufferSize,
            FileOptions.RandomAccess);
        stream.Position = offset;
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var line = reader.ReadLine() ?? string.Empty;
        if (offset == 0)
        {
            line = line.TrimStart('\uFEFF');
        }

        return line.Length <= MaxDisplayedLineChars
            ? line
            : $"{line[..MaxDisplayedLineChars]} ...";
    }

    private List<LogLine> ReadAppendedLines(string path, long startOffset, long endOffset)
    {
        if (startOffset >= endOffset || !File.Exists(path))
        {
            return [];
        }

        using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete,
            BufferSize,
            FileOptions.SequentialScan);
        stream.Position = startOffset;
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var lines = new List<LogLine>();

        while (reader.ReadLine() is { } line)
        {
            lines.Add(new LogLine(string.Empty, line.Length <= MaxDisplayedLineChars
                ? line
                : $"{line[..MaxDisplayedLineChars]} ..."));
        }

        return lines;
    }

    private void ReplaceVisibleLines(IEnumerable<LogLine> lines)
    {
        VisibleLines.Clear();
        foreach (var line in lines)
        {
            VisibleLines.Add(line);
        }
    }

    private void AppendTailLines(IEnumerable<LogLine> lines)
    {
        foreach (var line in lines)
        {
            VisibleLines.Add(line);
        }

        while (VisibleLines.Count > ViewportLineCount)
        {
            VisibleLines.RemoveAt(0);
        }
    }

    private void SetupWatcher(string path)
    {
        _watcher?.Dispose();
        _watcher = null;

        var directory = Path.GetDirectoryName(path);
        var fileName = Path.GetFileName(path);
        if (string.IsNullOrWhiteSpace(directory) || string.IsNullOrWhiteSpace(fileName))
        {
            return;
        }

        _watcher = new FileSystemWatcher(directory, fileName)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size,
            EnableRaisingEvents = true
        };
        _watcher.Changed += WatcherOnChanged;
        _watcher.Created += WatcherOnChanged;
        _watcher.Deleted += WatcherOnDeleted;
        _watcher.Renamed += WatcherOnChanged;
    }

    private void WatcherOnChanged(object sender, FileSystemEventArgs e)
    {
        _ = HandleFileChangedDebouncedAsync();
    }

    private void WatcherOnDeleted(object sender, FileSystemEventArgs e)
    {
        Dispatcher.UIThread.Post(() => StatusText = T(Localization.LogViewerView.FileDeletedStatus));
    }

    private async Task HandleFileChangedDebouncedAsync()
    {
        if (Interlocked.Exchange(ref _fileChangePending, 1) == 1)
        {
            return;
        }

        try
        {
            await Task.Delay(250);
            await ProcessFileChangedAsync();
        }
        finally
        {
            Interlocked.Exchange(ref _fileChangePending, 0);
        }
    }

    private async Task ProcessFileChangedAsync()
    {
        var path = _activeFilePath;
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            await Dispatcher.UIThread.InvokeAsync(() => StatusText = T(Localization.LogViewerView.FileDeletedStatus));
            return;
        }

        var newLength = GetFileLength(path);
        var oldLength = _fileLength;
        var wasIndexedToTail = _indexedBytes >= oldLength;
        var shouldKeepTail = FollowTail || IsScrollAtTail();
        if (newLength < _fileLength)
        {
            Dispatcher.UIThread.Post(async () => await LoadFileAsync(path));
            return;
        }

        if (newLength == _fileLength)
        {
            return;
        }

        var oldTailPosition = _tailPosition <= 0 ? _fileLength : _tailPosition;
        _fileLength = newLength;
        _isIndexComplete = false;
        if (wasIndexedToTail)
        {
            TryIndexAppendedRange(newLength);
        }

        if (shouldKeepTail)
        {
            var appendedLines = ReadAppendedLines(path, oldTailPosition, newLength);
            _tailPosition = newLength;
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                AppendTailLines(appendedLines);
                UpdateMetrics(T(Localization.LogViewerView.MonitoringStatus));
                KeepTailInView();
            });
        }
        else
        {
            await Dispatcher.UIThread.InvokeAsync(() => UpdateMetrics(T(Localization.LogViewerView.MonitoringStatus)));
        }

        StartBackgroundIndexing();
    }

    private void TryIndexAppendedRange(long endOffset)
    {
        if (!_indexGate.Wait(0))
        {
            return;
        }

        try
        {
            IndexRangeCore(_indexedBytes, endOffset, null, CancellationToken.None);
        }
        finally
        {
            _indexGate.Release();
        }
    }

    private void UpdateMetrics(string status)
    {
        var indexedLineCount = GetIndexedLineCount();
        IndexedLineCount = indexedLineCount;
        ScrollMaximum = Math.Max(0, indexedLineCount - ViewportLineCount);
        if (_firstLineIndex > ScrollMaximum)
        {
            _firstLineIndex = ScrollMaximum;
            this.RaisePropertyChanged(nameof(FirstLineIndex));
        }

        StatusText = $"{status} · {T(Localization.LogViewerView.LinesStatus)} {indexedLineCount:N0} · {T(Localization.LogViewerView.SizeStatus)} {FormatBytes(_fileLength)}";
    }

    private bool IsScrollAtTail()
    {
        return ScrollMaximum <= 0 || Math.Abs(_firstLineIndex - ScrollMaximum) < 1;
    }

    private void KeepTailInView()
    {
        MoveScrollBarToTail();
        ScrollToTailRequested?.Invoke(this, EventArgs.Empty);
    }

    private void MoveScrollBarToTail()
    {
        var target = ScrollMaximum;
        if (Math.Abs(_firstLineIndex - target) < 0.1)
        {
            return;
        }

        _firstLineIndex = target;
        this.RaisePropertyChanged(nameof(FirstLineIndex));
    }

    private void EnsureFirstOffset(long endOffset)
    {
        if (endOffset <= 0)
        {
            return;
        }

        lock (_indexLock)
        {
            if (_lineOffsets.Count == 0)
            {
                _lineOffsets.Add(0);
            }
        }
    }

    private void AddLineOffset(long offset)
    {
        lock (_indexLock)
        {
            if (_lineOffsets.Count == 0 || _lineOffsets[^1] < offset)
            {
                _lineOffsets.Add(offset);
            }
        }
    }

    private int GetIndexedLineCount()
    {
        lock (_indexLock)
        {
            return _lineOffsets.Count;
        }
    }

    private static long GetFileLength(string path)
    {
        return new FileInfo(path).Length;
    }

    private static string FormatBytes(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        var size = (double)bytes;
        var unit = 0;
        while (size >= 1024 && unit < units.Length - 1)
        {
            size /= 1024;
            unit++;
        }

        return $"{size:0.##} {units[unit]}";
    }

    private static string T(string key)
    {
        try
        {
            return I18nManager.Instance.GetResource(key) ?? key;
        }
        catch
        {
            return key;
        }
    }

    private void CancelActiveWork()
    {
        _loadCts?.Cancel();
        _loadCts?.Dispose();
        _loadCts = null;
        _watcher?.Dispose();
        _watcher = null;
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        CancelActiveWork();
        _indexGate.Dispose();
    }
}
