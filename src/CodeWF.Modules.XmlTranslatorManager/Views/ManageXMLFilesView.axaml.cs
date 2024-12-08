using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace CodeWF.Modules.XmlTranslatorManager.Views;

public partial class ManageXmlFilesView : UserControl
{
    public ManageXmlFilesView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}