using CodeWF.Core;
using CodeWF.Modules.ToolFramework.Services;
using Prism.Ioc;
using Prism.Modularity;

namespace CodeWF.Modules.Math;

public sealed class MathModule : IModule
{
    public MathModule(IToolMenuService toolMenuService)
    {
        ToolMenuRegistrar.RegisterCategory(
            toolMenuService,
            ToolCategories.Math,
            ToolLocalization.Groups.Math);
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
    }
}
