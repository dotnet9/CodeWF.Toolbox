using CodeWF.Core;
using CodeWF.Core.Models;
using CodeWF.Modules.Development.Views;
using CodeWF.Modules.ToolFramework.Services;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace CodeWF.Modules.Development;

public class DevelopmentModule : IModule
{
    public DevelopmentModule(IToolMenuService toolMenuService)
    {
        var groupName = Localization.DevelopmentModule.Title;
        toolMenuService.AddSeparator();
        toolMenuService.AddGroup(groupName, Icons.Development);
        toolMenuService.AddItem(Localization.YamlPrettifyView.Title, groupName,
            Localization.YamlPrettifyView.Description, nameof(YamlPrettifyView),
            Icons.Yaml,
            ToolStatus.Complete);
        toolMenuService.AddItem(Localization.JsonPrettifyView.Title, groupName,
            Localization.JsonPrettifyView.Description, nameof(JsonPrettifyView),
            Icons.Json,
            ToolStatus.Complete);
        ToolMenuRegistrar.RegisterCategory(
            toolMenuService,
            ToolCategories.Development,
            groupName,
            Icons.Development);
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
        IRegionManager? regionManager = containerProvider.Resolve<IRegionManager>();
        regionManager.RegisterViewWithRegion<YamlPrettifyView>(RegionNames.ContentRegion);
        regionManager.RegisterViewWithRegion<JsonPrettifyView>(RegionNames.ContentRegion);
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
    }
}

