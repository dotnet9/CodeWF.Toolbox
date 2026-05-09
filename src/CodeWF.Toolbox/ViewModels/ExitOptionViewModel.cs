using Irihi.Avalonia.Shared.Contracts;
using ReactiveUI;
using System;
using System.Threading.Tasks;

namespace CodeWF.Toolbox.ViewModels;

public class ExitOptionViewModel : ViewModelBase, IDialogContext
{
    private bool _hideTrayIconOnClose;
    private bool _needExitDialogOnClose;

    public bool HideTrayIconOnClose
    {
        get => _hideTrayIconOnClose;
        set
        {
            this.RaiseAndSetIfChanged(ref _hideTrayIconOnClose, value);
            this.RaisePropertyChanged(nameof(DirectToClose));
        }
    }

    public bool DirectToClose
    {
        get => !HideTrayIconOnClose;
        set
        {
            if (value)
            {
                HideTrayIconOnClose = false;
            }
        }
    }

    public bool NeedExitDialogOnClose
    {
        get => _needExitDialogOnClose;
        set
        {
            this.RaiseAndSetIfChanged(ref _needExitDialogOnClose, value);
            this.RaisePropertyChanged(nameof(RememberMyChoice));
        }
    }

    public bool RememberMyChoice
    {
        get => !NeedExitDialogOnClose;
        set => NeedExitDialogOnClose = !value;
    }

    public void Close()
    {
        RequestClose?.Invoke(this, null);
    }

    public async Task RaiseCloseHandlerAsync()
    {
        Close();
    }

    public event EventHandler<object?>? RequestClose;
}