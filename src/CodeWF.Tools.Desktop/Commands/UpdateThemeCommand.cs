namespace CodeWF.Tools.Desktop.Commands;

public class UpdateThemeCommand(ThemeVariant currentTheme) : Command
{
    public ThemeVariant CurrentTheme { get; set; } = currentTheme;
}