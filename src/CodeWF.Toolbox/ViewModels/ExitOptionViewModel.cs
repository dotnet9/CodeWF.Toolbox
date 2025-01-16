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

    private bool _hideTrayIconOnClose;

    public bool HideTrayIconOnClose
    {
        get => _hideTrayIconOnClose;
        set => this.RaiseAndSetIfChanged(ref _hideTrayIconOnClose, value);
    }
    
    private bool _needExitDialogOnClose;

    public bool NeedExitDialogOnClose
    {
        get => _needExitDialogOnClose;
        set => this.RaiseAndSetIfChanged(ref _needExitDialogOnClose, value);
    }
}