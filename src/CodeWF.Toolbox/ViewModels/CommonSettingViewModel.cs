using CodeWF.Core.IServices;
using CodeWF.Core.RegionAdapters;
using CodeWF.Toolbox.Commands;
using CodeWF.Toolbox.Models;
using CodeWF.Toolbox.Services;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Linq;
using Lang.Avalonia;
using System.Threading.Tasks;

namespace CodeWF.Toolbox.ViewModels;

public class CommonSettingViewModel : ViewModelBase, ITabItemBase
{
    private readonly IApplicationService _applicationService;

    public string? TitleKey { get; set; }
    public string? MessageKey { get; set; }

    public CommonSettingViewModel(IApplicationService applicationService)
    {
        _applicationService = applicationService;
        InitTheme();
        InitLanguage();
        AutoOpenToolboxAtStartup = applicationService.AutoOpenToolboxAtStartup;
        HideTrayIconOnClose = applicationService.HideTrayIconOnClose;
        NeedExitDialogOnClose = applicationService.NeedExitDialogOnClose;

        TitleKey = Localization.CommonSettingView.Title;
        MessageKey = Localization.CommonSettingView.Description;
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

    private bool _autoOpenToolboxAtStartup;

    public bool AutoOpenToolboxAtStartup
    {
        get => _autoOpenToolboxAtStartup;
        set => this.RaiseAndSetIfChanged(ref _autoOpenToolboxAtStartup, value);
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

    public ObservableCollection<ThemeItem> Themes { get; private set; } = [];

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

    public ObservableCollection<LocalizationLanguage> Languages { get; private set; } = [];

    private LocalizationLanguage? _selectedLanguage;

    public LocalizationLanguage? SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedLanguage, value);
            SetLanguage();
        }
    }

    public Task ChangeAutoOpenToolboxAtStartupHandlerAsync()
    {
        _applicationService.AutoOpenToolboxAtStartup = AutoOpenToolboxAtStartup;
        EventBus.EventBus.Default.Publish(new ChangeApplicationStatusCommand());
        return Task.CompletedTask;
    }

    public Task ChangeHideTrayIconOnCloseHandlerAsync()
    {
        _applicationService.HideTrayIconOnClose = HideTrayIconOnClose;
        EventBus.EventBus.Default.Publish(new ChangeApplicationStatusCommand());
        return Task.CompletedTask;
    }

    public Task ChangeDisplayPromptWhenClosingHandlerAsync()
    {
        _applicationService.NeedExitDialogOnClose = NeedExitDialogOnClose;
        return Task.CompletedTask;
    }

    private void SetTheme()
    {
        if (SelectedTheme?.Key is { } theme)
        {
            _applicationService.SetTheme(theme);
        }
    }

    private void SetLanguage()
    {
        if (SelectedLanguage?.CultureName is { } cultureName)
        {
            _applicationService.SetCulture(cultureName);
        }
    }
}
