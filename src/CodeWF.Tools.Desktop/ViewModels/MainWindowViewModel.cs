namespace CodeWF.Tools.Desktop.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        Title = AppInfo.AppInfo.ToolName;
    }
}