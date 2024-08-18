using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace CodeWF.Modules.AI.Views;

public partial class AskBotView : UserControl
{
    public AskBotView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}