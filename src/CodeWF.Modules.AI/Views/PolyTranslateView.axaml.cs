using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace CodeWF.Modules.AI.Views;

public partial class PolyTranslateView : UserControl
{
    public PolyTranslateView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}