using CodeWF.Tools.Core.IServices;
using CodeWF.Tools.EventBus.Commands;

namespace CodeWF.Tools.Desktop.Views;

public partial class MainView : UserControl
{
    private bool _isChangeTheme;

    public MainView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        var theme = ContainerLocator.Current.Resolve<ISystemService>().LoadTheme();
        UpdateThemeSwitch(theme);
        ContainerLocator.Current.Resolve<IEventBus>().Subscribe<UpdateThemeCommand>(ChangeTheme);

        TopLevel? level = TopLevel.GetTopLevel(this);
        if (level == null)
        {
            return;
        }

        var notificationService = ContainerLocator.Current.Resolve<INotificationService>();
        var fileChooserService = ContainerLocator.Current.Resolve<IFileChooserService>();
        notificationService.SetHostWindow(level);
        fileChooserService.SetHostWindow(level);
    }

    private void ChangeTheme(UpdateThemeCommand command)
    {
        UpdateThemeSwitch(command.CurrentTheme);
    }

    private void UpdateThemeSwitch(ThemeVariant theme)
    {
        _isChangeTheme = true;
        this.FindControl<ToggleSwitch>("ToggleSwitchTheme")!.IsChecked = theme == ThemeVariant.Dark;
        _isChangeTheme = false;
    }

    private void ToggleButton_OnIsCheckedChanged(object sender, RoutedEventArgs e)
    {
        if (_isChangeTheme) return;

        ContainerLocator.Current.Resolve<ISystemService>().ChangeTheme();
    }
}