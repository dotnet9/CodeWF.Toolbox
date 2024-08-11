using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace CodeWF.Modules.Converter.Views;

public partial class DateTimeConverterView : UserControl
{
    public DateTimeConverterView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}