using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CodeWF.AvaloniaControls.Controls;

namespace CodeWF.Toolbox.Views;

public partial class ExitOptionWindow : CodeWFWindow
{
    public ExitOptionWindow()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }

    private void ConfirmButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(true);
    }
}
