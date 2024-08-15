using Avalonia.Styling;

namespace CodeWF.Core.IServiceInterfaces;
public interface IApplicationService
{
    void Load();
    ThemeVariant GetTheme();
    void SetTheme(ThemeVariant theme);
    string GetCulture();
    void SetCulture(string culture);
}
