using ReactiveUI;

namespace CodeWF.Toolbox.ViewModels;

public class ExitOptionViewModel : ViewModelBase
{
    private string? _message;

    public string? Message
    {
        get => _message;
        set => this.RaiseAndSetIfChanged(ref _message, value);
    }


    private string? _optionContent;

    public string? OptionContent
    {
        get => _optionContent;
        set => this.RaiseAndSetIfChanged(ref _optionContent, value);
    }


    private bool _option;

    public bool Option
    {
        get => _option;
        set => this.RaiseAndSetIfChanged(ref _option, value);
    }
}