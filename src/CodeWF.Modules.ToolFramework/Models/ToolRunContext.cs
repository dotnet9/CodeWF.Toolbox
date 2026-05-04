using Avalonia.Media.Imaging;

namespace CodeWF.Modules.ToolFramework.Models;

public sealed class ToolRunContext(
    IReadOnlyDictionary<string, ToolField> fields,
    IReadOnlyDictionary<string, ToolOutput> outputs)
{
    public IReadOnlyDictionary<string, ToolField> Fields { get; } = fields;

    public IReadOnlyDictionary<string, ToolOutput> Outputs { get; } = outputs;

    public string Text(string id)
    {
        return Fields.TryGetValue(id, out var field) ? field.Text : string.Empty;
    }

    public decimal Number(string id)
    {
        return Fields.TryGetValue(id, out var field) ? field.Number : 0;
    }

    public int Int(string id)
    {
        return (int)Math.Round(Number(id), MidpointRounding.AwayFromZero);
    }

    public bool Bool(string id)
    {
        return Fields.TryGetValue(id, out var field) && field.Boolean;
    }

    public string Option(string id)
    {
        return Fields.TryGetValue(id, out var field) ? field.SelectedOption?.Value ?? string.Empty : string.Empty;
    }

    public void SetText(string id, string? value)
    {
        if (Outputs.TryGetValue(id, out var output))
        {
            output.Bitmap = null;
            output.Text = value ?? string.Empty;
        }
    }

    public void SetImage(string id, Bitmap? bitmap)
    {
        if (Outputs.TryGetValue(id, out var output))
        {
            output.Text = string.Empty;
            output.Bitmap = bitmap;
        }
    }

    public void ClearOutputs()
    {
        foreach (var output in Outputs.Values)
        {
            output.Text = string.Empty;
            output.Bitmap = null;
        }
    }
}

