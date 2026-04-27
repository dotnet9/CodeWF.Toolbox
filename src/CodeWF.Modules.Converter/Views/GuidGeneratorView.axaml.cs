using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CodeWF.Modules.Converter.ViewModels;

namespace CodeWF.Modules.Converter.Views;

public partial class GuidGeneratorView : UserControl
{
    public GuidGeneratorView()
    {
        InitializeComponent();

        if (DataContext is GuidGeneratorViewModel vm)
        {
            vm.ClipboardOwner = this;
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
