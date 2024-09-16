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
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(RawYamlChanged);
    }

    private string? _rawYaml;

    public string? RawYaml
    {
        get => _rawYaml;
        set => this.RaiseAndSetIfChanged(ref _rawYaml, value);
    }

    private string? _errorMessage;

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    public ReactiveCommand<Unit, Unit> RaiseCopyCommand { get; }

    private void RawYamlChanged(string? newRawYaml)
    {
        if (RawYaml.YamlPrettify(out var newYaml, out var errorMessage))
        {
            YamlTextEditor!.Text = newYaml;
            ErrorMessage = default;
        }
        else
        {
            YamlTextEditor!.Text = string.Empty;
            ErrorMessage = errorMessage;
        }
    }

    private async Task RaiseCopyHandlerAsync()
    {
        TopLevel.GetTopLevel(YamlTextEditor)?.Clipboard?.SetTextAsync(YamlTextEditor.Text);
    }
}