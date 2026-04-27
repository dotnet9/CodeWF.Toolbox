using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CodeWF.Core;
using CodeWF.Core.IServices;
using CodeWF.Core.RegionAdapters;
using CodeWF.Core.Services;
using CodeWF.Modules.XmlTranslatorManager;
using CodeWF.Toolbox.Services;
using CodeWF.Toolbox.ViewModels;
using CodeWF.Toolbox.Views;
using DryIoc;
using Lang.Avalonia;
using Lang.Avalonia.Xml;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Ursa.PrismExtension;
using AIModule = CodeWF.Modules.AI.AIModule;
using ConverterModule = CodeWF.Modules.Converter.ConverterModule;
using DevelopmentModule = CodeWF.Modules.Development.DevelopmentModule;

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
        AvaloniaXamlLoader.Load(this);
        I18nManager.Instance.Register(new XmlLangPlugin(), new CultureInfo("zh-CN"), out _);
        base.Initialize(); // <-- Required
    }

    protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
    {
        moduleCatalog.AddModule<MainModule>();
        moduleCatalog.AddModule<AIModule>();
        moduleCatalog.AddModule<ConverterModule>();
        moduleCatalog.AddModule<XmlTranslatorManagerModule>();
        moduleCatalog.AddModule<DevelopmentModule>();
        base.ConfigureModuleCatalog(moduleCatalog);
    }

    protected override void ConfigureRegionAdapterMappings(RegionAdapterMappings regionAdapterMappings)
    {
        base.ConfigureRegionAdapterMappings(regionAdapterMappings);
        regionAdapterMappings.RegisterMapping(typeof(TabControl), Container.Resolve<TabControlRegionAdapter>());
    }

    protected override AvaloniaObject CreateShell()
    {
        Instance = this;

        var mainWindow = Container.Resolve<MainWindow>();
        return mainWindow;
    }

    protected override async void OnInitialized()
    {
        base.OnInitialized();

        // Prism Shell 创建完成后再弹出登录窗口，避免登录窗口找不到 Owner。
        var loginWindow = Container.Resolve<LoginWindow>();
        if (MainWindow is Window owner)
        {
            await loginWindow.ShowDialog(owner);
            return;
        }

        loginWindow.Show();
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
