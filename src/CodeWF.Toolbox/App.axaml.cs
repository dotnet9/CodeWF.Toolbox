using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CodeWF.Core;
using CodeWF.Core.IServices;
using CodeWF.Core.RegionAdapters;
using CodeWF.Core.Services;
using CodeWF.Modules.AvaloniaXmlTranslatorManager;
using CodeWF.Toolbox.Services;
using CodeWF.Toolbox.Views;
using DryIoc;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Navigation.Regions;
using System;
using System.Linq;
using Ursa.PrismExtension;
using AIModule = CodeWF.Modules.AI.AIModule;
using ConverterModule = CodeWF.Modules.Converter.ConverterModule;
using DevelopmentModule = CodeWF.Modules.Development.DevelopmentModule;
using MainWindow = CodeWF.Toolbox.Views.MainWindow;

namespace CodeWF.Toolbox;

public partial class App : PrismApplication
{
    public static bool IsSingleViewLifetime =>
        Environment.GetCommandLineArgs()
            .Any(a => a == "--fbdev" || a == "--drm");

    public static App Instance { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        base.Initialize(); // <-- Required
    }

    protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
    {
        moduleCatalog.AddModule<MainModule>();
        moduleCatalog.AddModule<AIModule>();
        moduleCatalog.AddModule<ConverterModule>();
        moduleCatalog.AddModule<AvaloniaXmlTranslatorManagerModule>();
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
        if (IsSingleViewLifetime)
            return Container.Resolve<MainView>();
        return Container.Resolve<MainWindow>();
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterUrsaDialogService();

        containerRegistry.RegisterSingleton<IApplicationService, ApplicationService>();
        containerRegistry.RegisterSingleton<IToolMenuService, ToolMenuService>();
        containerRegistry.RegisterSingleton<IFileChooserService, FileChooserService>();
        containerRegistry.RegisterSingleton<INotificationService, NotificationService>();

        containerRegistry.Register<MainWindow>();
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
            OpenUrlAsync(desktop.MainWindow, "https://github.com/dotnet9/CodeWF.Toolbox");
        }
    }

    private void ExitApplication_OnClicked(object? sender, EventArgs e)
    {
        Environment.Exit(0);
    }

    private async void OpenUrlAsync(Visual? owner, string uri)
    {
        var top = TopLevel.GetTopLevel(owner);
        if (top is null) return;
        var launcher = top.Launcher;
        await launcher.LaunchUriAsync(new Uri(uri));
    }
}