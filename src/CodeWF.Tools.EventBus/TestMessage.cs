namespace CodeWF.Tools.EventBus;

public class TestMessage(object sender, string? name, string? currentTime) : Message(sender)
{
    public string? Name { get; set; } = name;
    public string? CurrentTime { get; set; } = currentTime;
}