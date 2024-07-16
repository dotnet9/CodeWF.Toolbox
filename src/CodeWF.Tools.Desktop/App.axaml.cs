using CodeWF.Tools.Core.IServices;
using CodeWF.Tools.Desktop.Dialogs;
using Prism.Regions;
using System.Reflection;
using Ursa.PrismExtension;

namespace CodeWF.Tools.Desktop;

public class App : PrismApplication
{
    private INotificationService? _notificationService;

    public App()
    {
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        _notificationService?.Show($"异常", "");
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        base.Initialize(); // <-- Required
    }

    protected override IModuleCatalog CreateModuleCatalog()
    {
        var modules = new List<IModuleCatalogItem>();
        foreach (var dir in Directory.GetDirectories("./Modules"))
        {
            var dirCategoryLog = new DirectoryModuleCatalog
            {
                ModulePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, dir)
            };
            dirCategoryLog.Initialize();
            modules.AddRange(dirCategoryLog.Items);
        }

        var categoryLog = new ModuleCatalog();
        foreach (var module in modules)
        {
            categoryLog.Items.Add(module);
        }

        return categoryLog;
    }

    protected override void ConfigureRegionAdapterMappings(RegionAdapterMappings regionAdapterMappings)
    {
        base.ConfigureRegionAdapterMappings(regionAdapterMappings);

        regionAdapterMappings.RegisterMapping(typeof(StackPanel), Container.Resolve<StackPanelRegionAdapter>());
        regionAdapterMappings.RegisterMapping(typeof(Grid), Container.Resolve<GridRegionAdapter>());
        regionAdapterMappings.RegisterMapping(typeof(TabControl), Container.Resolve<TabControlAdapter>());
    }

    protected override AvaloniaObject CreateShell()
    {
        return Container.Resolve<MainWindow>();
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        IContainer? container = containerRegistry.GetContainer();

        // Register EventBus
        //containerRegistry.AddEventBus();
        IOC.EventBus.EventBusExtensions.AddEventBus((t1,t2)=>containerRegistry.RegisterSingleton(t1, t2), 
            t=> containerRegistry.RegisterScoped(t));

        // Views - Generic
        containerRegistry.Register<MainWindow>();
        containerRegistry.RegisterUrsaDialogService();
        containerRegistry.RegisterUrsaDialogView<SettingsDialog>("MainSettings");

        IRegionManager? regionManager = container.Resolve<IRegionManager>();
        regionManager.RegisterViewWithRegion<DashboardView>(RegionNames.ContentRegion);
        regionManager.RegisterViewWithRegion<FooterView>(RegionNames.FooterRegion);

        containerRegistry.RegisterSingleton(typeof(FooterViewModel));
        containerRegistry.RegisterSingleton(typeof(DashboardViewModel));

        containerRegistry.RegisterSingleton<INotificationService, NotificationService>();
        containerRegistry.RegisterSingleton<IClipboardService, ClipboardService>();
        containerRegistry.RegisterSingleton<IToolManagerService, ToolManagerService>();
        containerRegistry.RegisterSingleton<IFileChooserService, FileChooserService>();
        containerRegistry.RegisterSingleton<ISystemService, SystemService>();

        IToolManagerService? toolManagerService = container.Resolve<IToolManagerService>();
        toolManagerService.AddTool("首页",
            "这是一个基于Avalonia UI + Prism框架打造的模块化跨平台工具平台，汇聚了众多开发实用小工具，目前已开发或即将开发的如编码解码、数据加密等，轻量且强大，开箱即用，助力开发者提升工作效率。",
            nameof(DashboardView),
            "M217.6 659.2c0-19.2-6.4-38.4-19.2-51.2s-32-25.6-51.2-25.6c-19.2 0-38.4 12.8-51.2 25.6-12.8 12.8-25.6 32-25.6 51.2 0 19.2 6.4 38.4 19.2 51.2s32 19.2 51.2 19.2c19.2 0 38.4-6.4 51.2-19.2s25.6-32 25.6-51.2z m108.8-256c0-19.2-6.4-38.4-19.2-51.2s-32-25.6-51.2-25.6c-19.2 0-38.4 6.4-51.2 19.2s-19.2 38.4-19.2 57.6c0 19.2 6.4 38.4 19.2 51.2 12.8 12.8 32 19.2 51.2 19.2 19.2 0 38.4-6.4 51.2-19.2s19.2-32 19.2-51.2zM576 678.4l57.6-217.6c0-12.8 0-19.2-6.4-25.6-6.4-12.8-12.8-19.2-19.2-19.2H576c-6.4 6.4-12.8 12.8-12.8 25.6l-57.6 217.6c-25.6 0-44.8 12.8-64 25.6-19.2 12.8-32 32-38.4 57.6-6.4 32-6.4 57.6 12.8 83.2 12.8 25.6 38.4 44.8 64 51.2s57.6 6.4 83.2-12.8c25.6-12.8 44.8-38.4 51.2-64 6.4-25.6 6.4-44.8-6.4-64 0-25.6-12.8-44.8-32-57.6z m377.6-19.2c0-19.2-6.4-38.4-19.2-51.2-12.8-12.8-32-19.2-51.2-19.2-19.2 0-38.4 6.4-51.2 19.2-12.8 12.8-19.2 32-19.2 51.2 0 19.2 6.4 38.4 19.2 51.2 12.8 12.8 32 19.2 51.2 19.2 19.2 0 38.4-6.4 51.2-19.2 6.4-12.8 19.2-32 19.2-51.2zM582.4 294.4c0-19.2-6.4-38.4-19.2-51.2-12.8-19.2-32-25.6-51.2-25.6-19.2 0-38.4 6.4-51.2 19.2-12.8 19.2-19.2 38.4-19.2 57.6 0 19.2 6.4 38.4 19.2 51.2 12.8 12.8 32 19.2 51.2 19.2 19.2 0 38.4-6.4 51.2-19.2 12.8-12.8 19.2-32 19.2-51.2z m256 108.8c0-19.2-6.4-38.4-19.2-51.2-12.8-12.8-32-19.2-51.2-19.2-19.2 0-38.4 6.4-51.2 19.2-12.8 12.8-19.2 32-19.2 51.2 0 19.2 6.4 38.4 19.2 51.2 12.8 12.8 32 19.2 51.2 19.2 19.2 0 38.4-6.4 51.2-19.2 12.8-12.8 19.2-32 19.2-51.2z m185.6 256c0 102.4-25.6 192-83.2 275.2-6.4 12.8-19.2 19.2-32 19.2H108.8c-12.8 0-25.6-6.4-32-19.2C25.6 851.2 0 755.2 0 659.2c0-70.4 12.8-134.4 38.4-198.4s64-115.2 108.8-166.4 102.4-83.2 166.4-108.8 128-38.4 198.4-38.4 134.4 12.8 198.4 38.4 115.2 64 166.4 108.8c44.8 44.8 83.2 102.4 108.8 166.4 25.6 64 38.4 128 38.4 198.4z",
            ToolStatus.Complete);
        _notificationService = container.Resolve<INotificationService>();

        // Use EventBus
        //container.UseEventBus();
        IOC.EventBus.EventBusExtensions.UseEventBus(t=> container.Resolve(t));
    }

    /// <summary>
    ///     1、DryIoc.Microsoft.DependencyInjection低版本可不要这个方法（5.1.0及以下）
    ///     2、高版本必须，否则会抛出异常：System.MissingMethodException:“Method not found: 'DryIoc.Rules
    ///     DryIoc.Rules.WithoutFastExpressionCompiler()'.”
    ///     参考issues：https://github.com/dadhi/DryIoc/issues/529
    /// </summary>
    /// <returns></returns>
    protected override Rules CreateContainerRules()
    {
        return Rules.Default.WithConcreteTypeDynamicRegistrations(reuse: Reuse.Transient)
            .With(Made.Of(FactoryMethod.ConstructorWithResolvableArguments))
            .WithFuncAndLazyWithoutRegistration()
            .WithTrackingDisposableTransients()
            //.WithoutFastExpressionCompiler()
            .WithFactorySelector(Rules.SelectLastRegisteredFactory());
    }
}