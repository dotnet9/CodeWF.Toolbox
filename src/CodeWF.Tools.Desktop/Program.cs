namespace CodeWF.Tools.Desktop;

internal sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .With(new X11PlatformOptions { EnableMultiTouch = false, UseDBusMenu = true })
            .UseSkia()
            .UseReactiveUI()
            .WithInterFont()
            .LogToTrace();
    }
}