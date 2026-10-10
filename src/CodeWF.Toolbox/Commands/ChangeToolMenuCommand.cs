using CodeWF.Core.Models;
using CodeWF.Toolkit.EventBus;

namespace CodeWF.Toolbox.Commands;

public class ChangeToolMenuCommand : Command
{
    public ChangeToolMenuCommand(ToolMenuItem toolMenuItem)
    {
        ToolMenuItem = toolMenuItem;
    }

    public ToolMenuItem ToolMenuItem { get; set; } = null!;
}