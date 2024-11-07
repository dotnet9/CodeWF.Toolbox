using Avalonia;
using Avalonia.Markup.Xaml;
using CodeWF.Core;
using CodeWF.Core.IServices;
using CodeWF.Core.Services;
using CodeWF.Modules.AI;
using CodeWF.Modules.Converter;
using CodeWF.Modules.Development;
using CodeWF.Toolbox.Services;
using CodeWF.Toolbox.Views;
using DryIoc;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Modularity;
using System;
using System.Linq;
using Ursa.PrismExtension;

namespace CodeWF.Toolbox;

public partial class App : PrismApplication
{
    public static bool IsSingleViewLifetime =>
        Environment.GetCommandLineArgs()
            .Any(a => a == "--fbdev" || a == "--drm");

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
        moduleCatalog.AddModule<DevelopmentModule>();
        base.ConfigureModuleCatalog(moduleCatalog);
    }

    protected override AvaloniaObject CreateShell()
    {
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
}