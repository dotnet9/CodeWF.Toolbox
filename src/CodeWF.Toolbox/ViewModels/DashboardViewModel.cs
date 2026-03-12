using CodeWF.Core.Helpers;
using ReactiveUI;
using System;

namespace CodeWF.Toolbox.ViewModels;

public class DashboardViewModel : ViewModelBase
{
    public DashboardViewModel()
    {
#if WIN64
        OSInfo = "Windows 64";
#elif WIN32
        OSInfo = "Windows 32";
#elif LINUX64
        OSInfo = "Linux 64";
#else
        throw new NotImplementedException("不支持的平台");
#endif
    }
        
    public string OSInfo
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
}