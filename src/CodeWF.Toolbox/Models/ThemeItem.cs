using global::Avalonia.Styling;
using global::Avalonia.Controls;
using global::Avalonia.Media;

namespace CodeWF.Toolbox.Models;

public class ThemeItem(string name, string key, ThemeVariant theme)
{
    public string Name { get; set; } = name;
    public string Key { get; set; } = key;
    public IBrush Swatch => global::Avalonia.Application.Current is { } app && app.TryFindResource("StudioSwatch" + Key, out var value) && value is IBrush brush ? brush : Brushes.Transparent;
    public ThemeVariant Theme { get; set; } = theme;
}