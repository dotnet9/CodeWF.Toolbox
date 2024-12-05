using CodeWF.Core;
using CodeWF.Core.Models;
using CodeWF.Modules.Converter.Views;

namespace CodeWF.Modules.Converter;

public class ConverterModule : IModule
{
    public ConverterModule(IToolMenuService toolMenuService)
    {
        var groupName = Localization.ConverterModule.Title;
        toolMenuService.AddSeparator();
        toolMenuService.AddGroup(groupName, Icons.Converter);
        toolMenuService.AddItem(Localization.DateTimeConverterView.Title, groupName, Localization.DateTimeConverterView.Description,
            nameof(DateTimeConverterView),
            Icons.Timestamp,
            ToolStatus.Developing);
        toolMenuService.AddItem(Localization.YamlToJsonView.Title, groupName, Localization.YamlToJsonView.Description, nameof(YamlToJsonView),
            Icons.Yaml,
            ToolStatus.Complete);
        toolMenuService.AddItem(Localization.JsonToYamlView.Title, groupName, Localization.JsonToYamlView.Description, nameof(JsonToYamlView),
            Icons.Json,
            ToolStatus.Complete);
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
        IRegionManager? regionManager = containerProvider.Resolve<IRegionManager>();
        regionManager.RegisterViewWithRegion<DateTimeConverterView>(RegionNames.ContentRegion);
        regionManager.RegisterViewWithRegion<YamlToJsonView>(RegionNames.ContentRegion);
        regionManager.RegisterViewWithRegion<JsonToYamlView>(RegionNames.ContentRegion);
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
    }
}