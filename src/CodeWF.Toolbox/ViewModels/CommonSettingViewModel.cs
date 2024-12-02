using AvaloniaExtensions.Axaml.Markup;
using CodeWF.Core.IServices;
using CodeWF.Core.RegionAdapters;
using CodeWF.Toolbox.Commands;
using CodeWF.Toolbox.I18n;
using CodeWF.Toolbox.Models;
using CodeWF.Toolbox.Services;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Linq;

namespace CodeWF.Toolbox.ViewModels;

public class CommonSettingViewModel : ViewModelBase, ITabItemBase
{
    private readonly IApplicationService _applicationService;

    public string? Title { get; set; }
    public string Message { get; set; }

    public CommonSettingViewModel(IApplicationService applicationService)
    {
        _applicationService = applicationService;
        InitTheme();
        InitLanguage();
        HideTrayIconOnClose = applicationService.HideTrayIconOnClose;
        NeedExitDialogOnClose = applicationService.NeedExitDialogOnClose;

        Title = I18nManager.GetString(Language.Setting);
        Message = I18nManager.GetString(Language.InterfaceStyleSettings);
    }

    private void InitTheme()
    {
        var themes = ((ApplicationService)_applicationService).Themes;
        Themes = new ObservableCollection<ThemeItem>(themes);

        var theme = _applicationService.GetTheme();
        _selectedTheme = themes.FirstOrDefault(item => string.Equals(theme, item.Name));
    }

    private void InitLanguage()
    {
        var languages = ((ApplicationService)_applicationService).Languages;
        Languages = new ObservableCollection<LanguageItem>(languages);

        var language = _applicationService.GetCulture();
        _selectedLanguage = languages.FirstOrDefault(item => string.Equals(language, item.Culture));
    }

    private bool _hideTrayIconOnClose;

    public bool HideTrayIconOnClose
    {
        get => _hideTrayIconOnClose;
        set => this.RaiseAndSetIfChanged(ref _hideTrayIconOnClose, value);
    }

    private bool _needExitDialogOnClose;

    public bool NeedExitDialogOnClose
    {
        get => _needExitDialogOnClose;
        set => this.RaiseAndSetIfChanged(ref _needExitDialogOnClose, value);
    }

    public ObservableCollection<ThemeItem> Themes { get; private set; }

    private ThemeItem? _selectedTheme;

    public ThemeItem? SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedTheme, value);
            SetTheme();
        }
    }

    public ObservableCollection<LanguageItem> Languages { get; private set; }

    private LanguageItem? _selectedLanguage;

    public LanguageItem? SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedLanguage, value);
            SetLanguage();
        }
    }

    public void ChangeHideTrayIconOnCloseHandler()
    {
        _applicationService.HideTrayIconOnClose = HideTrayIconOnClose;
        EventBus.EventBus.Default.Publish(new ChangeApplicationStatusCommand());
    }

    public void ChangeDisplayPromptWhenClosingHandler()
    {
        _applicationService.NeedExitDialogOnClose = NeedExitDialogOnClose;
    }

    private void SetTheme()
    {
        _applicationService.SetTheme(SelectedTheme?.Name);
    }

    private void SetLanguage()
    {
        _applicationService.SetCulture(SelectedLanguage?.Culture);
    }
}