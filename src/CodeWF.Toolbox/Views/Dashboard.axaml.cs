using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
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
        var top = TopLevel.GetTopLevel(this);
        if (top is null) return;
        var launcher = top.Launcher;
        await launcher.LaunchUriAsync(new Uri("https://github.com/dotnet9/CodeWF.Toolbox"));
    }
}