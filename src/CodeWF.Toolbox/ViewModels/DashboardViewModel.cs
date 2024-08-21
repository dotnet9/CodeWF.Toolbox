using Avalonia.Controls;
using System;

namespace CodeWF.Toolbox.ViewModels;

internal class DashboardViewModel : ViewModelBase
{
    public void OpenRepository(UserControl owner)
    {
        OpenUrlAsync(owner, "https://github.com/dotnet9/CodeWF.Toolbox");
    }

    public void OpenOnLineToolbox(UserControl owner)
    {
        OpenUrlAsync(owner, "https://codewf.com/tool");
    }

    private async void OpenUrlAsync(UserControl owner, string uri)
    {
        var top = TopLevel.GetTopLevel(owner);
        if (top is null) return;
        var launcher = top.Launcher;
        await launcher.LaunchUriAsync(new Uri(uri));
    }
}