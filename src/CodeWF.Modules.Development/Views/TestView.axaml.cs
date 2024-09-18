using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace CodeWF.Modules.Development.Views;

public partial class TestView : UserControl
{
    public TestView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}