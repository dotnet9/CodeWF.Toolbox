using Irihi.Avalonia.Shared.Contracts;
using ReactiveUI;
using System;

namespace CodeWF.Toolbox.ViewModels;

public class ExitOptionViewModel : ViewModelBase, IDialogContext
{
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

    public void Close()
    {
        RequestClose?.Invoke(this, null);
    }

    public void RaiseCloseHandler()
    {
        Close();
    }

    public event EventHandler<object?>? RequestClose;
}