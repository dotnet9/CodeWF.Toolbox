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
    private readonly ILoginService _loginService;

    public TitleBarSettingsViewModel(IApplicationService applicationService, ILoginService loginService)
    {
        _applicationService = applicationService;
        _loginService = loginService;
        _loginService.LoginStateChanged += (_, _) => RefreshUser();
        InitTheme();
        InitLanguage();
        RefreshUser();
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

    private string _currentUsername = string.Empty;

    public string CurrentUsername
    {
        get => _currentUsername;
        private set
        {
            this.RaiseAndSetIfChanged(ref _currentUsername, value);
            this.RaisePropertyChanged(nameof(UserInitial));
        }
    }

    public string UserInitial =>
        string.IsNullOrWhiteSpace(CurrentUsername)
            ? "?"
            : CurrentUsername.Trim()[0].ToString().ToUpperInvariant();

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

    private void RefreshUser()
    {
        CurrentUsername = _loginService.CurrentUsername
                          ?? _loginService.ConfiguredUsername
                          ?? string.Empty;
    }
}
