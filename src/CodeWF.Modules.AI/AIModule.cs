using CodeWF.Core;
using CodeWF.Core.Models;
using CodeWF.Modules.AI.Helpers;
using CodeWF.Modules.AI.Views;
using Ursa.PrismExtension;

namespace CodeWF.Modules.AI;

public class AIModule : IModule
{
    public AIModule(IToolMenuService toolMenuService)
    {
        var groupName = Localization.AIModule.Title;
        toolMenuService.AddSeparator();
        toolMenuService.AddGroup(groupName, Icons.AI);
        toolMenuService.AddItem(Localization.AskBotView.Title, groupName, Localization.AskBotView.Description, nameof(AskBotView),
            Icons.AskBot,
            ToolStatus.Developing);
        toolMenuService.AddItem(Localization.PolyTranslateView.Title, groupName, Localization.PolyTranslateView.Description,
            nameof(PolyTranslateView),
            Icons.PolyTranslate,
            ToolStatus.Complete);
        toolMenuService.AddItem(Localization.Title2SlugView.Title, groupName, Localization.Title2SlugView.Description, nameof(Title2SlugView),
            Icons.Title2Slug,
            ToolStatus.Complete);
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
        IRegionManager? regionManager = containerProvider.Resolve<IRegionManager>();
        regionManager.RegisterViewWithRegion<AskBotView>(RegionNames.ContentRegion);
        regionManager.RegisterViewWithRegion<PolyTranslateView>(RegionNames.ContentRegion);
        regionManager.RegisterViewWithRegion<Title2SlugView>(RegionNames.ContentRegion);
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.Register<ChoiceLanguagesView>();
        containerRegistry.RegisterScoped<ApiClient>();
        containerRegistry.RegisterUrsaDialogView<ChoiceLanguagesView>(DialogNames.ChoiceLanguages);
    }
}