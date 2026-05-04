using CodeWF.Core;
using CodeWF.Modules.ToolFramework.Services;
using Prism.Ioc;
using Prism.Modularity;

namespace CodeWF.Modules.Network;

public sealed class NetworkModule : IModule
{
    public NetworkModule(IToolMenuService toolMenuService)
    {
        ToolMenuRegistrar.RegisterCategory(
            toolMenuService,
            ToolCategories.Network,
            ToolLocalization.Groups.Network);
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
    }
}
