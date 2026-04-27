using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using CodeWF.Core;
using CodeWF.EventBus;
using CodeWF.Core.Models;
using CodeWF.Toolbox.Commands;
using CodeWF.Toolbox.Views;
using Lang.Avalonia;
using Prism.Ioc;
using Prism.Regions;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace CodeWF.Toolbox.ViewModels;

internal class MainMenuViewModel : ViewModelBase
{
    private readonly IRegionManager _regionManager;
    private readonly IToolMenuService _toolMenuService;
    private string _searchKeyword = string.Empty;

    public ObservableCollection<ToolMenuItem> MenuItems { get; } = [];

    private ToolMenuItem? _selectedMenuItem;

    public ToolMenuItem? SelectedMenuItem
    {
        get => _selectedMenuItem;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedMenuItem, value);
            ChangeTool();
        }
    }

    private NotificationType _selectedMenuStatus;

    public NotificationType SelectedMenuStatus
    {
        get => _selectedMenuStatus;
        set => this.RaiseAndSetIfChanged(ref _selectedMenuStatus, value);
    }

    internal MainMenuViewModel(IRegionManager regionManager, IToolMenuService toolMenuService)
    {
        _regionManager = regionManager;
        _toolMenuService = toolMenuService;

        _toolMenuService.ToolMenuChanged += MenuChangedHandler;
        EventBus.EventBus.Default.Subscribe(this);

        ApplyMenuFilter();
    }

    private void MenuChangedHandler()
    {
        ApplyMenuFilter(SelectedMenuItem?.Name, SelectedMenuItem?.ViewName);
    }

    [EventHandler]
    private void SearchToolMenuHandler(SearchToolMenuCommand command)
    {
        _searchKeyword = command.Keyword.Trim();
        ApplyMenuFilter(SelectedMenuItem?.Name, SelectedMenuItem?.ViewName);
    }

    private void ApplyMenuFilter(string? preferredName = null, string? preferredViewName = null)
    {
        MenuItems.Clear();

        foreach (ToolMenuItem sourceItem in _toolMenuService.MenuItems)
        {
            ToolMenuItem? displayItem = string.IsNullOrWhiteSpace(_searchKeyword)
                ? sourceItem
                : FilterMenuItem(sourceItem, _searchKeyword);

            if (displayItem != null)
            {
                MenuItems.Add(displayItem);
            }
        }

        SelectedMenuItem =
            FindMenuItem(MenuItems, preferredName, preferredViewName)
            ?? FindFirstNavigableItem(MenuItems);
    }

    private static ToolMenuItem? FilterMenuItem(ToolMenuItem item, string keyword)
    {
        if (item.IsSeparator)
        {
            return null;
        }

        var isMatched = IsMatched(item, keyword);
        var matchedChildren = item.Children
            .Select(child => isMatched ? CloneMenuItem(child) : FilterMenuItem(child, keyword))
            .Where(child => child != null)
            .Cast<ToolMenuItem>()
            .ToList();

        if (!isMatched && matchedChildren.Count == 0)
        {
            return null;
        }

        var clone = CloneMenuItem(item);
        clone.Children.Clear();
        foreach (ToolMenuItem child in matchedChildren)
        {
            clone.Children.Add(child);
        }

        return clone;
    }

    private static ToolMenuItem CloneMenuItem(ToolMenuItem item)
    {
        var clone = new ToolMenuItem
        {
            Level = item.Level,
            Name = item.Name,
            Description = item.Description,
            ViewName = item.ViewName,
            Status = item.Status,
            Icon = item.Icon,
            IsSeparator = item.IsSeparator,
            Children = []
        };

        foreach (ToolMenuItem child in item.Children)
        {
            var childClone = CloneMenuItem(child);
            if (!childClone.IsSeparator)
            {
                clone.Children.Add(childClone);
            }
        }

        return clone;
    }

    private static bool IsMatched(ToolMenuItem item, string keyword)
    {
        return ContainsKeyword(item.Name, keyword)
               || ContainsKeyword(item.Description, keyword)
               || ContainsKeyword(item.ViewName, keyword);
    }

    private static bool ContainsKeyword(string? value, string keyword)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var displayText = ResolveDisplayText(value);
        return value.Contains(keyword, System.StringComparison.CurrentCultureIgnoreCase)
               || displayText.Contains(keyword, System.StringComparison.CurrentCultureIgnoreCase);
    }

    private static string ResolveDisplayText(string value)
    {
        try
        {
            return I18nManager.Instance.GetResource(value) ?? value;
        }
        catch
        {
            return value;
        }
    }

    private static ToolMenuItem? FindMenuItem(
        ObservableCollection<ToolMenuItem> items,
        string? name,
        string? viewName)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        foreach (ToolMenuItem item in items)
        {
            if (item.Name == name && item.ViewName == viewName)
            {
                return item;
            }

            var child = FindMenuItem(item.Children, name, viewName);
            if (child != null)
            {
                return child;
            }
        }

        return null;
    }

    private static ToolMenuItem? FindFirstNavigableItem(ObservableCollection<ToolMenuItem> items)
    {
        foreach (ToolMenuItem item in items)
        {
            if (!item.IsSeparator && !string.IsNullOrWhiteSpace(item.ViewName))
            {
                return item;
            }

            var child = FindFirstNavigableItem(item.Children);
            if (child != null)
            {
                return child;
            }
        }

        return null;
    }

    private void ChangeTool()
    {
        if (_selectedMenuItem == null
            || _selectedMenuItem.IsSeparator
            || string.IsNullOrWhiteSpace(_selectedMenuItem.ViewName))
        {
            return;
        }

        // 只有真正的工具菜单项才触发区域导航，分组与分隔符只承担结构展示职责。
        _regionManager.RequestNavigate(RegionNames.ContentRegion, _selectedMenuItem.ViewName);
        SelectedMenuStatus = _selectedMenuItem.Status switch
        {
            ToolStatus.Planned => NotificationType.Warning,
            ToolStatus.Developing => NotificationType.Information,
            ToolStatus.Complete => NotificationType.Success,
            _ => NotificationType.Information
        };
        EventBus.EventBus.Default.Publish(new ChangeToolMenuCommand(_selectedMenuItem));
    }

    public async Task RaiseOpenSettingHandlerAsync()
    {
        var settingView = ContainerLocator.Container.Resolve<SettingView>();
        if (App.Instance.MainWindow is Window owner)
        {
            await settingView.ShowDialog(owner);
            return;
        }

        settingView.Show();
    }
}
