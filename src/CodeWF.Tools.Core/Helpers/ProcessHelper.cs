namespace CodeWF.Tools.Core.Helpers;

public class ProcessHelper
{
    public static void OpenBrowserForVisitSite(string link)
    {
        ProcessStartInfo param = new ProcessStartInfo { FileName = link, UseShellExecute = true, Verb = "open" };
        Process.Start(param);
    }
}