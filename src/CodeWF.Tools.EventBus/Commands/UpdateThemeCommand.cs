using Avalonia.Styling;

namespace CodeWF.Tools.EventBus.Commands;

public class UpdateThemeCommand(ThemeVariant currentTheme) : Command
{
    public ThemeVariant CurrentTheme { get; set; } = currentTheme;
}