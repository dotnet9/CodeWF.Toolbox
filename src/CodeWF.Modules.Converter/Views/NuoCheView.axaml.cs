using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace CodeWF.Modules.Converter.Views;

public partial class NuoCheView : UserControl
{
    private Image _qrCodeImage;

    public NuoCheView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}