using CodeWF.Core;
using CodeWF.Core.Models;
using CodeWF.Modules.Development.I18n;
using CodeWF.Modules.Development.ViewModels;
using CodeWF.Modules.Development.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace CodeWF.Modules.Development;

public class DevelopmentModule : IModule
{
    public DevelopmentModule(IToolMenuService toolMenuService)
    {
        var groupName = Language.Development;
        toolMenuService.AddSeparator();
        toolMenuService.AddGroup(groupName, Icons.Converter);
        toolMenuService.AddItem(Language.YamlPrettify, groupName, Language.YamlPrettifyDescription, nameof(YamlPrettifyView),
            Icons.Timestamp,
            ToolStatus.Developing);
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
        IRegionManager? regionManager = containerProvider.Resolve<IRegionManager>();
        regionManager.RegisterViewWithRegion<YamlPrettifyView>(RegionNames.ContentRegion);
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton(typeof(YamlPrettifyViewModel));
    }
}