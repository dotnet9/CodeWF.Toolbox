using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using CodeWF.Core;
using CodeWF.Core.IServices;
using CodeWF.Core.Models;
using CodeWF.EventBus;
using CodeWF.Toolbox.Commands;
using CodeWF.Toolbox.Views;
using Lang.Avalonia;
using Prism.Ioc;
using Prism.Regions;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

namespace CodeWF.Toolbox.ViewModels;

internal class MainMenuViewModel : ViewModelBase
{
    private readonly IRegionManager _regionManager;
    private readonly IToolMenuService _toolMenuService;
    private readonly IUserProfileService _userProfileService;
    private bool _isSyncingGroupSelection;
    private string _searchKeyword = string.Empty;

    public ObservableCollection<ToolMenuItem> MenuItems { get; } = [];
    public ObservableCollection<ToolMenuItem> GroupItems { get; } = [];
    public ReactiveCommand<Unit, Unit> OpenSettingCommand { get; }

    private ObservableCollection<ToolMenuItem> _activeMenuItems = [];

    public ObservableCollection<ToolMenuItem> ActiveMenuItems
    {
        get => _activeMenuItems;
        private set => this.RaiseAndSetIfChanged(ref _activeMenuItems, value);
    }

    private ToolMenuItem? _selectedMenuItem;

    public ToolMenuItem? SelectedMenuItem
    {
        get => _selectedMenuItem;
        set
        {
            if (ReferenceEquals(_selectedMenuItem, value))
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _selectedMenuItem, value);
            ChangeTool();
        }
    }

    private ToolMenuItem? _selectedGroupItem;

    public ToolMenuItem? SelectedGroupItem
    {
        get => _selectedGroupItem;
        set
        {
            if (ReferenceEquals(_selectedGroupItem, value))
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _selectedGroupItem, value);
            RefreshActiveMenuItems();

            if (!_isSyncingGroupSelection)
            {
                SelectFirstToolInGroup(value);
            }
        }
    }

    private NotificationType _selectedMenuStatus;

    public NotificationType SelectedMenuStatus
    {
        get => _selectedMenuStatus;
        set => this.RaiseAndSetIfChanged(ref _selectedMenuStatus, value);
    }

    internal MainMenuViewModel(
        IRegionManager regionManager,
        IToolMenuService toolMenuService,
        IUserProfileService userProfileService)
    {
        _regionManager = regionManager;
        _toolMenuService = toolMenuService;
        _userProfileService = userProfileService;

        _toolMenuService.ToolMenuChanged += MenuChangedHandler;
        EventBus.EventBus.Default.Subscribe(this);
        OpenSettingCommand = ReactiveCommand.CreateFromTask(RaiseOpenSettingHandlerAsync);

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

    [EventHandler]
    private void OpenToolMenuHandler(OpenToolMenuCommand command)
    {
        var target = FindMenuItemByViewName(MenuItems, command.ViewName);
        if (target != null)
        {
            SelectedMenuItem = target;
        }
    }

    private void ApplyMenuFilter(string? preferredName = null, string? preferredViewName = null)
    {
        MenuItems.Clear();
        GroupItems.Clear();
        ActiveMenuItems = [];

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

        foreach (ToolMenuItem item in MenuItems.Where(item => !item.IsSeparator))
        {
            GroupItems.Add(item);
        }

        var preferredItem =
            FindMenuItem(MenuItems, preferredName, preferredViewName)
            ?? FindFirstNavigableItem(MenuItems, skipDashboard: string.IsNullOrWhiteSpace(_searchKeyword))
            ?? FindFirstNavigableItem(MenuItems);
        SetSelectedGroupFromMenu(preferredItem);
        SelectedMenuItem = preferredItem;
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

    private static ToolMenuItem? FindMenuItemByViewName(ObservableCollection<ToolMenuItem> items, string viewName)
    {
        foreach (ToolMenuItem item in items)
        {
            if (string.Equals(item.ViewName, viewName, System.StringComparison.OrdinalIgnoreCase))
            {
                return item;
            }

            var child = FindMenuItemByViewName(item.Children, viewName);
            if (child != null)
            {
                return child;
            }
        }

        return null;
    }

    private static ToolMenuItem? FindFirstNavigableItem(ObservableCollection<ToolMenuItem> items, bool skipDashboard = false)
    {
        foreach (ToolMenuItem item in items)
        {
            if (!item.IsSeparator
                && !string.IsNullOrWhiteSpace(item.ViewName)
                && (!skipDashboard || item.ViewName != nameof(DashboardView)))
            {
                return item;
            }

            var child = FindFirstNavigableItem(item.Children, skipDashboard);
            if (child != null)
            {
                return child;
            }
        }

        return null;
    }

    private void RefreshActiveMenuItems()
    {
        if (SelectedGroupItem == null)
        {
            ActiveMenuItems = [];
            return;
        }

        IEnumerable<ToolMenuItem> sourceItems = SelectedGroupItem.Children.Count > 0
            ? SelectedGroupItem.Children
            : [SelectedGroupItem];

        ActiveMenuItems = new ObservableCollection<ToolMenuItem>(
            sourceItems.Where(item => !item.IsSeparator));
    }

    private void SelectFirstToolInGroup(ToolMenuItem? group)
    {
        if (group == null)
        {
            return;
        }

        // 点击左侧类别图标时，自动进入该类别下第一个可用工具，让内容区保持聚焦。
        var target = !string.IsNullOrWhiteSpace(group.ViewName)
            ? group
            : FindFirstNavigableItem(group.Children);
        if (target != null)
        {
            SelectedMenuItem = target;
        }
    }

    private void SetSelectedGroupFromMenu(ToolMenuItem? item)
    {
        var group = FindOwningTopLevel(MenuItems, item) ?? GroupItems.FirstOrDefault();
        _isSyncingGroupSelection = true;
        try
        {
            SelectedGroupItem = group;
        }
        finally
        {
            _isSyncingGroupSelection = false;
        }
    }

    private static ToolMenuItem? FindOwningTopLevel(ObservableCollection<ToolMenuItem> groups, ToolMenuItem? item)
    {
        if (item == null)
        {
            return null;
        }

        foreach (ToolMenuItem group in groups)
        {
            if (ReferenceEquals(group, item) || ContainsMenuItem(group.Children, item))
            {
                return group;
            }
        }

        return null;
    }

    private static bool ContainsMenuItem(ObservableCollection<ToolMenuItem> items, ToolMenuItem item)
    {
        foreach (ToolMenuItem current in items)
        {
            if (ReferenceEquals(current, item) || ContainsMenuItem(current.Children, item))
            {
                return true;
            }
        }

        return false;
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
        SetSelectedGroupFromMenu(_selectedMenuItem);
        _regionManager.RequestNavigate(RegionNames.ContentRegion, _selectedMenuItem.ViewName);
        _userProfileService.RecordToolUsage(_selectedMenuItem);
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
