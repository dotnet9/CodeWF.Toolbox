using CodeWF.Core;
using CodeWF.Modules.ToolFramework.Services;
using Prism.Ioc;
using Prism.Modularity;

namespace CodeWF.Modules.Text;

public sealed class TextModule : IModule
{
    public TextModule(IToolMenuService toolMenuService)
    {
        ToolMenuRegistrar.RegisterCategory(
            toolMenuService,
            ToolCategories.Text,
            ToolLocalization.Groups.Text);
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
    }
}
