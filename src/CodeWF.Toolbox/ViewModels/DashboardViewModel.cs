using CodeWF.Core;
using CodeWF.Core.Models;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;

namespace CodeWF.Toolbox.ViewModels;

public class DashboardViewModel : ViewModelBase
{
    private readonly IToolMenuService _toolMenuService;

    public DashboardViewModel(IToolMenuService toolMenuService)
    {
        _toolMenuService = toolMenuService;
        _toolMenuService.ToolMenuChanged += RefreshMenuMetrics;

        OSInfo = GetPlatformName();
        RefreshMenuMetrics();
    }

    public int ModuleCount
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public int ToolCount
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string OSInfo
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    private void RefreshMenuMetrics()
    {
        // 首页只展示真实可导航工具数量，分组与分隔符不计入工具数。
        var menuItems = _toolMenuService.MenuItems;
        ModuleCount = menuItems.Count(item => !item.IsSeparator && item.Children.Count > 0);
        ToolCount = Flatten(menuItems)
            .Count(item => !item.IsSeparator && !string.IsNullOrWhiteSpace(item.ViewName));
    }

    private static IEnumerable<ToolMenuItem> Flatten(IEnumerable<ToolMenuItem> items)
    {
        foreach (var item in items)
        {
            yield return item;

            foreach (var child in Flatten(item.Children))
            {
                yield return child;
            }
        }
    }

    private static string GetPlatformName()
    {
#if PLATFORM_LINUX_X64
        return "Linux x64";
#elif PLATFORM_LINUX_ARM64
        return "Linux ARM64";
#elif PLATFORM_WIN_X64
        return "Windows x64";
#elif PLATFORM_WIN_X86
        return "Windows x86";
#else
        return "Unknown";
#endif
    }
}
