namespace CodeWF.Modules.ToolFramework.Models;

public sealed class ToolOption(string label, string value)
{
    public string Label { get; set; } = label;

    public string Value { get; } = value;

    public override string ToString()
    {
        return Label;
    }
}

