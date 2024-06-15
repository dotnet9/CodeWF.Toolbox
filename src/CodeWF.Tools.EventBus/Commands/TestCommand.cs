namespace CodeWF.Tools.EventBus.Commands;

public class TestCommand(string? name, string? currentTime) : Command
{
    public string? Name { get; set; } = name;
    public string? CurrentTime { get; set; } = currentTime;
}