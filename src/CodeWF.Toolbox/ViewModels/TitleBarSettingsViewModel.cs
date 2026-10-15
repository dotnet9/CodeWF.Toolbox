using CodeWF.Toolkit.EventBus;
using CodeWF.Core.IServices;
using CodeWF.Toolbox.Models;
using CodeWF.Toolbox.Commands;
using CodeWF.Toolbox.Services;
using CodeWF.Toolkit.Lang;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Styling;

namespace CodeWF.Toolbox.ViewModels;

public class TitleBarSettingsViewModel : ViewModelBase
{
    private readonly IApplicationService _applicationService;
    private readonly ILoginService _loginService;
    private readonly IUserProfileService _userProfileService;

    public TitleBarSettingsViewModel(
        IApplicationService applicationService,
        ILoginService loginService,
        IUserProfileService userProfileService)
    {
        _applicationService = applicationService;
        _loginService = loginService;
        _userProfileService = userProfileService;
        _loginService.LoginStateChanged += (_, _) => RefreshUser();
        _userProfileService.ProfileChanged += (_, _) => RefreshSearchHistory();
        InitTheme();
        var service = (ApplicationService)_applicationService;
        _selectedMode = ThemeModes.FirstOrDefault(item => item.Key == service.GetMode()) ?? ThemeModes[0];
        InitLanguage();
        RefreshUser();
        RefreshSearchHistory();
    }

    public ObservableCollection<ThemeItem> Themes { get; private set; } = [];
    public ObservableCollection<ThemeItem> ThemeModes { get; } =
    [new(Localization.Workspace.System, "System", ThemeVariant.Default), new(Localization.Workspace.Light, "Light", ThemeVariant.Light), new(Localization.Workspace.Dark, "Dark", ThemeVariant.Dark)];
    private ThemeItem? _selectedMode;
    public ThemeItem? SelectedMode
    {
        get => _selectedMode;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedMode, value);
            if (value is not null) ((ApplicationService)_applicationService).SetMode(value.Key);
        }
    }

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

    public ObservableCollection<string> SearchHistory { get; } = [];

    private string _searchText = string.Empty;

    public string SearchText
    {
        get => _searchText;
        set
        {
            this.RaiseAndSetIfChanged(ref _searchText, value ?? string.Empty);
            EventBus.Default.Publish(new SearchToolMenuCommand(_searchText));
        }
    }

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

    public void CommitSearchText()
    {
        _userProfileService.RecordSearch(SearchText);
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

    private void RefreshUser()
    {
        CurrentUsername = _loginService.CurrentUsername
                          ?? _loginService.ConfiguredUsername
                          ?? string.Empty;
    }

    private void RefreshSearchHistory()
    {
        SearchHistory.Clear();
        foreach (var keyword in _userProfileService.SearchHistory)
        {
            SearchHistory.Add(keyword);
        }
    }
}
