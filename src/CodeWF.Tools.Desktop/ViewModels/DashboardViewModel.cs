using CodeWF.Tools.EventBus.Commands;

namespace CodeWF.Tools.Desktop.ViewModels;

public class DashboardViewModel : ViewModelBase
{
    private readonly IEventBus _eventBus;
    private readonly IToolManagerService _toolManagerService;

    public DashboardViewModel(IToolManagerService toolManagerService, IEventBus eventBus)
    {
        _toolManagerService = toolManagerService;
        _eventBus = eventBus;
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
        _eventBus.Publish(new ChangeToolCommand(menuItem.Header));
    }
}