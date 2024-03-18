namespace CodeWF.Tools.Prism;

public class ChangeToolEvent : PubSubEvent<ChangeToolEventParameter>
{
}

public class ChangeToolEventParameter
{
    public string? ToolHeader { get; set; }
}