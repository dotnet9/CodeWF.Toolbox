using Avalonia.Styling;

namespace CodeWF.Toolbox.Services;

public interface IApplicationService
{
    void Load();
    ThemeVariant GetTheme();
    void SetTheme(ThemeVariant theme);
}