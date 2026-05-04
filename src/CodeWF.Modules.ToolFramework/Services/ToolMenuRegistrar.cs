using CodeWF.Core;
using CodeWF.Core.Models;
using CodeWF.Modules.ToolFramework.Views;

namespace CodeWF.Modules.ToolFramework.Services;

public static class ToolMenuRegistrar
{
    public static void RegisterCategory(
        IToolMenuService toolMenuService,
        string category,
        string groupName,
        string? icon = null,
        bool addSeparator = false)
    {
        var tools = ToolCatalog.GetByCategory(category);
        if (tools.Count == 0)
        {
            return;
        }

        if (addSeparator)
        {
            toolMenuService.AddSeparator();
        }

        toolMenuService.AddGroup(groupName, icon ?? ToolCatalog.IconForCategory(category));
        foreach (var tool in tools)
        {
            toolMenuService.AddItem(
                tool.Name,
                groupName,
                tool.Description,
                $"{nameof(ToolView)}?tool={Uri.EscapeDataString(tool.Id)}",
                tool.Icon,
                ToolStatus.Complete);
        }
    }
}
