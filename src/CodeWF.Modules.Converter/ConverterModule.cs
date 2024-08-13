using CodeWF.Core;
using CodeWF.Core.Models;
using CodeWF.Modules.Converter.ViewModels;
using CodeWF.Modules.Converter.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace CodeWF.Modules.Converter;

public class ConverterModule : IModule
{
    public ConverterModule(IToolMenuService toolMenuService)
    {
        var groupName = CultureNames.Converter;
        toolMenuService.AddSeparator();
        toolMenuService.AddGroup(groupName, Icons.Timestamp);
        toolMenuService.AddItem(CultureNames.DateTimeConverter, groupName, CultureNames.DateTimeFormatConversion, nameof(DateTimeConverterView),
            Icons.Timestamp,
            ToolStatus.Developing);
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
        IRegionManager? regionManager = containerProvider.Resolve<IRegionManager>();
        regionManager.RegisterViewWithRegion<DateTimeConverterView>(RegionNames.ContentRegion);
        //LogFactory.Instance.Log.Info("转换器模块初始化完成");
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton(typeof(DateTimeConverterViewModel));
    }
}