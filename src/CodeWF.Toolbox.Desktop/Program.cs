using Avalonia;
using ReactiveUI.Avalonia;
using System;

namespace CodeWF.Toolbox.Desktop;

internal sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .With(new Win32PlatformOptions() 
            {
                RenderingMode = [Win32RenderingMode.Software],   // 软渲染
                OverlayPopups = true,
            })   
            .With(new X11PlatformOptions 
            {
                OverlayPopups = true
            })
            .With(new AvaloniaNativePlatformOptions
            { 
                OverlayPopups=true
            })
            .WithFont_SourceHanSansCN()
            .UseReactiveUI(_ => { })
            .LogToTrace();
}