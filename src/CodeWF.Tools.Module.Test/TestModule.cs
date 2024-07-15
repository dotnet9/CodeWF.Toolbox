using CodeWF.Tools.Module.Test.Dialogs;
using Prism.Regions;
using Ursa.PrismExtension;

namespace CodeWF.Tools.Module.Test;

public class TestModule : IModule
{
    public TestModule(IToolManagerService toolManagerService)
    {
        toolManagerService.AddTool(ToolType.Test, TestToolInfo.MessageTestName,
            TestToolInfo.MessageTestDescription, nameof(EventBusTestView),
            IconHelper.MessageTest, ToolStatus.Complete);
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
        IRegionManager? regionManager = containerProvider.Resolve<IRegionManager>();
        regionManager.RegisterViewWithRegion<EventBusTestView>(RegionNames.ContentRegion);
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton(typeof(EventBusTestViewModel));
        containerRegistry.RegisterUrsaDialogView<SettingsDialog>("TestModuleSettings");
    }
}