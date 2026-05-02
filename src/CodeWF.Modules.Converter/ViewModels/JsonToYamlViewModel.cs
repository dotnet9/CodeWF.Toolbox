using Avalonia.Controls;
using AvaloniaEdit;
using CodeWF.Core.Helpers;
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
        if (JsonEditor == null)
        {
            return;
        }

        this.WhenAnyValue(x => x.JsonString)
            .Throttle(TimeSpan.FromMilliseconds(400))
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Subscribe(_ => JsonChanged());
        JsonEditor.TextChanged += (_, _) => JsonString = JsonEditor.Text;
    }

    public string? JsonString
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ErrorMessage
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
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
        await ClipboardHelper.SetTextAsync(YamlEditor, YamlEditor?.Text);
    }
}
