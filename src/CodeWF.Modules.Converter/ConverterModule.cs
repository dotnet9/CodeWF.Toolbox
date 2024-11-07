using CodeWF.Core;
using CodeWF.Core.Models;
using CodeWF.Modules.Converter.I18n;
using CodeWF.Modules.Converter.Views;

namespace CodeWF.Modules.Converter;

public class ConverterModule : IModule
{
    public ConverterModule(IToolMenuService toolMenuService)
    {
        var groupName = Language.Converter;
        toolMenuService.AddSeparator();
        toolMenuService.AddGroup(groupName, Icons.Converter);
        toolMenuService.AddItem(Language.DateTimeConverter, groupName, Language.DateTimeFormatConversion,
            nameof(DateTimeConverterView),
            Icons.Timestamp,
            ToolStatus.Developing);
        toolMenuService.AddItem(Language.YamlToJson, groupName, Language.YamlToJsonDescription, nameof(YamlToJsonView),
            Icons.Yaml,
            ToolStatus.Complete);
        toolMenuService.AddItem(Language.JsonToYaml, groupName, Language.JsonToYamlDescription, nameof(JsonToYamlView),
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