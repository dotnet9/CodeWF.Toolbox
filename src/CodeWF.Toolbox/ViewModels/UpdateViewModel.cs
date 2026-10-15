using System.Diagnostics;
using Avalonia.Controls.ApplicationLifetimes;
using CodeWF.Toolkit.Core.UpdateChecking;
using CodeWF.Toolkit.Lang;
using CodeWF.Toolbox.Commands;
using Language = Localization.UpdateFlow;
using AppText = CodeWF.Toolbox.Services.ToolboxText;
using System;
using System.Threading;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Mvvm;

namespace CodeWF.Toolbox.ViewModels;

/// <summary>界面只维护更新状态；网络、下载、校验和安装复用 Toolkit。</summary>
public sealed class UpdateViewModel : BindableBase, IDisposable
{
    private readonly IUpdateService _updates;
    private readonly bool _ownsService;
    private readonly Action<string> _openUrl;
    private readonly Action _exit;
    private CancellationTokenSource? _operation;
    private UpdateInfo? _update;
    private UpdateDownloadResult? _download;
    private UpdateState _state;
    private UpdateState _retry;
    private string? _error;
    private double? _progress;
    private bool _disposed;
    private UpdateInstallResult? _install;

    public UpdateViewModel(IUpdateService? updates = null, Action<string>? openUrl = null, Action? exit = null)
    {
        _updates = updates ?? new UpdateService(new UpdateServiceOptions { Repository = "CodeWF.Toolbox", ProductName = "CodeWF.Toolbox" });
        _ownsService = updates is null;
        _openUrl = openUrl ?? (url => Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }));
        _exit = exit ?? (() => (global::Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Shutdown());
        UpdateCommand = new AsyncDelegateCommand(AdvanceAsync, () => !IsBusy && !_disposed);
        CheckCommand = new AsyncDelegateCommand(CheckAsync, () => !IsBusy && !_disposed);
        CancelCommand = new DelegateCommand(Cancel);
        RepositoryCommand = new DelegateCommand(() => OpenUrl("https://github.com/dotnet9/CodeWF.Toolbox"));
        I18nManager.Instance.CultureChanged += OnCultureChanged;
    }

    public AsyncDelegateCommand UpdateCommand { get; }
    public AsyncDelegateCommand CheckCommand { get; }
    public DelegateCommand CancelCommand { get; }
    public DelegateCommand RepositoryCommand { get; }
    public UpdateState State => _state;
    public bool IsBusy => _state is UpdateState.Checking or UpdateState.Downloading or UpdateState.Installing;
    public bool IsDownloading => _state == UpdateState.Downloading;
    public bool CanRecheck => _state is UpdateState.Available or UpdateState.Downloaded or UpdateState.Failed;
    public bool IsIndeterminate => !_progress.HasValue;
    public double Progress => _progress ?? 0;
    public string ProgressText => _progress.HasValue ? (_progress.Value / 100).ToString("P0", AppText.Culture) : string.Empty;
    public string ActionText => AppText.Get(_state switch
    {
        UpdateState.Checking => Language.AboutChecking,
        UpdateState.Downloading => Language.AboutDownloading,
        UpdateState.Installing => Language.AboutInstalling,
        UpdateState.Available when !CanDownload => Language.UpdateReleasePage,
        UpdateState.Available => Language.UpdateDownload,
        UpdateState.Downloaded when _download?.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) == true => Language.AboutOpenFolder,
        UpdateState.Downloaded => Language.UpdateInstall,
        UpdateState.Failed => Language.AboutRetry,
        _ => Language.UpdateCheck
    });
    public string StatusText => _state switch
    {
        UpdateState.Checking => AppText.Get(Language.AboutChecking),
        UpdateState.Current => AppText.Get(Language.AboutCurrent),
        UpdateState.Available => AppText.Format(Language.AboutAvailable, _update!.Tag),
        UpdateState.Downloading => AppText.Format(Language.AboutDownloadProgress, ProgressText),
        UpdateState.Downloaded => AppText.Get(Language.AboutDownloaded),
        UpdateState.Failed => AppText.Format(Language.AboutFailed, _error),
        UpdateState.Canceled => AppText.Get(Language.AboutCanceled),
        UpdateState.Installed => _install?.Message ?? AppText.Get(Language.AboutInstalled),
        _ => string.Empty
    };
    public string ReleaseNotes => _update?.Notes ?? string.Empty;
    private bool CanDownload => !string.IsNullOrWhiteSpace(_update?.AssetUrl) && !string.IsNullOrWhiteSpace(_update.ChecksumUrl);

    public Task AdvanceAsync()
    {
        if (IsBusy || _disposed) return Task.CompletedTask;
        if (_state == UpdateState.Failed && _retry == UpdateState.Downloading && _update != null) return DownloadAsync();
        if (_state == UpdateState.Failed && _retry == UpdateState.Installing && _download != null) return InstallAsync();
        if (_state == UpdateState.Available)
        {
            if (CanDownload) return DownloadAsync();
            OpenUrl(_update!.PageUrl);
            return Task.CompletedTask;
        }
        return _state == UpdateState.Downloaded ? InstallAsync() : CheckAsync();
    }

    public Task CheckAsync() => RunAsync(UpdateState.Checking, async token =>
    {
        _update = null;
        _download = null;
        _install = null;
        var result = await _updates.CheckAsync(UpdateVersion.Current(typeof(UpdateViewModel).Assembly), token);
        token.ThrowIfCancellationRequested();
        if (!result.Succeeded) { _error = result.Error; SetState(UpdateState.Failed); }
        else { _update = result.Update; SetState(_update is null ? UpdateState.Current : UpdateState.Available); }
    });

    private Task DownloadAsync() => RunAsync(UpdateState.Downloading, async token =>
    {
        _progress = null;
        var progress = new Progress<UpdateDownloadProgress>(value =>
        {
            if (_disposed || !IsDownloading) return;
            _progress = value.Percentage;
            Refresh();
        });
        _download = await _updates.DownloadAsync(_update!, progress, token);
        token.ThrowIfCancellationRequested();
        SetState(UpdateState.Downloaded);
    });

    private Task InstallAsync() => RunAsync(UpdateState.Installing, async token =>
    {
        var installed = _install = await _updates.InstallAsync(_download!, token);
        SetState(UpdateState.Installed);
        if (installed.ShouldExitApplication) _exit();
    });

    private async Task RunAsync(UpdateState state, Func<CancellationToken, Task> action)
    {
        if (IsBusy || _disposed) return;
        using var operation = new CancellationTokenSource();
        _operation = operation;
        _retry = state;
        SetState(state);
        try { await action(operation.Token); }
        catch (OperationCanceledException) when (operation.IsCancellationRequested) { SetState(UpdateState.Canceled); }
        catch (Exception error) { _error = error.Message; SetState(UpdateState.Failed); }
        finally { _operation = null; }
    }

    public void Cancel() => _operation?.Cancel();
    private void OpenUrl(string url)
    {
        try { _openUrl(url); }
        catch (Exception error) { _error = error.Message; SetState(UpdateState.Failed); }
    }
    private void SetState(UpdateState state) { _state = state; Refresh(); }
    private void OnCultureChanged(object? sender, EventArgs args) => Refresh();
    private void Refresh()
    {
        foreach (var name in new[] { nameof(State), nameof(IsBusy), nameof(IsDownloading), nameof(CanRecheck), nameof(IsIndeterminate),
                     nameof(Progress), nameof(ProgressText), nameof(ActionText), nameof(StatusText), nameof(ReleaseNotes) }) RaisePropertyChanged(name);
        UpdateCommand.RaiseCanExecuteChanged();
        CheckCommand.RaiseCanExecuteChanged();
    }
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Cancel();
        I18nManager.Instance.CultureChanged -= OnCultureChanged;
        if (_ownsService && _updates is IDisposable service) service.Dispose();
        Refresh();
    }
}

public enum UpdateState { Idle, Checking, Current, Available, Downloading, Downloaded, Installing, Installed, Failed, Canceled }
