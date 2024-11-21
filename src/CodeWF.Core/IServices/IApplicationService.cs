using Avalonia.Styling;

namespace CodeWF.Core.IServices;

public interface IApplicationService
{
    public bool HideTrayIconOnClose { get; set; }
    void Load();
    string GetTheme();
    void SetTheme(string theme);
    string GetCulture();
    void SetCulture(string culture);
}