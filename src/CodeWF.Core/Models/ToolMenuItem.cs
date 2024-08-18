using System.Collections.ObjectModel;

namespace CodeWF.Core.Models;

public class ToolMenuItem
{
    public int Level { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? ViewName { get; set; }
    public ToolStatus? Status { get; set; }
    public string? Icon { get; set; }
    public bool IsSeparator { get; set; }
    public ObservableCollection<ToolMenuItem> Children { get; set; } = new();

    public override string ToString()
    {
        return $"{Name}";
    }
}