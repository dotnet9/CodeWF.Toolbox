using CodeWF.LogViewer.Avalonia.Log4Net;
using CodeWF.Tools.Core.IServices;
using CodeWF.Tools.EventBus.Commands;
using CodeWF.Tools.Helpers;

namespace CodeWF.Tools.Desktop.Services;

public class SystemService(IEventBus eventBus) : ISystemService
{
    private readonly IEventBus _eventBus = eventBus;
    private const string ThemeKey = "Theme";

    public ThemeVariant LoadTheme()
    {
        var defaultTheme = AppConfigHelper.GetEntryAssembly().Get<string>(ThemeKey);

        Application? app = Application.Current;
        if (app is null)
        {
            return ThemeVariant.Default;
        }

        app.RequestedThemeVariant =
            string.Compare(defaultTheme, nameof(ThemeVariant.Dark), StringComparison.OrdinalIgnoreCase) == 0
                ? ThemeVariant.Dark
                : ThemeVariant.Light;
        LogFactory.Instance.Log.Info($"当前主题为{app.RequestedThemeVariant}");
        return app.RequestedThemeVariant;
    }

    public void ChangeTheme(ThemeVariant theme)
    {
        Application? app = Application.Current;
        if (app is null)
        {
            return;
        }

        app.RequestedThemeVariant = theme;
        AppConfigHelper.GetEntryAssembly().Set(ThemeKey, app.RequestedThemeVariant.ToString());
        LogFactory.Instance.Log.Info($"切换主题为{app.RequestedThemeVariant}");
        _eventBus.Publish(new UpdateThemeCommand(app.RequestedThemeVariant));
    }
    public void ChangeTheme()
    {
        Application? app = Application.Current;
        if (app is null)
        {
            return;
        }

        ThemeVariant theme = app.ActualThemeVariant;
        app.RequestedThemeVariant = theme == ThemeVariant.Dark ? ThemeVariant.Light : ThemeVariant.Dark;
        AppConfigHelper.GetEntryAssembly().Set(ThemeKey, app.RequestedThemeVariant.ToString());
        LogFactory.Instance.Log.Info($"切换主题为{app.RequestedThemeVariant}");
        _eventBus.Publish(new UpdateThemeCommand(app.RequestedThemeVariant));
    }
}