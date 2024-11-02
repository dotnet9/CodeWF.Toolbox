using CodeWF.Core.IServices;
using CodeWF.Toolbox.Models;
using CodeWF.Toolbox.Services;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Linq;

namespace CodeWF.Toolbox.ViewModels;

public class SettingViewModel : ViewModelBase
{
    private readonly IApplicationService _applicationService;

    public SettingViewModel(IApplicationService applicationService)
    {
        _applicationService = applicationService;
        InitTheme();
        InitLanguage();
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

    private void SetTheme()
    {
        _applicationService.SetTheme(SelectedTheme?.Name);
    }

    private void SetLanguage()
    {
        _applicationService.SetCulture(SelectedLanguage?.Culture);
    }
}