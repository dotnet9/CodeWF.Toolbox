using Prism.Regions;

namespace CodeWF.Tools.Module.Image;

public class ImageModule : IModule
{
    public ImageModule(IToolManagerService toolManagerService)
    {
        toolManagerService.AddTool(ToolType.Image, ImageToolInfo.IconConverterName,
            ImageToolInfo.IconConverterDescription, nameof(IconConverterView), IconHelper.IconConverter,
            ToolStatus.Developing);
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
        IRegionManager? regionManager = containerProvider.Resolve<IRegionManager>();
        regionManager.RegisterViewWithRegion<IconConverterView>(RegionNames.ContentRegion);
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton(typeof(GifToImagesViewModel));
    }
}