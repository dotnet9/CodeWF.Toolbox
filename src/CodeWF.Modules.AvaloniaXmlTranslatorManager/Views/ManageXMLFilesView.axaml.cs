using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace CodeWF.Modules.AvaloniaXmlTranslatorManager.Views;

public partial class ManageXMLFilesView : UserControl
{
    public ManageXMLFilesView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}