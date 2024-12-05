using CodeWF.Core;
using CodeWF.Core.Models;
using CodeWF.Toolbox.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Navigation.Regions;
using Ursa.PrismExtension;

namespace CodeWF.Toolbox;

public class MainModule : IModule
{
    public MainModule(IToolMenuService toolMenuService)
    {
        toolMenuService.AddItem(Localization.DashboardView.Title, parentName: null, null, nameof(DashboardView), Icons.Dashboard,
            ToolStatus.Developing);
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
        IRegionManager? regionManager = containerProvider.Resolve<IRegionManager>();
        regionManager.RegisterViewWithRegion<DashboardView>(RegionNames.ContentRegion);
        regionManager.RegisterViewWithRegion<CommonSettingView>(RegionNames.SettingRegion);
        regionManager.RegisterViewWithRegion<AboutView>(RegionNames.SettingRegion);
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterUrsaDialogView<SettingView>(DialogNames.Setting);
    }
}