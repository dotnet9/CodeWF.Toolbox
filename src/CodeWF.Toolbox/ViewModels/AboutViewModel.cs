using CodeWF.Core.RegionAdapters;
using CodeWF.Tools.Extensions;
using System.Reflection;

namespace CodeWF.Toolbox.ViewModels;

public class AboutViewModel : ViewModelBase, ITabItemBase
{
    public string? TitleKey { get; set; } = Localization.AboutView.Title;
    public string? AppName { get; set; }
    public string? Version { get; set; } = Assembly.GetExecutingAssembly().Version();
    public string? MessageKey { get; set; } = Localization.AboutView.Description;

    public string? CompileTime { get; set; } =
        Assembly.GetExecutingAssembly().CompileTime()?.ToString("yyyy-MM-dd HH:mm:ss");
}