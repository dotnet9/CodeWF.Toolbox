using Avalonia;
using CodeWF.Toolbox.Diagnostics;
using ReactiveUI.Avalonia;
using System;
using System.Threading.Tasks;

namespace CodeWF.Toolbox;

internal sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        StartupDiagnostics.LogStartupEnvironment();
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is Exception exception)
            {
                StartupDiagnostics.LogException("AppDomain.UnhandledException", exception);
                return;
            }

            StartupDiagnostics.Log($"AppDomain.UnhandledException: {e.ExceptionObject}");
        };
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            StartupDiagnostics.LogException("TaskScheduler.UnobservedTaskException", e.Exception);
        };

        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception exception)
        {
            StartupDiagnostics.LogException("Program.Main", exception);
            throw;
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .With(new Win32PlatformOptions
            {
                RenderingMode =
                [
                    Win32RenderingMode.AngleEgl,
                    Win32RenderingMode.Wgl,
                    Win32RenderingMode.Software
                ],
                OverlayPopups = true,
            })
            .With(new X11PlatformOptions
            {
                OverlayPopups = true
            })
            .With(new AvaloniaNativePlatformOptions
            {
                OverlayPopups = true
            })
            .WithBundledAppFont()
            .UseReactiveUI(_ => { })
            .LogToTrace();
}