using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using CodeWF.Toolbox.Services;

namespace CodeWF.Toolbox.Views;

public partial class SettingView : UserControl
{
    private readonly IApplicationService _applicationService;

    public SettingView(IApplicationService applicationService)
    {
        InitializeComponent();
        _applicationService = applicationService;

        var theme = _applicationService.GetTheme();
        if (theme == ThemeVariant.Default)
        {
            this.FindControl<RadioButton>("RBtnThemeDefault")!.IsChecked = true;
        }
        else if (theme == ThemeVariant.Dark)
        {
            this.FindControl<RadioButton>("RBtnThemeDark")!.IsChecked = true;
        }
        else
        {
            this.FindControl<RadioButton>("RBtnThemeLight")!.IsChecked = true;
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void ChangeTheme_OnClick(object? sender, RoutedEventArgs e)
    {
        var theme = (sender as RadioButton)?.Tag as ThemeVariant;
        _applicationService.SetTheme(theme ?? ThemeVariant.Default);
    }
}