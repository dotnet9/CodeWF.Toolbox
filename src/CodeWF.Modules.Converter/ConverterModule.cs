using CodeWF.Core;
using CodeWF.Core.Models;
using CodeWF.Modules.Converter.ViewModels;
using CodeWF.Modules.Converter.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;

namespace CodeWF.Modules.Converter;

public class ConverterModule : IModule
{
    public ConverterModule(IToolMenuService toolMenuService)
    {
        var groupName = Localization.ConverterModule.Title;
        toolMenuService.AddSeparator();
        toolMenuService.AddGroup(groupName, Icons.Converter);
        toolMenuService.AddItem(Localization.DateTimeConverterView.Title, groupName,
            Localization.DateTimeConverterView.Description,
            nameof(DateTimeConverterView),
            Icons.Timestamp,
            ToolStatus.Complete);
        toolMenuService.AddItem(Localization.YamlToJsonView.Title, groupName, Localization.YamlToJsonView.Description,
            nameof(YamlToJsonView),
            Icons.Yaml,
            ToolStatus.Complete);
        toolMenuService.AddItem(Localization.JsonToYamlView.Title, groupName, Localization.JsonToYamlView.Description,
            nameof(JsonToYamlView),
            Icons.Json,
            ToolStatus.Complete);
        toolMenuService.AddItem(Localization.ImageToIconView.Title, groupName,
            Localization.ImageToIconView.MemoContent1, nameof(ImageToIconView),
            Icons.Icon,
            ToolStatus.Complete);
        toolMenuService.AddItem(Localization.NuoCheView.Title, groupName,
            Localization.NuoCheView.Description, nameof(NuoCheView),
            Icons.Icon,
            ToolStatus.Complete);
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
        IRegionManager? regionManager = containerProvider.Resolve<IRegionManager>();
        regionManager.RegisterViewWithRegion<DateTimeConverterView>(RegionNames.ContentRegion);
        regionManager.RegisterViewWithRegion<YamlToJsonView>(RegionNames.ContentRegion);
        regionManager.RegisterViewWithRegion<JsonToYamlView>(RegionNames.ContentRegion);
        regionManager.RegisterViewWithRegion<ImageToIconView>(RegionNames.ContentRegion);
        regionManager.RegisterViewWithRegion<NuoCheView>(RegionNames.ContentRegion);
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        ViewModelLocationProvider.Register<ImageToIconView, ImageToIconViewModel>();
        ViewModelLocationProvider.Register<DateTimeConverterView, DateTimeConverterViewModel>();
        ViewModelLocationProvider.Register<NuoCheView, NuoCheViewModel>();
    }
}