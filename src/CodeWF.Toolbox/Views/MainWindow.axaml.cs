using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CodeWF.AvaloniaControls.Helpers;
using CodeWF.Core.Helpers;
using CodeWF.Core.IServices;
using CodeWF.EventBus;
using CodeWF.Toolbox.Commands;
using CodeWF.Toolbox.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using Lang.Avalonia;
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
        this.EnableOSVersionAwareDecorations();

        RenderOptions.SetTextRenderingMode(this, TextRenderingMode.SubpixelAntialias);
        RenderOptions.SetBitmapInterpolationMode(this, BitmapInterpolationMode.HighQuality);
        RenderOptions.SetEdgeMode(this, EdgeMode.Antialias);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _fileChooserService?.SetHostWindow(this);
        _notificationService?.SetHostWindow(this);
    }

    private void Init()
    {
        EventBus.EventBus.Default.Subscribe(this);
        ChangeApplicationStatus(new ChangeApplicationStatusCommand());
    }

    private void Search_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        EventBus.EventBus.Default.Publish(new SearchToolMenuCommand((sender as TextBox)?.Text));
    }

    [EventHandler]
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
        if (_applicationService.NeedExitDialogOnClose)
        {
            dialogResult = await ShowOptionDialogAsync(DialogMode.Info, DialogButton.OKCancel);
        }

        if (dialogResult != DialogResult.OK)
        {
            return;
        }

        if (_applicationService.HideTrayIconOnClose)
        {
            Hide();
            return;
        }

        Environment.Exit(0);
    }

    private async Task<DialogResult> ShowOptionDialogAsync(DialogMode mode, DialogButton button)
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
            HideTrayIconOnClose = _applicationService.HideTrayIconOnClose,
            NeedExitDialogOnClose = _applicationService.NeedExitDialogOnClose
        };
        var result = await Dialog.ShowModal<ExitOptionView, ExitOptionViewModel>(vm, options: options);
        _applicationService.HideTrayIconOnClose = vm.HideTrayIconOnClose;
        _applicationService.NeedExitDialogOnClose = vm.NeedExitDialogOnClose;

        return result;
    }

    private void AdjustWindowSize()
    {
        if (Screens.Primary is not { } screen)
        {
            return;
        }

        const double resolutionThreshold = 1920 + 50;
        var isSmaller = screen.WorkingArea.Width < resolutionThreshold;
        var targetWidth = isSmaller ? 1440 : 1920;
        var targetHeight = isSmaller ? 810 : 1080;
        MinWidth = Width = Math.Min(targetWidth, screen.WorkingArea.Width);
        MinHeight = Height = Math.Min(targetHeight, screen.WorkingArea.Height);
    }
}
