namespace CodeWF.Tools.Desktop.Views;

public partial class MainView : UserControl
{
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

    private void ToggleButton_OnIsCheckedChanged(object sender, RoutedEventArgs e)
    {
        Application? app = Application.Current;
        if (app is not null)
        {
            ThemeVariant theme = app.ActualThemeVariant;
            app.RequestedThemeVariant = theme == ThemeVariant.Dark ? ThemeVariant.Light : ThemeVariant.Dark;
        }
    }
}