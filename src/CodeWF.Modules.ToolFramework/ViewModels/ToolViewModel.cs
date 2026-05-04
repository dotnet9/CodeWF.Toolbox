using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.Platform.Storage;
using CodeWF.Core.Helpers;
using CodeWF.Core.IServices;
using CodeWF.Modules.ToolFramework.Models;
using CodeWF.Modules.ToolFramework.Services;
using Lang.Avalonia;
using Prism.Regions;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace CodeWF.Modules.ToolFramework.ViewModels;

public sealed class ToolViewModel : ReactiveObject, INavigationAware, IDisposable
{
    private readonly ToolRegistry _registry;
    private readonly IFileChooserService _fileChooserService;
    private readonly CompositeDisposable _subscriptions = new();
    private CancellationTokenSource? _runCts;
    private ToolSpec? _currentTool;
    private string _errorMessage = string.Empty;

    public ToolViewModel(ToolRegistry registry, IFileChooserService fileChooserService)
    {
        _registry = registry;
        _fileChooserService = fileChooserService;
        RunCommand = ReactiveCommand.CreateFromTask(RunAsync);
        BrowsePathCommand = ReactiveCommand.CreateFromTask<ToolField>(BrowsePathAsync);
        CopyOutputCommand = ReactiveCommand.CreateFromTask<ToolOutput>(CopyOutputAsync);
    }

    public Control? ClipboardOwner { get; set; }

    public ObservableCollection<ToolField> Fields { get; } = [];

    public ObservableCollection<ToolOutput> Outputs { get; } = [];

    public ObservableCollection<ToolActionItem> Actions { get; } = [];

    public string BrowseLabel => ToolLocalization.Common.Browse;

    public string CopyLabel => ToolLocalization.Common.Copy;

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            this.RaiseAndSetIfChanged(ref _errorMessage, value);
            this.RaisePropertyChanged(nameof(HasError));
        }
    }

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public ReactiveCommand<Unit, Unit> RunCommand { get; }

    public ReactiveCommand<ToolField, Unit> BrowsePathCommand { get; }

    public ReactiveCommand<ToolOutput, Unit> CopyOutputCommand { get; }

    public bool IsNavigationTarget(NavigationContext navigationContext)
    {
        return true;
    }

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
        _runCts?.Cancel();
    }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        var id = navigationContext.Parameters.GetValue<string>("tool");
        if (string.IsNullOrWhiteSpace(id))
        {
            ErrorMessage = Translate(ToolLocalization.Common.MissingToolId);
            return;
        }

        LoadTool(id);
    }

    private void LoadTool(string id)
    {
        _subscriptions.Clear();
        _runCts?.Cancel();
        _currentTool = _registry.Create(id);
        Fields.Clear();
        Outputs.Clear();
        Actions.Clear();
        ErrorMessage = string.Empty;

        if (_currentTool == null)
        {
            ErrorMessage = string.Format(
                Translate(ToolLocalization.Common.ToolNotRegistered),
                id);
            return;
        }

        foreach (var field in _currentTool.Fields)
        {
            Fields.Add(field);

            if (!_currentTool.AutoRun)
            {
                continue;
            }

            _subscriptions.Add(field.WhenAnyValue(x => x.Text)
                .Skip(1)
                .Throttle(TimeSpan.FromMilliseconds(300))
                .Subscribe(_ => Dispatcher.UIThread.Post(async () => await RunAsync())));
            _subscriptions.Add(field.WhenAnyValue(x => x.Number)
                .Skip(1)
                .Throttle(TimeSpan.FromMilliseconds(300))
                .Subscribe(_ => Dispatcher.UIThread.Post(async () => await RunAsync())));
            _subscriptions.Add(field.WhenAnyValue(x => x.Boolean)
                .Skip(1)
                .Subscribe(_ => Dispatcher.UIThread.Post(async () => await RunAsync())));
            _subscriptions.Add(field.WhenAnyValue(x => x.SelectedOption)
                .Skip(1)
                .Subscribe(_ => Dispatcher.UIThread.Post(async () => await RunAsync())));
        }

        foreach (var output in _currentTool.Outputs)
        {
            Outputs.Add(output);
        }

        Actions.Add(new ToolActionItem
        {
            Label = ToolLocalization.Common.Run,
            IsPrimary = true,
            Command = RunCommand
        });

        _ = RunAsync();
    }

    private async Task RunAsync()
    {
        if (_currentTool?.RunAsync == null)
        {
            return;
        }

        _runCts?.Cancel();
        _runCts = new CancellationTokenSource();
        var token = _runCts.Token;
        var context = new ToolRunContext(
            Fields.ToDictionary(field => field.Id, StringComparer.OrdinalIgnoreCase),
            Outputs.ToDictionary(output => output.Id, StringComparer.OrdinalIgnoreCase));

        try
        {
            ErrorMessage = string.Empty;
            this.RaisePropertyChanged(nameof(HasError));
            await _currentTool.RunAsync(context, token);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            context.ClearOutputs();
            ErrorMessage = ex.Message;
            this.RaisePropertyChanged(nameof(HasError));
        }
    }

    private async Task BrowsePathAsync(ToolField field)
    {
        if (field.IsSaveFile)
        {
            var savePath = await _fileChooserService.SaveFileAsync(
                Translate(ToolLocalization.Common.SaveFile),
                [FilePickerFileTypes.All]);
            if (!string.IsNullOrWhiteSpace(savePath))
            {
                field.Text = savePath;
            }

            return;
        }

        var files = await _fileChooserService.OpenFileAsync(
            Translate(ToolLocalization.Common.OpenFile),
            false,
            [FilePickerFileTypes.All]);
        var path = files?.FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(path))
        {
            field.Text = path;
        }
    }

    private async Task CopyOutputAsync(ToolOutput output)
    {
        await ClipboardHelper.SetTextAsync(ClipboardOwner, output.Text);
    }

    public void OnKeyDown(string key, string physicalKey, string modifiers)
    {
        if (!string.Equals(_currentTool?.Id, "keycode-info", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var context = new ToolRunContext(
            Fields.ToDictionary(field => field.Id, StringComparer.OrdinalIgnoreCase),
            Outputs.ToDictionary(output => output.Id, StringComparer.OrdinalIgnoreCase));
        context.SetText("result", $"Key: {key}{Environment.NewLine}Physical key: {physicalKey}{Environment.NewLine}Modifiers: {modifiers}");
    }

    private static string Translate(string key)
    {
        return I18nManager.Instance.GetResource(key) ?? key;
    }

    public void Dispose()
    {
        _runCts?.Cancel();
        _runCts?.Dispose();
        _subscriptions.Dispose();
    }
}

