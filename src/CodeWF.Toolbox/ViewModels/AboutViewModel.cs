using Avalonia.Controls.Notifications;
using CodeWF.Core.IServices;
using CodeWF.Core.RegionAdapters;
using CodeWF.Avalonia.Lang;
using CodeWF.Toolkit.Core.Extensions;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace CodeWF.Toolbox.ViewModels;

public class AboutViewModel : ViewModelBase, ITabItemBase
{
    private readonly IUpdateChecker _updateChecker;
    private readonly INotificationService _notificationService;

    public string? TitleKey { get; set; } = Localization.AboutView.Title;
    public string? MessageKey { get; set; } = Localization.AboutView.Description;
    public string? AppName { get; set; }
    public string? Product { get; set; } = Assembly.GetExecutingAssembly().Product();
    public string? Version { get; set; } = Assembly.GetExecutingAssembly().Version();
    public string? Platform { get; set; }
    public string? Copyright { get; set; } = Assembly.GetExecutingAssembly().Copyright();

    public string? CompileTime { get; set; } =
        Assembly.GetExecutingAssembly().CompileTime()?.ToString("yyyy-MM-dd HH:mm:ss");

    public ReactiveCommand<Unit, Unit> CheckUpdateCommand { get; }

    public AboutViewModel(IUpdateChecker updateChecker, INotificationService notificationService)
    {
        _updateChecker = updateChecker;
        _notificationService = notificationService;
        CheckUpdateCommand = ReactiveCommand.CreateFromTask(CheckUpdateAsync);

#if PLATFORM_LINUX_X64
        Platform = "Linux x64";
#elif PLATFORM_LINUX_ARM64
        Platform = "Linux ARM64";
#elif PLATFORM_WIN_X64
        Platform = "Windows x64";
#elif PLATFORM_WIN_X86
        Platform = "Windows x86";
#else
        Platform = "Unknown";
#endif
    }

    private async Task CheckUpdateAsync()
    {
        try
        {
            System.Version.TryParse(Version, out System.Version? current);
            UpdateCheckResult result = await _updateChecker.CheckAsync(current ?? new System.Version(0, 0, 0));
            if (!result.Succeeded)
            {
                _notificationService.Show(Product ?? "CodeWF Toolbox",
                    ResolveText(Localization.AboutView.UpdateCheckFailed) + result.Error,
                    NotificationType.Error);
                return;
            }

            if (result.Update is { } update)
            {
                _notificationService.Show(Product ?? "CodeWF Toolbox",
                    ResolveText(Localization.AboutView.UpdateAvailable) + update.Tag,
                    NotificationType.Information);
                OpenReleasePage(update.PageUrl);
                return;
            }

            _notificationService.Show(Product ?? "CodeWF Toolbox",
                ResolveText(Localization.AboutView.UpToDate), NotificationType.Success);
        }
        catch (Exception exception)
        {
            _notificationService.Show(Product ?? "CodeWF Toolbox",
                ResolveText(Localization.AboutView.UpdateCheckFailed) + exception.Message,
                NotificationType.Error);
        }
    }

    private static string ResolveText(string key)
    {
        try
        {
            return I18nManager.Instance.GetResource(key) ?? key;
        }
        catch
        {
            return key;
        }
    }

    private static void OpenReleasePage(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
        }
        catch
        {
            // 打不开浏览器就忽略，通知里已给出提示
        }
    }
}
