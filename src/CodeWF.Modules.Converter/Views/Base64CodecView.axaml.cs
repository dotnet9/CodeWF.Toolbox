using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CodeWF.Modules.Converter.ViewModels;

namespace CodeWF.Modules.Converter.Views;

public partial class Base64CodecView : UserControl
{
    public Base64CodecView()
    {
        InitializeComponent();

        if (DataContext is Base64CodecViewModel vm)
        {
            vm.ClipboardOwner = this;
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
