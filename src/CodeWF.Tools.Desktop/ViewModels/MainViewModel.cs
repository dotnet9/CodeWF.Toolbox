namespace CodeWF.Tools.Desktop.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly IEventAggregator _eventAggregator;
    private readonly INotificationService _notificationService;
    private readonly IRegionManager _regionManager;
    private ToolMenuItem? _searchSelectedItem;

    private ToolMenuItem? _selectedMenuItem;

    private NotificationType _selectedMenuStatus;

    public MainViewModel(IToolManagerService toolManagerService, INotificationService notificationService,
        IEventAggregator eventAggregator, IRegionManager regionManager)
    {
        Title = AppInfo.AppInfo.ToolName;
        _notificationService = notificationService;
        _eventAggregator = eventAggregator;
        _regionManager = regionManager;
        RegisterPrismEvent();
        SearchMenuItems = new ObservableCollection<ToolMenuItem>();
        MenuItems = toolManagerService.MenuItems;
        toolManagerService.ToolMenuChanged += MenuChangedHandler;
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

    private void RegisterPrismEvent()
    {
        _eventAggregator.GetEvent<TestEvent>().Subscribe(args =>
        {
            _notificationService?.Show("Prism Event",
                $"主工程Prism事件处理程序：Args = {args.Args}, Now = {DateTime.Now}");
        });
        _eventAggregator.GetEvent<ChangeToolEvent>().Subscribe(args =>
        {
            ChangeSearchMenu(args.ToolHeader!);
        });
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