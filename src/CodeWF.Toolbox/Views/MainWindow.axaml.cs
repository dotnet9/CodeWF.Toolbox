using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaXmlTranslator;
using CodeWF.Core.IServices;
using CodeWF.Toolbox.Commands;
using CodeWF.Toolbox.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
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

        var dialogResult = DialogResult.OK;
        // Do not close directly, hide in tray
        if (_applicationService.HideTrayIconOnClose)
        {
            if (_applicationService.NeedExitDialogOnClose)
            {
                dialogResult = await ShowOptionDialogAsync(I18nManager.Instance.GetResource(Localization.MainWindow.FindInTrayIcon),
                    DialogMode.Info,
                    DialogButton.OKCancel);
            }

            if (dialogResult != DialogResult.OK)
            {
                return;
            }

            Hide();
            return;
        }

        // Close directly
        if (_applicationService.NeedExitDialogOnClose)
        {
            dialogResult = await ShowOptionDialogAsync(I18nManager.Instance.GetResource(Localization.MainWindow.SureExit), DialogMode.Warning,
                DialogButton.OKCancel);
        }

        if (dialogResult != DialogResult.OK)
        {
            return;
        }

        Environment.Exit(0);
    }

    private async Task<DialogResult> ShowOptionDialogAsync(string message, DialogMode mode, DialogButton button)
    {
        var options = new DialogOptions()
        {
            Title = I18nManager.Instance.GetResource(Localization.MainModule.Exit),
            Mode = mode,
            Button = button,
            ShowInTaskBar = false,
            IsCloseButtonVisible = true,
            StartupLocation = WindowStartupLocation.CenterOwner,
            CanDragMove = false,
            CanResize = false,
            StyleClass = default,
        };
        var vm = new ExitOptionViewModel()
        {
            Message = message,
            Option = !_applicationService.NeedExitDialogOnClose,
            OptionContent = I18nManager.Instance.GetResource(Localization.MainWindow.NoMorePrompts)
        };
        var result = await Dialog.ShowModal<ExitOptionView, ExitOptionViewModel>(vm, options: options);
        _applicationService.NeedExitDialogOnClose = !vm.Option;

        return result;
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