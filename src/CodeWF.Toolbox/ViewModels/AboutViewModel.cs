using CodeWF.Core.RegionAdapters;
using CodeWF.Tools.Extensions;
using System.Reflection;

namespace CodeWF.Toolbox.ViewModels;

public class AboutViewModel : ViewModelBase, ITabItemBase
{
    public string? TitleKey { get; set; } = Localization.AboutView.Title;
    public string? MessageKey { get; set; } = Localization.AboutView.Description;
    public string? AppName { get; set; }
    public string? Product { get; set; } = Assembly.GetExecutingAssembly().Product();
    public string? Version { get; set; } = Assembly.GetExecutingAssembly().Version();
    public string? Platform { get; set; }
    public string? Copyright { get; set; } = Assembly.GetExecutingAssembly().Copyright();

    public string? CompileTime { get; set; } =
        Assembly.GetExecutingAssembly().CompileTime()?.ToString("yyyy-MM-dd HH:mm:ss");

    public AboutViewModel()
    {
#if LINUX_X64
        Platform = "Linux X64";
#elif LINUX_ARM64
        Platform = "Linux ARM64";
#elif WIN_X64
        Platform = "Windows X64";
#elif WIN_X86
        Platform = "Windows X86";
#else
        Platform = "Unknown";
#endif
    }
}