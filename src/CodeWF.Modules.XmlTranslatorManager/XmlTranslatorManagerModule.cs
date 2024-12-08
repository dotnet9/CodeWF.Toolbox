using CodeWF.Core;
using CodeWF.Core.Models;
using CodeWF.Modules.XmlTranslatorManager.Views;

namespace CodeWF.Modules.XmlTranslatorManager;

public class XmlTranslatorManagerModule : IModule
{
    public XmlTranslatorManagerModule(IToolMenuService toolMenuService)
    {
        var groupName = Localization.XmlTranslatorManager.Title;
        toolMenuService.AddSeparator();
        toolMenuService.AddGroup(groupName, Icons.XmlTranslatorManager);
        toolMenuService.AddItem(Localization.MergeXMLFilesView.Title, groupName, Localization.MergeXMLFilesView.Description,
            nameof(MergeXMLFilesView),
            Icons.MergeXMLFiles,
            ToolStatus.Developing);
        toolMenuService.AddItem(Localization.ManageXMLFilesView.Title, groupName, Localization.ManageXMLFilesView.Description, nameof(ManageXMLFilesView),
            Icons.ManageXMLFiles,
            ToolStatus.Developing);
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
        IRegionManager? regionManager = containerProvider.Resolve<IRegionManager>();
        regionManager.RegisterViewWithRegion<MergeXMLFilesView>(RegionNames.ContentRegion);
        regionManager.RegisterViewWithRegion<ManageXMLFilesView>(RegionNames.ContentRegion);
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
    }
}