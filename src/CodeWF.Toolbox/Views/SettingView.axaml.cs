using Avalonia.Markup.Xaml;
using Ursa.Controls;

namespace CodeWF.Toolbox.Views;

public partial class SettingView : UrsaWindow
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