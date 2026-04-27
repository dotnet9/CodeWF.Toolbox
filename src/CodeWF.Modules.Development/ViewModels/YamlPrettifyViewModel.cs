using Avalonia.Controls;
using AvaloniaEdit;
using CodeWF.Tools.Extensions;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;

namespace CodeWF.Modules.Development.ViewModels;

public class YamlPrettifyViewModel : ReactiveObject
{
    public TextEditor? YamlTextEditor { get; set; }

    public YamlPrettifyViewModel()
    {
        RaiseCopyCommand = ReactiveCommand.CreateFromTask(RaiseCopyHandlerAsync);

        this.WhenAnyValue(x => x.RawYaml)
            .Throttle(TimeSpan.FromMilliseconds(400))
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Subscribe(RawYamlChanged);
    }

    public string? RawYaml
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ErrorMessage
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public ReactiveCommand<Unit, Unit> RaiseCopyCommand { get; }

    private void RawYamlChanged(string? newRawYaml)
    {
        if (YamlTextEditor == null)
        {
            return;
        }

        if (newRawYaml.YamlPrettify(out var newYaml, out var errorMessage))
        {
            YamlTextEditor.Text = newYaml;
            ErrorMessage = default;
        }
        else
        {
            YamlTextEditor.Text = string.Empty;
            ErrorMessage = errorMessage;
        }
    }

    private async Task RaiseCopyHandlerAsync()
    {
        await (TopLevel.GetTopLevel(YamlTextEditor)?.Clipboard?.SetTextAsync(YamlTextEditor?.Text) ?? Task.CompletedTask);
    }
}
