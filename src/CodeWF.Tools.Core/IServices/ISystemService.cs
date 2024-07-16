using Avalonia.Styling;

namespace CodeWF.Tools.Core.IServices;

public interface ISystemService
{
    ThemeVariant LoadTheme();

    void ChangeTheme(ThemeVariant theme);

    void ChangeTheme();
}