using Avalonia.Interactivity;
using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using CodeWF.Tools.Core.IServices;

namespace CodeWF.Tools.Module.Test.Dialogs;

public partial class SettingsDialog : UserControl
{
    private bool _isChangeTheme;

    public SettingsDialog()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        var theme = ContainerLocator.Current.Resolve<ISystemService>().LoadTheme();
        UpdateThemeSwitch(theme);
    }

    private void UpdateThemeSwitch(ThemeVariant theme)
    {
        _isChangeTheme = true;
        this.FindControl<ToggleSwitch>("ToggleSwitchTheme")!.IsChecked = theme == ThemeVariant.Dark;
        _isChangeTheme = false;
    }

    private void ToggleButton_OnIsCheckedChanged(object sender, RoutedEventArgs e)
    {
        if (_isChangeTheme) return;

        ContainerLocator.Current.Resolve<ISystemService>().ChangeTheme();
    }
}