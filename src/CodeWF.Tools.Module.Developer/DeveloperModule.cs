using Prism.Regions;

namespace CodeWF.Tools.Module.Developer;

public class DeveloperModule : IModule
{
    public DeveloperModule(IToolManagerService toolManagerService)
    {
        toolManagerService.AddTool(ToolType.Developer, DeveloperToolInfo.TimestampName,
            DeveloperToolInfo.TimestampTitle, nameof(TimestampView), IconHelper.Timestamp, ToolStatus.Developing);
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
        IRegionManager? regionManager = containerProvider.Resolve<IRegionManager>();
        regionManager.RegisterViewWithRegion<TimestampView>(RegionNames.ContentRegion);
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton(typeof(TimestampViewModel));
    }
}