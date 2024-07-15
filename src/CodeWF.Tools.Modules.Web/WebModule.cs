using Prism.Regions;

namespace CodeWF.Tools.Modules.Web;

public class WebModule : IModule
{
    public WebModule(IToolManagerService toolManagerService)
    {
        toolManagerService.AddTool(ToolType.Web, WebToolInfo.SlugifyName, WebToolInfo.SlugifyDescription,
            nameof(SlugifyView),
            IconHelper.SlugifyName,
            ToolStatus.Complete);

        toolManagerService.AddTool(ToolType.Web, WebToolInfo.IPQueryName, WebToolInfo.IPQueryDescription,
            nameof(IPQueryView),
            IconHelper.IPQuery,
            ToolStatus.Complete);
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
        IRegionManager? regionManager = containerProvider.Resolve<IRegionManager>();
        regionManager.RegisterViewWithRegion<SlugifyView>(RegionNames.ContentRegion);
        regionManager.RegisterViewWithRegion<IPQueryView>(RegionNames.ContentRegion);
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.Register<ITranslationService, TranslationService>();

        containerRegistry.Register<IIPQueryService, IPTaoBaoQueryService>();
        containerRegistry.Register<IIPQueryService, IPBaiDuQueryService>();
        containerRegistry.Register<IIPQueryService, IPTencentQueryService>();
        containerRegistry.Register<IIPQueryService, IPAMapQueryService>();
        containerRegistry.Register<IIPQueryService, IPPCOnlineQueryService>();

        containerRegistry.RegisterSingleton(typeof(SlugifyViewModel));
        containerRegistry.RegisterSingleton(typeof(IPQueryViewModel));
    }
}