namespace CodeWF.Tools.Desktop.ViewModels;

public class FooterViewModel
{
    public int CurrentYear => DateTime.Now.Year;
    public string Owner => $"{AppInfo.AppInfo.ToolName}&{AppInfo.AppInfo.Author}";
    public string DotnetVersion => RuntimeInformation.FrameworkDescription;

    public void OpenCodeWFWebSite()
    {
        ProcessHelper.OpenBrowserForVisitSite("https://codewf.com");
    }

    public void OpenCodeWFRepository()
    {
        ProcessHelper.OpenBrowserForVisitSite("https://github.com/dotnet9/CodeWF");
    }
}