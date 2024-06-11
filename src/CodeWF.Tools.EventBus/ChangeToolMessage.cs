namespace CodeWF.Tools.EventBus;

public class ChangeToolMessage(object sender, string? toolHeader) : Message(sender)
{
    public string? ToolHeader { get; set; } = toolHeader;
}