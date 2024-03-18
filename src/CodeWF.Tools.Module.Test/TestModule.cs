namespace CodeWF.Tools.Module.Test;

public class TestModule : IModule
{
    public TestModule(IToolManagerService toolManagerService)
    {
        toolManagerService.AddTool(ToolType.Test, TestToolInfo.MessageTestName,
            TestToolInfo.MessageTestDescription, nameof(MessageTestView),
            IconHelper.MessageTest, ToolStatus.Complete);
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
        IRegionManager? regionManager = containerProvider.Resolve<IRegionManager>();
        regionManager.RegisterViewWithRegion<MessageTestView>(RegionNames.ContentRegion);
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton(typeof(MessageTestViewModel));
    }
}