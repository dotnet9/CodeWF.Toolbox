using CodeWF.Core.IServices;
using CodeWF.Toolbox.Models;
using CodeWF.Toolbox.Services;
using Lang.Avalonia;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Linq;

namespace CodeWF.Toolbox.ViewModels;

public class TitleBarSettingsViewModel : ViewModelBase
{
    private readonly IApplicationService _applicationService;

    public TitleBarSettingsViewModel(IApplicationService applicationService)
    {
        _applicationService = applicationService;
        InitTheme();
        InitLanguage();
    }

    public ObservableCollection<ThemeItem> Themes { get; private set; } = [];

    private ThemeItem? _selectedTheme;

    public ThemeItem? SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            if (ReferenceEquals(_selectedTheme, value))
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _selectedTheme, value);
            if (value?.Key is { } theme)
            {
                _applicationService.SetTheme(theme);
            }
        }
    }

    public ObservableCollection<LocalizationLanguage> Languages { get; private set; } = [];

    private LocalizationLanguage? _selectedLanguage;

    public LocalizationLanguage? SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            if (ReferenceEquals(_selectedLanguage, value))
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _selectedLanguage, value);
            if (value?.CultureName is { } cultureName)
            {
                _applicationService.SetCulture(cultureName);
            }
        }
    }

    private void InitTheme()
    {
        var themes = ((ApplicationService)_applicationService).Themes;
        Themes = new ObservableCollection<ThemeItem>(themes);

        var theme = _applicationService.GetTheme();
        _selectedTheme = themes.FirstOrDefault(item => string.Equals(theme, item.Key));
    }

    private void InitLanguage()
    {
        var languages = I18nManager.Instance.GetLanguages() ?? [];
        Languages = new ObservableCollection<LocalizationLanguage>(languages);

        var language = _applicationService.GetCulture();
        _selectedLanguage = Languages.FirstOrDefault(l => l.CultureName == language);
    }
}