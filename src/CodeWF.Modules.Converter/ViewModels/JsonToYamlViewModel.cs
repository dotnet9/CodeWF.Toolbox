using Avalonia.Controls;
using AvaloniaEdit;
using CodeWF.Tools.Extensions;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;

namespace CodeWF.Modules.Converter.ViewModels;

public class JsonToYamlViewModel : ReactiveObject
{
    public TextEditor? JsonEditor { get; set; }
    public TextEditor? YamlEditor { get; set; }

    public JsonToYamlViewModel()
    {
        RaiseClearCommand = ReactiveCommand.CreateFromTask(RaiseClearHandlerAsync);
        RaiseCopyCommand = ReactiveCommand.CreateFromTask(RaiseCopyHandlerAsync);
    }

    public void StartListen()
    {
        this.WhenAnyValue(x => x.JsonString)
            .Throttle(TimeSpan.FromMilliseconds(400))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => JsonChanged());
        JsonEditor!.TextChanged += (s, e) => JsonString = JsonEditor.Text;
    }

    private string? _jsonString;

    public string? JsonString
    {
        get => _jsonString;
        set => this.RaiseAndSetIfChanged(ref _jsonString, value);
    }

    private string? _errorMessage;

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    public ReactiveCommand<Unit, Unit> RaiseClearCommand { get; }
    public ReactiveCommand<Unit, Unit> RaiseCopyCommand { get; }

    private void JsonChanged()
    {
        if (JsonEditor == null || YamlEditor == null)
        {
            return;
        }

        if (JsonString.JsonToYaml(out var jsonString, out string? errorMsg))
        {
            YamlEditor.Text = jsonString;
            ErrorMessage = default;
        }
        else
        {
            YamlEditor.Text = default;
            ErrorMessage = errorMsg;
        }
    }

    private async Task RaiseClearHandlerAsync()
    {
        JsonEditor?.Clear();
    }

    private async Task RaiseCopyHandlerAsync()
    {
        TopLevel.GetTopLevel(YamlEditor)?.Clipboard?.SetTextAsync(YamlEditor.Text);
    }
}