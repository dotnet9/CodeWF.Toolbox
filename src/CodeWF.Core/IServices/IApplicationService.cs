using Avalonia.Styling;

namespace CodeWF.Core.IServices;
public interface IApplicationService
{
    void Load();
    string GetTheme();
    void SetTheme(string theme);
    string GetCulture();
    void SetCulture(string culture);
}
