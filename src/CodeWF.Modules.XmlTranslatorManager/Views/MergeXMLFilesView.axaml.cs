using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace CodeWF.Modules.XmlTranslatorManager.Views;

public partial class MergeXMLFilesView : UserControl
{
    public MergeXMLFilesView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}