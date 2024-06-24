using Avalonia.Markup.Xaml;

namespace CodeWF.Tools.Module.Test.Views;

public partial class EventBusTestView : UserControl
{
    public EventBusTestView()
    {
        InitializeComponent();
    }
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}