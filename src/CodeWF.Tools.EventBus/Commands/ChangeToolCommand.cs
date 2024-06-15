namespace CodeWF.Tools.EventBus.Commands;

public class ChangeToolCommand(string? toolHeader) : Command
{
    public string? ToolHeader { get; set; } = toolHeader;
}