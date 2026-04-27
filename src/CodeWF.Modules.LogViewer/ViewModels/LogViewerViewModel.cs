using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaEdit;
using CodeWF.Core.IServices;
using Lang.Avalonia;
using ReactiveUI;
using System.Reactive;
using System.Text;

namespace CodeWF.Modules.LogViewer.ViewModels;

public class LogViewerViewModel : ReactiveObject, IDisposable
{
    private const int BufferSize = 64 * 1024;
    private const int InitialIndexLineCount = 600;
    private const int MaxDisplayedTextBytes = 4 * 1024 * 1024;
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
    private string _displayedText = string.Empty;
    private bool _followTail = true;

    public LogViewerViewModel(IFileChooserService fileChooserService)
    {
        _fileChooserService = fileChooserService;
        OpenFileCommand = ReactiveCommand.CreateFromTask(OpenFileAsync);
        ReloadCommand = ReactiveCommand.CreateFromTask(ReloadAsync);
        JumpToTailCommand = ReactiveCommand.Create(JumpToTail);
        StatusText = T(Localization.LogViewerView.ReadyStatus);
    }

    public TextEditor? LogEditor { get; private set; }

    public void AttachEditor(TextEditor logEditor)
    {
        LogEditor = logEditor;
        SetEditorText(ToEditorDisplayText(_displayedText));
        if (FollowTail)
        {
            ScrollEditorToTail();
        }
    }

    public void DetachEditor(TextEditor logEditor)
    {
        if (ReferenceEquals(LogEditor, logEditor))
        {
            LogEditor = null;
        }
    }

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

            RefreshEditorTextFromIndex();
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
        ReplaceEditorText(string.Empty);
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
            RefreshEditorTextFromIndex();
        }

        StartBackgroundIndexing();
    }

    private void InitializeIndex(string path)
    {
        // 大文件不一次性读入内存，只维护行起始偏移，滚动时按可见窗口读取。
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
            var text = await Task.Run(() => ReadLastText(path, ViewportLineCount, token), token);
            ReplaceEditorText(text);
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
            if (FollowTail && _isIndexComplete && IsEditorVerticalScrollAtTail())
            {
                JumpToTail();
                return;
            }

            if (FollowTail && IsEditorVerticalScrollAtTail())
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
                        RefreshEditorTextFromIndex();
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

    private void RefreshEditorTextFromIndex()
    {
        var path = _activeFilePath;
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            ReplaceEditorText(string.Empty);
            return;
        }

        var start = (int)Math.Clamp(FirstLineIndex, 0, int.MaxValue);
        var text = ReadIndexedText(path, start, ViewportLineCount);
        ReplaceEditorText(text);
        UpdateMetrics(_isIndexComplete
            ? T(Localization.LogViewerView.MonitoringStatus)
            : T(Localization.LogViewerView.IndexingStatus));
    }

    private void JumpToTail()
    {
        var start = Math.Max(0, GetIndexedLineCount() - ViewportLineCount);
        _firstLineIndex = start;
        this.RaisePropertyChanged(nameof(FirstLineIndex));
        _ = ShowTailPreviewAsync();
    }

    private string ReadIndexedText(string path, int startLineIndex, int count)
    {
        List<long> offsets;
        lock (_indexLock)
        {
            offsets = _lineOffsets.Skip(startLineIndex).Take(count + 1).ToList();
        }

        if (offsets.Count == 0)
        {
            return string.Empty;
        }

        var startOffset = offsets[0];
        var endOffset = offsets.Count > count
            ? offsets[^1]
            : GetFileLength(path);
        return ReadTextRange(path, startOffset, endOffset);
    }

    private string ReadLastText(string path, int count, CancellationToken token)
    {
        var length = GetFileLength(path);
        if (length == 0)
        {
            return string.Empty;
        }

        var offsets = new List<long>();
        using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete,
            BufferSize,
            FileOptions.RandomAccess);
        var contentEnd = FindLastContentEndOffset(stream, length, token);
        if (contentEnd == 0)
        {
            return string.Empty;
        }

        var buffer = new byte[BufferSize];
        var position = contentEnd;

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

        var startOffset = offsets
            .Distinct()
            .OrderBy(offset => offset)
            .First();
        var text = TrimTextToLastLines(ReadTextRange(path, startOffset, contentEnd), count);
        return contentEnd < length
            ? $"{text}\n"
            : text;
    }

    private static long FindLastContentEndOffset(FileStream stream, long length, CancellationToken token)
    {
        var buffer = new byte[BufferSize];
        var position = length;

        while (position > 0)
        {
            token.ThrowIfCancellationRequested();

            var readSize = (int)Math.Min(BufferSize, position);
            position -= readSize;
            stream.Position = position;
            var read = stream.Read(buffer, 0, readSize);

            for (var i = read - 1; i >= 0; i--)
            {
                if (buffer[i] != (byte)'\r' && buffer[i] != (byte)'\n')
                {
                    return position + i + 1;
                }
            }
        }

        return 0;
    }

    private static string ReadTextRange(string path, long startOffset, long endOffset)
    {
        if (startOffset >= endOffset || !File.Exists(path))
        {
            return string.Empty;
        }

        var bytesToRead = (int)Math.Min(endOffset - startOffset, MaxDisplayedTextBytes);
        var buffer = new byte[bytesToRead];
        using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete,
            BufferSize,
            FileOptions.RandomAccess);
        stream.Position = startOffset;

        var totalRead = 0;
        while (totalRead < bytesToRead)
        {
            var read = stream.Read(buffer, totalRead, bytesToRead - totalRead);
            if (read == 0)
            {
                break;
            }

            totalRead += read;
        }

        var text = Encoding.UTF8.GetString(buffer, 0, totalRead);
        return startOffset == 0
            ? text.TrimStart('\uFEFF')
            : text;
    }

    private void ReplaceEditorText(string text)
    {
        _displayedText = TrimTextToLastLines(text, ViewportLineCount);
        SetEditorText(ToEditorDisplayText(_displayedText));
    }

    private void AppendEditorText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        AppendEditorText(text, trimToViewport: true);
    }

    private void AppendEditorText(string text, bool trimToViewport)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        var previousDisplayText = ToEditorDisplayText(_displayedText);
        var nextText = _displayedText + text;
        var trimmedText = trimToViewport
            ? TrimTextToLastLines(nextText, ViewportLineCount)
            : TrimTextToMaxChars(nextText, MaxDisplayedTextBytes);
        _displayedText = trimmedText;
        AppendOrSetEditorText(previousDisplayText, ToEditorDisplayText(_displayedText));
    }

    private static string TrimTextToLastLines(string text, int lineCount)
    {
        if (string.IsNullOrEmpty(text) || lineCount <= 0)
        {
            return string.Empty;
        }

        var linesSeen = 0;
        for (var i = text.Length - 1; i >= 0; i--)
        {
            if (text[i] != '\n')
            {
                continue;
            }

            linesSeen++;
            if (linesSeen > lineCount)
            {
                return text[(i + 1)..];
            }
        }

        return text;
    }

    private static string TrimTextToMaxChars(string text, int maxChars)
    {
        if (text.Length <= maxChars)
        {
            return text;
        }

        return text[^maxChars..];
    }

    private static string ToEditorDisplayText(string text)
    {
        return text.TrimEnd('\r', '\n');
    }

    private void SetEditorText(string text)
    {
        var editor = LogEditor;
        if (editor == null)
        {
            return;
        }

        if (Dispatcher.UIThread.CheckAccess())
        {
            editor.Text = text;
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            if (ReferenceEquals(LogEditor, editor))
            {
                editor.Text = text;
            }
        });
    }

    private void AppendOrSetEditorText(string previousDisplayText, string nextDisplayText)
    {
        var editor = LogEditor;
        if (editor == null)
        {
            return;
        }

        if (Dispatcher.UIThread.CheckAccess())
        {
            AppendOrSetEditorTextCore(editor, previousDisplayText, nextDisplayText);
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            if (ReferenceEquals(LogEditor, editor))
            {
                AppendOrSetEditorTextCore(editor, previousDisplayText, nextDisplayText);
            }
        });
    }

    private static void AppendOrSetEditorTextCore(TextEditor editor, string previousDisplayText, string nextDisplayText)
    {
        if (editor.Text == previousDisplayText
            && nextDisplayText.StartsWith(previousDisplayText, StringComparison.Ordinal))
        {
            editor.AppendText(nextDisplayText[previousDisplayText.Length..]);
            return;
        }

        editor.Text = nextDisplayText;
    }

    private void ScrollEditorToTail()
    {
        var editor = LogEditor;
        if (editor == null)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            if (!ReferenceEquals(LogEditor, editor) || editor.Document == null)
            {
                return;
            }

            editor.CaretOffset = editor.Document.TextLength;
            editor.ScrollToEnd();
        }, DispatcherPriority.Render);
    }

    private bool IsEditorVerticalScrollAtTail()
    {
        var editor = LogEditor;
        if (editor == null)
        {
            return IsScrollAtTail();
        }

        var scrollViewer = GetEditorScrollViewer(editor);
        if (scrollViewer == null)
        {
            return true;
        }

        if (editor.SelectionLength > 0)
        {
            return false;
        }

        var maxY = Math.Max(0, scrollViewer.Extent.Height - scrollViewer.Viewport.Height);
        return maxY <= 1 || scrollViewer.Offset.Y >= maxY - 1;
    }

    private Vector? CaptureEditorScrollOffset()
    {
        var editor = LogEditor;
        var scrollViewer = editor == null ? null : GetEditorScrollViewer(editor);
        return scrollViewer?.Offset;
    }

    private void RestoreEditorScrollOffset(Vector? offset)
    {
        if (!offset.HasValue)
        {
            return;
        }

        var editor = LogEditor;
        var scrollViewer = editor == null ? null : GetEditorScrollViewer(editor);
        if (scrollViewer != null)
        {
            scrollViewer.Offset = offset.Value;
            Dispatcher.UIThread.Post(() =>
            {
                if (ReferenceEquals(LogEditor, editor))
                {
                    scrollViewer.Offset = offset.Value;
                }
            }, DispatcherPriority.Render);
        }
    }

    private static ScrollViewer? GetEditorScrollViewer(TextEditor editor)
    {
        return editor
            .GetVisualDescendants()
            .OfType<ScrollViewer>()
            .FirstOrDefault();
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
        // 只有用户仍停留在末尾时才自动跟随，手动上翻或正在选择文本时保留当前位置。
        var shouldKeepTail = FollowTail && await Dispatcher.UIThread.InvokeAsync(IsEditorVerticalScrollAtTail);
        var preservedOffset = shouldKeepTail
            ? null
            : await Dispatcher.UIThread.InvokeAsync(CaptureEditorScrollOffset);
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
            _tailPosition = newLength;
            if (newLength - oldTailPosition > MaxDisplayedTextBytes)
            {
                await ShowTailPreviewAsync();
                StartBackgroundIndexing();
                return;
            }

            var appendedText = ReadTextRange(path, oldTailPosition, newLength);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                AppendEditorText(appendedText);
                UpdateMetrics(T(Localization.LogViewerView.MonitoringStatus));
                KeepTailInView();
            });
        }
        else
        {
            _tailPosition = newLength;
            if (newLength - oldTailPosition > MaxDisplayedTextBytes)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    UpdateMetrics(T(Localization.LogViewerView.MonitoringStatus));
                    RestoreEditorScrollOffset(preservedOffset);
                });
                StartBackgroundIndexing();
                return;
            }

            var appendedText = ReadTextRange(path, oldTailPosition, newLength);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                AppendEditorText(appendedText, trimToViewport: false);
                UpdateMetrics(T(Localization.LogViewerView.MonitoringStatus));
                RestoreEditorScrollOffset(preservedOffset);
            });
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
        ScrollEditorToTail();
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
