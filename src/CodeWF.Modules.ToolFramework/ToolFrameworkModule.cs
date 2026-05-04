using CodeWF.Core;
using CodeWF.Modules.ToolFramework.Services;
using CodeWF.Modules.ToolFramework.ViewModels;
using CodeWF.Modules.ToolFramework.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;

namespace CodeWF.Modules.ToolFramework;

public sealed class ToolFrameworkModule : IModule
{
    public void OnInitialized(IContainerProvider containerProvider)
    {
        var regionManager = containerProvider.Resolve<IRegionManager>();
        regionManager.RegisterViewWithRegion<ToolView>(RegionNames.ContentRegion);
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton<ToolRegistry>();
        ViewModelLocationProvider.Register<ToolView, ToolViewModel>();
    }
}



