using Avalonia.Controls;
using Avalonia.Interactivity;
using System;

namespace CodeWF.Toolbox.Views;

public partial class Dashboard : UserControl
{
    public Dashboard()
    {
        InitializeComponent();
    }

    private async void OpenRepository(object sender, RoutedEventArgs e)
    {
        OpenUrl("https://github.com/dotnet9/CodeWF.Toolbox");
    }

    private async void OpenOnLineToolbox(object sender, RoutedEventArgs e)
    {
        OpenUrl("https://codewf.com/tool");
    }

    private async void OpenUrl(string uri)
    {
        var top = TopLevel.GetTopLevel(this);
        if (top is null) return;
        var launcher = top.Launcher;
        await launcher.LaunchUriAsync(new Uri(uri));
    }
}