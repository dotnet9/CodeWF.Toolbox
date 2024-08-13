using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using CodeWF.Toolbox.Assets.i18n;
using CodeWF.Toolbox.Services;
using System.Globalization;

namespace CodeWF.Toolbox.Views;

public partial class SettingView : UserControl
{
    private readonly IApplicationService _applicationService;

    public SettingView(IApplicationService applicationService)
    {
        InitializeComponent();
        _applicationService = applicationService;
        InitTheme();
        InitLanguage();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void InitTheme()
    {

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

    private void InitLanguage()
    {
        var culture = _applicationService.GetCulture();
        switch (culture)
        {
            case CultureNames.ChineseSimple:
                this.FindControl<RadioButton>("RBtn_zh_CN")!.IsChecked = true;
                break;
            case CultureNames.ChineseTraditional:
                this.FindControl<RadioButton>("RBtn_zh_Hant")!.IsChecked = true;
                break;
            default:
                this.FindControl<RadioButton>("RBtn_en")!.IsChecked = true;
                break;
        }
    }

    private void ChangeTheme_OnClick(object? sender, RoutedEventArgs e)
    {
        var theme = (sender as RadioButton)?.Tag as ThemeVariant;
        _applicationService.SetTheme(theme ?? ThemeVariant.Default);
    }
    
    private void ChangeLanguage_OnClick(object? sender, RoutedEventArgs e)
    {
        var lauguage = (sender as RadioButton)?.Tag?.ToString() ?? CultureNames.English;
        _applicationService.SetCulture(lauguage);
    }
}