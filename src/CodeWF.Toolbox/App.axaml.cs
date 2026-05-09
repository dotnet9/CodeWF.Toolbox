using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CodeWF.Core;
using CodeWF.Core.IServices;
using CodeWF.Core.RegionAdapters;
using CodeWF.Core.Services;
using CodeWF.Modules.XmlTranslatorManager;
using CodeWF.Toolbox.Diagnostics;
using CodeWF.Toolbox.Services;
using CodeWF.Toolbox.ViewModels;
using CodeWF.Toolbox.Views;
using DryIoc;
using Lang.Avalonia;
using Lang.Avalonia.Json;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ursa.PrismExtension;
using AIModule = CodeWF.Modules.AI.AIModule;
using ConverterModule = CodeWF.Modules.Converter.ConverterModule;
using DataModule = CodeWF.Modules.Data.DataModule;
using DevelopmentModule = CodeWF.Modules.Development.DevelopmentModule;
using LogViewerModule = CodeWF.Modules.LogViewer.LogViewerModule;
using MathModule = CodeWF.Modules.Math.MathModule;
using MeasurementModule = CodeWF.Modules.Measurement.MeasurementModule;
using MediaModule = CodeWF.Modules.Media.MediaModule;
using NetworkModule = CodeWF.Modules.Network.NetworkModule;
using SecurityModule = CodeWF.Modules.Security.SecurityModule;
using TextModule = CodeWF.Modules.Text.TextModule;
using ToolFrameworkModule = CodeWF.Modules.ToolFramework.ToolFrameworkModule;
using WebModule = CodeWF.Modules.Web.WebModule;

namespace CodeWF.Toolbox;

public partial class App : PrismApplication
{
    public static bool IsSingleViewLifetime =>
        Environment.GetCommandLineArgs()
            .Any(a => a == "--fbdev" || a == "--drm");

    public static App Instance { get; private set; } = null!;
    public static bool IsLoggedIn { get; set; } = false;

    public override void Initialize()
    {
        try
        {
            StartupDiagnostics.Log("App.Initialize: loading application XAML.");
            AvaloniaXamlLoader.Load(this);
            var langPlugin = new JsonLangPlugin
            {
                ResourceFolder = Path.Combine(AppContext.BaseDirectory, "I18n")
            };
            I18nManager.Instance.Register(langPlugin, new CultureInfo("zh-CN"), out _);
            StartupDiagnostics.Log("App.Initialize: calling Prism base initialization.");
            base.Initialize(); // <-- Required
            StartupDiagnostics.Log("App.Initialize: completed.");
        }
        catch (Exception exception)
        {
            StartupDiagnostics.LogException("App.Initialize", exception);
            throw;
        }
    }

    protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
    {
        moduleCatalog.AddModule<MainModule>();
        moduleCatalog.AddModule<ToolFrameworkModule>();
        moduleCatalog.AddModule<AIModule>();
        moduleCatalog.AddModule<ConverterModule>();
        moduleCatalog.AddModule<DevelopmentModule>();
        moduleCatalog.AddModule<SecurityModule>();
        moduleCatalog.AddModule<WebModule>();
        moduleCatalog.AddModule<MediaModule>();
        moduleCatalog.AddModule<NetworkModule>();
        moduleCatalog.AddModule<MathModule>();
        moduleCatalog.AddModule<MeasurementModule>();
        moduleCatalog.AddModule<TextModule>();
        moduleCatalog.AddModule<DataModule>();
        moduleCatalog.AddModule<LogViewerModule>();
        moduleCatalog.AddModule<XmlTranslatorManagerModule>();
        base.ConfigureModuleCatalog(moduleCatalog);
    }

    protected override void ConfigureRegionAdapterMappings(RegionAdapterMappings regionAdapterMappings)
    {
        base.ConfigureRegionAdapterMappings(regionAdapterMappings);
        regionAdapterMappings.RegisterMapping(typeof(TabControl), Container.Resolve<TabControlRegionAdapter>());
    }

    protected override AvaloniaObject CreateShell()
    {
        try
        {
            Instance = this;

            StartupDiagnostics.Log("App.CreateShell: resolving MainWindow.");
            var mainWindow = Container.Resolve<MainWindow>();
            StartupDiagnostics.Log("App.CreateShell: MainWindow resolved.");
            return mainWindow;
        }
        catch (Exception exception)
        {
            StartupDiagnostics.LogException("App.CreateShell", exception);
            throw;
        }
    }

    protected override async void OnInitialized()
    {
        try
        {
            base.OnInitialized();

            // Prism Shell 创建完成后再弹出登录窗口，避免登录窗口找不到 Owner。
            StartupDiagnostics.Log("App.OnInitialized: resolving LoginWindow.");
            var loginWindow = Container.Resolve<LoginWindow>();
            StartupDiagnostics.Log("App.OnInitialized: LoginWindow resolved.");
            if (MainWindow is Window owner)
            {
                await loginWindow.ShowDialog(owner);
                return;
            }

            loginWindow.Show();
        }
        catch (Exception exception)
        {
            StartupDiagnostics.LogException("App.OnInitialized", exception);
            throw;
        }
    }
   

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterUrsaDialogService();

        containerRegistry.RegisterSingleton<IApplicationService, ApplicationService>();
        containerRegistry.RegisterSingleton<IToolMenuService, ToolMenuService>();
        containerRegistry.RegisterSingleton<IFileChooserService, FileChooserService>();
        containerRegistry.RegisterSingleton<INotificationService, NotificationService>();
        containerRegistry.RegisterSingleton<ILoginService, LoginService>();

        containerRegistry.RegisterSingleton<LoginViewModel>();
        containerRegistry.Register<MainWindow>();
        containerRegistry.Register<LoginWindow>();
    }

    private void OpenMainWindow_OnClicked(object? sender, EventArgs e)
    {
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return;
        }

        desktop.MainWindow?.Show();
        desktop.MainWindow?.Activate();
    }

    private void OpenGithub_OnClicked(object? sender, EventArgs e)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _ = OpenUrlAsync(desktop.MainWindow, "https://github.com/dotnet9/CodeWF.Toolbox");
        }
    }

    private void ExitApplication_OnClicked(object? sender, EventArgs e)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
            return;
        }

        Environment.Exit(0);
    }

    private static async Task OpenUrlAsync(Visual? owner, string uri)
    {
        var top = TopLevel.GetTopLevel(owner);
        if (top is null) return;
        var launcher = top.Launcher;
        await launcher.LaunchUriAsync(new Uri(uri));
    }
}
