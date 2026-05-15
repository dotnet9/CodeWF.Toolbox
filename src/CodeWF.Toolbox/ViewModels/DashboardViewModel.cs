using CodeWF.Core;
using CodeWF.Core.IServices;
using CodeWF.Core.Models;
using CodeWF.Toolbox.Commands;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;

namespace CodeWF.Toolbox.ViewModels;

public class DashboardViewModel : ViewModelBase
{
    private readonly IToolMenuService _toolMenuService;
    private readonly IUserProfileService _userProfileService;

    public DashboardViewModel(IToolMenuService toolMenuService, IUserProfileService userProfileService)
    {
        _toolMenuService = toolMenuService;
        _userProfileService = userProfileService;
        _toolMenuService.ToolMenuChanged += RefreshMenuMetrics;
        _userProfileService.ProfileChanged += (_, _) => RefreshFrequentTools();
        OpenFrequentToolCommand = ReactiveCommand.Create<UserToolUsage>(OpenFrequentTool);

        OSInfo = GetPlatformName();
        RefreshMenuMetrics();
        RefreshFrequentTools();
    }

    public ObservableCollection<UserToolUsage> FrequentTools { get; } = [];

    public bool HasFrequentTools
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public ReactiveCommand<UserToolUsage, Unit> OpenFrequentToolCommand { get; }

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

    private void RefreshFrequentTools()
    {
        FrequentTools.Clear();
        foreach (var item in _userProfileService.FrequentTools)
        {
            FrequentTools.Add(item);
        }

        HasFrequentTools = FrequentTools.Count > 0;
    }

    private static void OpenFrequentTool(UserToolUsage tool)
    {
        if (!string.IsNullOrWhiteSpace(tool.ViewName))
        {
            EventBus.EventBus.Default.Publish(new OpenToolMenuCommand(tool.ViewName));
        }
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
