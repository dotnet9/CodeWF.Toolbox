using CodeWF.Core;
using CodeWF.Modules.ToolFramework.Services;
using Prism.Ioc;
using Prism.Modularity;

namespace CodeWF.Modules.Security;

public sealed class SecurityModule : IModule
{
    public SecurityModule(IToolMenuService toolMenuService)
    {
        ToolMenuRegistrar.RegisterCategory(
            toolMenuService,
            ToolCategories.Security,
            ToolLocalization.Groups.Security,
            addSeparator: true);
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
    }
}
