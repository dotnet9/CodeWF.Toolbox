using CodeWF.Core;
using CodeWF.Core.Models;
using CodeWF.Modules.Development.I18n;
using CodeWF.Modules.Development.Views;

namespace CodeWF.Modules.Development;

public class DevelopmentModule : IModule
{
    public DevelopmentModule(IToolMenuService toolMenuService)
    {
        var groupName = Language.Development;
        toolMenuService.AddSeparator();
        toolMenuService.AddGroup(groupName, Icons.Development);
        toolMenuService.AddItem(Language.YamlPrettify, groupName, Language.YamlPrettifyDescription, nameof(YamlPrettifyView),
            Icons.Yaml,
            ToolStatus.Complete);
        toolMenuService.AddItem(Language.JsonPrettify, groupName, Language.JsonPrettifyDescription, nameof(JsonPrettifyView),
            Icons.Json,
            ToolStatus.Complete);
        toolMenuService.AddItem("Test", groupName, "Test", nameof(TestView),
            Icons.Json,
            ToolStatus.Complete);
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