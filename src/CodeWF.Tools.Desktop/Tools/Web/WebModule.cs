namespace CodeWF.Tools.Desktop.Tools.Web;

public class WebModule : IModule
{
    public WebModule(IToolManagerService toolManagerService)
    {
        toolManagerService.AddTool(ToolType.Web, WebToolInfo.SlugifyName, WebToolInfo.SlugifyDescription,
            nameof(SlugifyView),
            IconHelper.SlugifyName,
            ToolStatus.Complete);
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
        IRegionManager? regionManager = containerProvider.Resolve<IRegionManager>();
        regionManager.RegisterViewWithRegion<SlugifyView>(RegionNames.ContentRegion);
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.Register<ITranslationService, TranslationService>();

        containerRegistry.RegisterSingleton(typeof(SlugifyViewModel));
    }
}