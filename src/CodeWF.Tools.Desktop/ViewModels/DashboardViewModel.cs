namespace CodeWF.Tools.Desktop.ViewModels;

public class DashboardViewModel : ViewModelBase
{
    private readonly IMessenger _messenger;
    private readonly IToolManagerService _toolManagerService;

    public DashboardViewModel(IToolManagerService toolManagerService, IMessenger messenger)
    {
        _toolManagerService = toolManagerService;
        _messenger = messenger;
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
        _messenger.Publish(this, new ChangeToolMessage(this, menuItem.Header));
    }
}