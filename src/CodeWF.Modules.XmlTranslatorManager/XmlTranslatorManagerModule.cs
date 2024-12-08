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
        toolMenuService.AddItem(Localization.MergeXmlFilesView.Title, groupName,
            Localization.MergeXmlFilesView.Description,
            nameof(MergeXmlFilesView),
            Icons.MergeXMLFiles,
            ToolStatus.Complete);
        toolMenuService.AddItem(Localization.ManageXmlFilesView.Title, groupName,
            Localization.ManageXmlFilesView.Description, nameof(ManageXmlFilesView),
            Icons.ManageXMLFiles,
            ToolStatus.Developing);
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
        IRegionManager? regionManager = containerProvider.Resolve<IRegionManager>();
        regionManager.RegisterViewWithRegion<MergeXmlFilesView>(RegionNames.ContentRegion);
        regionManager.RegisterViewWithRegion<ManageXmlFilesView>(RegionNames.ContentRegion);
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
    }
}