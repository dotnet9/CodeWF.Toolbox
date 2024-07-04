using Avalonia.Markup.Xaml;

namespace CodeWF.Tools.Module.Test.Views;

public partial class AvaPlotView : UserControl
{
    public AvaPlotView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}