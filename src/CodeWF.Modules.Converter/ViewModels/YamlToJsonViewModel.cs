using Avalonia.Controls;
using AvaloniaEdit;
using CodeWF.Tools.Extensions;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;

namespace CodeWF.Modules.Converter.ViewModels;

public class YamlToJsonViewModel : ReactiveObject
{
    public TextEditor? JsonEditor { get; set; }
    public TextEditor? YamlEditor { get; set; }

    public YamlToJsonViewModel()
    {
        RaiseClearCommand = ReactiveCommand.CreateFromTask(RaiseClearHandlerAsync);
        RaiseCopyCommand = ReactiveCommand.CreateFromTask(RaiseCopyHandlerAsync);
    }

    public void StartListen()
    {
        this.WhenAnyValue(x => x.YamlString)
            .Throttle(TimeSpan.FromMilliseconds(400))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => YamlChanged());
        YamlEditor!.TextChanged += (s, e) => YamlString = YamlEditor.Text;
    }

    private string? _yamlString;

    public string? YamlString
    {
        get => _yamlString;
        set => this.RaiseAndSetIfChanged(ref _yamlString, value);
    }


    private string? _errorMessage;

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    public ReactiveCommand<Unit, Unit> RaiseClearCommand { get; }
    public ReactiveCommand<Unit, Unit> RaiseCopyCommand { get; }

    private void YamlChanged()
    {
        if (JsonEditor == null || YamlEditor == null)
        {
            return;
        }

        if (YamlString.YamlToJson(out var jsonString, out string? errorMsg))
        {
            JsonEditor.Text = jsonString;
            ErrorMessage = default;
        }
        else
        {
            JsonEditor.Text = default;
            ErrorMessage = errorMsg;
        }
    }

    private async Task RaiseClearHandlerAsync()
    {
        YamlEditor?.Clear();
    }

    private async Task RaiseCopyHandlerAsync()
    {
        TopLevel.GetTopLevel(JsonEditor)?.Clipboard?.SetTextAsync(JsonEditor.Text);
    }
}