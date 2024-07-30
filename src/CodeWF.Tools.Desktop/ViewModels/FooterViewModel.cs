﻿namespace CodeWF.Tools.Desktop.ViewModels;

public class FooterViewModel
{
    public int CurrentYear => DateTime.Now.Year;
    public string Owner => $"{AppInfo.ToolName}&{AppInfo.Author}";
    public string DotnetVersion => RuntimeInformation.FrameworkDescription;

    public void OpenCodeWFWebSite()
    {
        "https://codewf.com".OpenBrowserForVisitSite();
    }

    public void OpenCodeWFRepository()
    {
        "https://github.com/dotnet9/CodeWF".OpenBrowserForVisitSite();
    }
}