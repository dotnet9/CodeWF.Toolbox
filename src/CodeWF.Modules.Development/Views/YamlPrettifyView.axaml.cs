using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace CodeWF.Modules.Development.Views;

public partial class YamlPrettifyView : UserControl
{
    public YamlPrettifyView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}