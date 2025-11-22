using Irihi.Avalonia.Shared.Contracts;
using ReactiveUI;
using System;
using System.Threading.Tasks;

namespace CodeWF.Toolbox.ViewModels;

public class ExitOptionViewModel : ViewModelBase, IDialogContext
{

    public bool HideTrayIconOnClose
    {
        get ;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool NeedExitDialogOnClose
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
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