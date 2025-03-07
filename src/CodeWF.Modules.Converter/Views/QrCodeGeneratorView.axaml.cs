using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace CodeWF.Modules.Converter.Views;

public partial class QrCodeGeneratorView : UserControl
{
    public QrCodeGeneratorView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}