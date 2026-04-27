using CodeWF.Core;
using CodeWF.Core.Models;
using CodeWF.Modules.LogViewer.ViewModels;
using CodeWF.Modules.LogViewer.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;

namespace CodeWF.Modules.LogViewer;

public class LogViewerModule : IModule
{
    public LogViewerModule(IToolMenuService toolMenuService)
    {
        var groupName = Localization.LogViewerModule.Title;
        toolMenuService.AddSeparator();
        toolMenuService.AddGroup(groupName, Icons.LogViewer);
        toolMenuService.AddItem(Localization.LogViewerView.Title, groupName,
            Localization.LogViewerView.Description,
            nameof(LogViewerView),
            Icons.LogFile,
            ToolStatus.Complete);
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
        IRegionManager? regionManager = containerProvider.Resolve<IRegionManager>();
        regionManager.RegisterViewWithRegion<LogViewerView>(RegionNames.ContentRegion);
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        ViewModelLocationProvider.Register<LogViewerView, LogViewerViewModel>();
    }
}
