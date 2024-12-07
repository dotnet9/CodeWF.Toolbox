using CodeWF.Core;
using CodeWF.Core.Models;
using CodeWF.Modules.Development.Views;

namespace CodeWF.Modules.Development;

public class DevelopmentModule : IModule
{
    public DevelopmentModule(IToolMenuService toolMenuService)
    {
        var groupName = Localization.DevelopmentModule.Title;
        toolMenuService.AddSeparator();
        toolMenuService.AddGroup(groupName, Icons.Development);
        toolMenuService.AddItem(Localization.YamlPrettifyView.Title, groupName, Localization.YamlPrettifyView.Description, nameof(YamlPrettifyView),
            Icons.Yaml,
            ToolStatus.Complete);
        toolMenuService.AddItem(Localization.JsonPrettifyView.Title, groupName, Localization.JsonPrettifyView.Description, nameof(JsonPrettifyView),
            Icons.Json,
            ToolStatus.Complete);
        toolMenuService.AddItem("Test", groupName, "Test", nameof(TestView),
            Icons.Json,
            ToolStatus.Developing);
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
        IRegionManager? regionManager = containerProvider.Resolve<IRegionManager>();
        regionManager.RegisterViewWithRegion<YamlPrettifyView>(RegionNames.ContentRegion);
        regionManager.RegisterViewWithRegion<JsonPrettifyView>(RegionNames.ContentRegion);
        regionManager.RegisterViewWithRegion<TestView>(RegionNames.ContentRegion);
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
    }
}