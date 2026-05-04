using CodeWF.Core;
using CodeWF.Modules.ToolFramework.Services;
using Prism.Ioc;
using Prism.Modularity;

namespace CodeWF.Modules.Media;

public sealed class MediaModule : IModule
{
    public MediaModule(IToolMenuService toolMenuService)
    {
        ToolMenuRegistrar.RegisterCategory(
            toolMenuService,
            ToolCategories.Media,
            ToolLocalization.Groups.Media);
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
    }
}
