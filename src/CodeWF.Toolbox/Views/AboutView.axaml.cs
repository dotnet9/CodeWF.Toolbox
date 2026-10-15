using global::Avalonia.Controls;

namespace CodeWF.Toolbox.Views;

public partial class AboutView : UserControl
{
    public AboutView()
    {
        InitializeComponent();
        DetachedFromVisualTree += (_, _) => (DataContext as CodeWF.Toolbox.ViewModels.AboutViewModel)?.Update.Cancel();
    }
}