using CodeWF.LogViewer.Avalonia.Log4Net;
using CodeWF.Tools.Helpers;

namespace CodeWF.Tools.Desktop.Views;

public partial class MainView : UserControl
{
    private const string ThemeKey = "Theme";

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
        LoadTheme();
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
        ChangeTheme();
    }

    private void LoadTheme()
    {
        var defaultTheme = AppConfigHelper.GetEntryAssembly().Get<string>(ThemeKey);

        Application? app = Application.Current;
        if (app is null)
        {
            return;
        }

        app.RequestedThemeVariant =
            string.Compare(defaultTheme, nameof(ThemeVariant.Dark), StringComparison.OrdinalIgnoreCase) == 0
                ? ThemeVariant.Dark
                : ThemeVariant.Light;
        LogFactory.Instance.Log.Info($"当前主题为{app.RequestedThemeVariant}");
    }

    private void ChangeTheme()
    {
        Application? app = Application.Current;
        if (app is null)
        {
            return;
        }

        ThemeVariant theme = app.ActualThemeVariant;
        app.RequestedThemeVariant = theme == ThemeVariant.Dark ? ThemeVariant.Light : ThemeVariant.Dark;
        AppConfigHelper.GetEntryAssembly().Set(ThemeKey, app.RequestedThemeVariant.ToString());
        LogFactory.Instance.Log.Info($"切换主题为{theme}");
    }
}