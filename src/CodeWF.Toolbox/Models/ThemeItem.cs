using Avalonia.Styling;

namespace CodeWF.Toolbox.Models;

public class ThemeItem(string name, string key, ThemeVariant theme)
{
    public string Name { get; set; } = name;
    public string Key { get; set; } = key;
    public ThemeVariant Theme { get; set; } = theme;
}