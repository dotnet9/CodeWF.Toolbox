using CodeWF.Core.IServices;
using CodeWF.Core.RegionAdapters;
using CodeWF.Toolbox.Commands;
using ReactiveUI;
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
        AutoOpenToolboxAtStartup = applicationService.AutoOpenToolboxAtStartup;
        HideTrayIconOnClose = applicationService.HideTrayIconOnClose;
        NeedExitDialogOnClose = applicationService.NeedExitDialogOnClose;

        TitleKey = Localization.CommonSettingView.Title;
        MessageKey = Localization.CommonSettingView.Description;
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
}