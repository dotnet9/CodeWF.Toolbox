namespace CodeWF.Tools.Desktop.IServices;

public interface ISystemService
{
    ThemeVariant LoadTheme();

    void ChangeTheme(ThemeVariant theme);

    void ChangeTheme();
}