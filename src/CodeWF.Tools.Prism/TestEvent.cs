namespace CodeWF.Tools.Prism;

public class TestEvent : PubSubEvent<TestEventParameter>
{
}

public class TestEventParameter
{
    public string? Args { get; set; }
}