using CodeWF.EventBus;

namespace CodeWF.Toolbox.Commands;

public sealed class OpenToolMenuCommand : Command
{
    public OpenToolMenuCommand(string viewName)
    {
        ViewName = viewName;
    }

    public string ViewName { get; }
}
