using CodeWF.Core;
using CodeWF.Core.IServices;
using CodeWF.Core.Models;
using CodeWF.Toolbox.Commands;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reactive;

namespace CodeWF.Toolbox.ViewModels;

public class DashboardViewModel : ViewModelBase
{
    private static readonly string[] RecommendedToolViewNames =
    [
        "DateTimeConverterView",
        "Base64CodecView",
        "GuidGeneratorView",
        "ImageToIconView",
        "ToolView?tool=json-viewer",
        "ToolView?tool=jwt-parser",
        "ToolView?tool=qr-code-generator",
        "ToolView?tool=hash-text"
    ];

    private readonly IToolMenuService _toolMenuService;
    private readonly IUserProfileService _userProfileService;

    public DashboardViewModel(IToolMenuService toolMenuService, IUserProfileService userProfileService)
    {
        _toolMenuService = toolMenuService;
        _userProfileService = userProfileService;
        _toolMenuService.ToolMenuChanged += RefreshDashboard;
        _userProfileService.ProfileChanged += (_, _) => RefreshDashboardTools();
        OpenDashboardToolCommand = ReactiveCommand.Create<DashboardToolItem>(OpenDashboardTool);

        OSInfo = GetPlatformName();
        RefreshDashboard();
    }

    public ObservableCollection<DashboardToolItem> DashboardTools { get; } = [];

    public bool HasDashboardTools
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string DashboardToolsTitleKey
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = Localization.DashboardView.FrequentToolsTitle;

    public ReactiveCommand<DashboardToolItem, Unit> OpenDashboardToolCommand { get; }

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

    private void RefreshDashboard()
    {
        RefreshMenuMetrics();
        RefreshDashboardTools();
    }

    private void RefreshDashboardTools()
    {
        DashboardTools.Clear();
        var frequentTools = _userProfileService.FrequentTools
            .Where(item => !string.IsNullOrWhiteSpace(item.ViewName))
            .Take(8)
            .ToList();

        foreach (var item in frequentTools)
        {
            DashboardTools.Add(DashboardToolItem.FromUsage(item));
        }

        DashboardToolsTitleKey = frequentTools.Count switch
        {
            0 => Localization.DashboardView.RecommendedToolsTitle,
            >= 8 => Localization.DashboardView.FrequentToolsTitle,
            _ => "Localization.DashboardView.QuickToolsTitle"
        };

        var navigableItems = Flatten(_toolMenuService.MenuItems)
            .Where(item => !item.IsSeparator && !string.IsNullOrWhiteSpace(item.ViewName))
            .ToList();

        for (var index = 0; index < RecommendedToolViewNames.Length; index++)
        {
            if (DashboardTools.Count >= 8)
            {
                break;
            }

            var viewName = RecommendedToolViewNames[index];
            if (DashboardTools.Any(item => string.Equals(item.ViewName, viewName, System.StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            var menuItem = navigableItems.FirstOrDefault(item =>
                string.Equals(item.ViewName, viewName, System.StringComparison.OrdinalIgnoreCase));
            if (menuItem is null)
            {
                continue;
            }

            DashboardTools.Add(DashboardToolItem.FromMenuItem(
                menuItem,
                (DashboardTools.Count + 1).ToString(CultureInfo.InvariantCulture)));
        }

        HasDashboardTools = DashboardTools.Count > 0;
    }

    private static void OpenDashboardTool(DashboardToolItem tool)
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

public sealed class DashboardToolItem
{
    public string ViewName { get; init; } = string.Empty;

    public string? Name { get; init; }

    public string? Description { get; init; }

    public string? Icon { get; init; }

    public string Badge { get; init; } = string.Empty;

    public static DashboardToolItem FromUsage(UserToolUsage item)
    {
        return new DashboardToolItem
        {
            ViewName = item.ViewName,
            Name = item.Name,
            Description = item.Description,
            Icon = item.Icon,
            Badge = item.Count.ToString(CultureInfo.InvariantCulture)
        };
    }

    public static DashboardToolItem FromMenuItem(ToolMenuItem item, string badge)
    {
        return new DashboardToolItem
        {
            ViewName = item.ViewName ?? string.Empty,
            Name = item.Name,
            Description = item.Description,
            Icon = item.Icon,
            Badge = badge
        };
    }
}
