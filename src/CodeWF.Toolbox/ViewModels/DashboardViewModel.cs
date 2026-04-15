using ReactiveUI;

namespace CodeWF.Toolbox.ViewModels;

public class DashboardViewModel : ViewModelBase
{
    public DashboardViewModel()
    {
#if LINUX_X64
        OSInfo = "Linux X64";
#elif LINUX_ARM64
        OSInfo = "Linux ARM64";
#elif WIN_X64
        OSInfo = "Windows X64";
#elif WIN_X86
        OSInfo = "Windows X86";
#else
        OSInfo = "Unknown";
#endif
    }

    public string OSInfo
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
}