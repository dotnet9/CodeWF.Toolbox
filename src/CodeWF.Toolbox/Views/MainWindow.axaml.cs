using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AvaloniaExtensions.Axaml.Markup;
using CodeWF.Core.IServices;
using CodeWF.Toolbox.Commands;
using CodeWF.Toolbox.I18n;
using System;
using System.Linq;
using Ursa.Controls;

namespace CodeWF.Toolbox.Views;

public partial class MainWindow : UrsaWindow
{
    private readonly IApplicationService _applicationService;
    private readonly IFileChooserService _fileChooserService;
    private readonly INotificationService _notificationService;

    public MainWindow(IApplicationService applicationService, IFileChooserService fileChooserService,
        INotificationService notificationService)
    {
        _applicationService = applicationService;
        _fileChooserService = fileChooserService;
        _notificationService = notificationService;
        InitializeComponent();
        Init();
        AdjustWindowSize();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _fileChooserService.SetHostWindow(this);
        _notificationService.SetHostWindow(this);
    }

    private void Init()
    {
        _applicationService.Load();
        EventBus.EventBus.Default.Subscribe<ChangeApplicationStatusCommand>(ChangeApplicationStatus);
        ChangeApplicationStatus(new ChangeApplicationStatusCommand());
    }

    private void ChangeApplicationStatus(ChangeApplicationStatusCommand command)
    {
        var icon = TrayIcon.GetIcons(App.Instance)?.FirstOrDefault();
        if (icon == null)
        {
            return;
        }

        icon.IsVisible = _applicationService.HideTrayIconOnClose;
    }

    protected override async void OnClosing(WindowClosingEventArgs e)
    {
        e.Cancel = true;

        if (!_applicationService.HideTrayIconOnClose)
        {
            var result = await MessageBox.ShowOverlayAsync(I18nManager.GetString(Language.SureExit),
                I18nManager.GetString(Language.Exit),
                button: MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                if (App.Instance.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    desktop.Shutdown(0);
                }
                else
                {
                    Environment.Exit(0);
                }
            }

            return;
        }

        await MessageBox.ShowOverlayAsync(I18nManager.GetString(Language.FindInTrayIcon),
            I18nManager.GetString(Language.Exit), button: MessageBoxButton.OK);
        Hide();
    }

    private void AdjustWindowSize()
    {
        var screen = Screens.Primary;
        if (screen == null)
        {
            return;
        }

        var width = screen.WorkingArea.Width;
        var height = screen.WorkingArea.Height;

        switch (width)
        {
            case <= 1920 when height <= 1080:
                Width = 800;
                Height = 580;
                break;
            case <= 2560 when height <= 1440:
                Width = 1180;
                Height = 720;
                break;
            case <= 3840 when height <= 2160:
                Width = 1300;
                Height = 900;
                break;
            default:
                Width = 1520;
                Height = 1080;
                break;
        }
    }
}