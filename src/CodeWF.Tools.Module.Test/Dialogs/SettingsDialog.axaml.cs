using Avalonia.Markup.Xaml;

namespace CodeWF.Tools.Module.Test.Dialogs;

public partial class SettingsDialog : UserControl
{
    public SettingsDialog()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}