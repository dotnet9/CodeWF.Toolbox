using CodeWF.Tools.EventBus.Commands;
using Prism.Regions;

namespace CodeWF.Tools.Desktop.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly IEventBus _eventBus;
    private readonly INotificationService _notificationService;
    private readonly IRegionManager _regionManager;
    private ToolMenuItem? _searchSelectedItem;

    private ToolMenuItem? _selectedMenuItem;

    private NotificationType _selectedMenuStatus;

    public MainViewModel(IToolManagerService toolManagerService, INotificationService notificationService,
        IEventBus eventBus, IRegionManager regionManager)
    {
        Title = AppInfo.AppInfo.ToolName;
        _notificationService = notificationService;
        _eventBus = eventBus;
        _regionManager = regionManager;
        SearchMenuItems = new ObservableCollection<ToolMenuItem>();
        MenuItems = toolManagerService.MenuItems;
        toolManagerService.ToolMenuChanged += MenuChangedHandler;

        _eventBus.Subscribe(this);
    }

    public ObservableCollection<ToolMenuItem> SearchMenuItems { get; set; }

    public ToolMenuItem? SearchSelectedItem
    {
        get => _searchSelectedItem;
        set
        {
            this.RaiseAndSetIfChanged(ref _searchSelectedItem, value);
            ChangeSearchMenu();
        }
    }

    public ToolMenuItem? SelectedMenuItem
    {
        get => _selectedMenuItem;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedMenuItem, value);
            ChangeTool();
        }
    }

    public NotificationType SelectedMenuStatus
    {
        get => _selectedMenuStatus;
        set => this.RaiseAndSetIfChanged(ref _selectedMenuStatus, value);
    }

    public ObservableCollection<ToolMenuItem> MenuItems { get; }

    [EventHandler]
    public void ReceiveTestMessage(TestCommand message)
    {
        _notificationService?.Show("CodeWF EventBus",
            $"主工程收到{nameof(TestCommand)}，Name: {message.Name}, Time: {message.CurrentTime}");
    }

    [EventHandler]
    public void ReceiveChangeToolMessage(ChangeToolCommand message)
    {
        ChangeSearchMenu(message.ToolHeader!);
    }

    private void MenuChangedHandler(object sender, EventArgs e)
    {
        SearchMenuItems.Clear();
        MenuItems.ForEach(firstMenuItem =>
        {
            if (firstMenuItem.Children.Any())
            {
                firstMenuItem.Children.ForEach(secondMenuItem => SearchMenuItems.Add(secondMenuItem));
            }
        });
        SelectedMenuItem = SelectedMenuItem == null ? MenuItems.First() : GetMenuItem(SelectedMenuItem.Header!);
    }

    private ToolMenuItem? GetMenuItem(string name)
    {
        foreach (ToolMenuItem firstMenuItem in MenuItems)
        {
            if (name == firstMenuItem.Header)
            {
                return firstMenuItem;
            }

            foreach (ToolMenuItem secondMenuItem in firstMenuItem.Children)
            {
                if (name == secondMenuItem.Header)
                {
                    return secondMenuItem;
                }
            }
        }

        return default;
    }

    private void ChangeTool()
    {
        _regionManager.RequestNavigate(RegionNames.ContentRegion, _selectedMenuItem?.ViewName);
        SelectedMenuStatus = _selectedMenuItem?.Status switch
        {
            ToolStatus.Planned => NotificationType.Warning,
            ToolStatus.Developing => NotificationType.Information,
            ToolStatus.Complete => NotificationType.Success,
            _ => NotificationType.Information
        };
    }

    private void ChangeSearchMenu()
    {
        ChangeSearchMenu(_searchSelectedItem!.Header!);
    }

    private void ChangeSearchMenu(string name)
    {
        foreach (ToolMenuItem firstMenuItem in MenuItems)
        {
            foreach (ToolMenuItem secondMenuItem in firstMenuItem.Children)
            {
                if (secondMenuItem.Header != name)
                {
                    continue;
                }

                SelectedMenuItem = secondMenuItem;
                return;
            }
        }
    }
}