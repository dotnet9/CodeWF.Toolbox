using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace CodeWF.Toolbox.Views;

public partial class SettingView : UserControl
{
    public SettingView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}