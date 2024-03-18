namespace CodeWF.Tools.Desktop.ViewModels;

public class DashboardViewModel : ViewModelBase
{
    private readonly IEventAggregator _eventAggregator;
    private readonly IToolManagerService _toolManagerService;

    public DashboardViewModel(IToolManagerService toolManagerService, IEventAggregator eventAggregator)
    {
        _toolManagerService = toolManagerService;
        _eventAggregator = eventAggregator;
        toolManagerService.ToolMenuChanged += MenuChangedHandler;
    }

    public ObservableCollection<ToolMenuItem> MenuItems { get; } = new();

    private void MenuChangedHandler(object sender, EventArgs e)
    {
        MenuItems.Clear();
        _toolManagerService.MenuItems.ForEach(firstMenuItem =>
        {
            if (firstMenuItem.Children.Any())
            {
                MenuItems.Add(firstMenuItem);
            }
        });
    }

    public void ExecuteChangeToolHandle(ToolMenuItem menuItem)
    {
        _eventAggregator.GetEvent<ChangeToolEvent>()
            .Publish(new ChangeToolEventParameter { ToolHeader = menuItem.Header });
    }
}