using Avalonia.Controls;
using AvaloniaXmlTranslator;
using CodeWF.Core.RegionAdapters;
using CodeWF.Tools.Extensions;
using System;
using System.Reflection;
using TextMateSharp.Grammars;

namespace CodeWF.Toolbox.ViewModels;

public class AboutViewModel : ViewModelBase, ITabItemBase
{
    public string? Title { get; set; } = I18nManager.GetString(Localization.AboutView.Title);
    public string? AppName { get; set; }
    public string? Message { get; set; } = I18nManager.GetString(Localization.AboutView.Description);

    public string? CompileTime { get; set; } =
        Assembly.GetExecutingAssembly().CompileTime()?.ToString("yyyy-MM-dd HH:mm:ss");

    public void OpenRepository(UserControl owner)
    {
        OpenUrlAsync(owner, "https://github.com/dotnet9/CodeWF.Toolbox");
    }

    public void OpenOnLineToolbox(UserControl owner)
    {
        OpenUrlAsync(owner, "https://dotnet9.com");
    }

    private async void OpenUrlAsync(UserControl owner, string uri)
    {
        var top = TopLevel.GetTopLevel(owner);
        if (top is null) return;
        var launcher = top.Launcher;
        await launcher.LaunchUriAsync(new Uri(uri));
    }
}