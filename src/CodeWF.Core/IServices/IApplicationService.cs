namespace CodeWF.Core.IServices;

public interface IApplicationService
{
    public bool HideTrayIconOnClose { get; set; }
    public bool NeedExitDialogOnClose { get; set; }
    void Load();
    string GetTheme();
    void SetTheme(string theme);
    string GetCulture();
    void SetCulture(string culture);
}