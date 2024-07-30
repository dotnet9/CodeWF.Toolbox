namespace CodeWF.Tools.Desktop.Commands;

public class ChangeToolCommand(string? toolHeader) : Command
{
    public string? ToolHeader { get; set; } = toolHeader;
}