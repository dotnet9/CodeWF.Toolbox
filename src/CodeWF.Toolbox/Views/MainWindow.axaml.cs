using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CodeWF.AvaloniaControls.Controls;
using CodeWF.AvaloniaControls.Helpers;
using CodeWF.Core.Helpers;
using CodeWF.Core.IServices;
using CodeWF.EventBus;
using CodeWF.Toolbox.Commands;
using CodeWF.Toolbox.Diagnostics;
using CodeWF.Toolbox.ViewModels;
using Lang.Avalonia;
using Prism.Ioc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CodeWF.Toolbox.Views;

public partial class MainWindow : CodeWFWindow
{
    private readonly IApplicationService _applicationService;
    private readonly IFileChooserService _fileChooserService;
    private readonly INotificationService _notificationService;

    public MainWindow(IApplicationService applicationService, IFileChooserService fileChooserService,
        INotificationService notificationService, TitleBarSettingsViewModel titleBarSettingsViewModel)
    {
        try
        {
            StartupDiagnostics.Log("MainWindow.ctor: started.");
            _applicationService = applicationService;
            _fileChooserService = fileChooserService;
            _notificationService = notificationService;
            DataContext = titleBarSettingsViewModel;
            InitializeComponent();
            RunOptionalStartupStep("MainWindow.Init", Init);
            RunOptionalStartupStep("MainWindow.AdjustWindowSize", AdjustWindowSize);
            RunOptionalStartupStep("MainWindow.ApplyRenderOptions", ApplyRenderOptions);
            StartupDiagnostics.Log("MainWindow.ctor: completed.");
        }
        catch (Exception exception)
        {
            StartupDiagnostics.LogException("MainWindow.ctor", exception);
            throw;
        }
    }

    private void ApplyRenderOptions()
    {
        RenderOptions.SetTextRenderingMode(this, TextRenderingMode.SubpixelAntialias);
        RenderOptions.SetBitmapInterpolationMode(this, BitmapInterpolationMode.HighQuality);
        RenderOptions.SetEdgeMode(this, EdgeMode.Antialias);
    }

    private void InitializeComponent()
    {
        StartupDiagnostics.Log("MainWindow.InitializeComponent: loading XAML.");
        AvaloniaXamlLoader.Load(this);
        StartupDiagnostics.Log("MainWindow.InitializeComponent: XAML loaded.");
        RunOptionalStartupStep("MainWindow.SetFileChooserHost", () => _fileChooserService.SetHostWindow(this));
        RunOptionalStartupStep("MainWindow.SetNotificationHost", () => _notificationService.SetHostWindow(this));
    }

    private void Init()
    {
        EventBus.EventBus.Default.Subscribe(this);
        ChangeApplicationStatus(new ChangeApplicationStatusCommand());
    }

    private async void OpenSettingButton_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var settingView = ContainerLocator.Container.Resolve<SettingView>();
        await settingView.ShowDialog(this);
    }

    private void Search_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is TitleBarSettingsViewModel viewModel)
        {
            viewModel.CommitSearchText();
        }
    }

    private void Search_OnLostFocus(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is TitleBarSettingsViewModel viewModel)
        {
            viewModel.CommitSearchText();
        }
    }

    private void Search_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox { SelectedItem: string keyword }
            && DataContext is TitleBarSettingsViewModel viewModel)
        {
            viewModel.SearchText = keyword;
            viewModel.CommitSearchText();
        }
    }

    [EventHandler]
    private void ChangeApplicationStatus(ChangeApplicationStatusCommand command)
    {
        try
        {
            var icon = TrayIcon.GetIcons(App.Instance)?.FirstOrDefault();
            if (icon == null)
            {
                return;
            }

            icon.IsVisible = _applicationService.HideTrayIconOnClose;
        }
        catch (Exception exception)
        {
            StartupDiagnostics.LogException("MainWindow.ChangeApplicationStatus", exception);
        }
    }

    protected override async void OnClosing(WindowClosingEventArgs e)
    {
        e.Cancel = true;

        var shouldClose = true;
        if (_applicationService.NeedExitDialogOnClose)
        {
            shouldClose = await ShowOptionDialogAsync();
        }

        if (!shouldClose)
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

    private async Task<bool> ShowOptionDialogAsync()
    {
        var vm = new ExitOptionViewModel()
        {
            HideTrayIconOnClose = _applicationService.HideTrayIconOnClose,
            NeedExitDialogOnClose = _applicationService.NeedExitDialogOnClose
        };

        var dialog = new ExitOptionWindow
        {
            DataContext = vm
        };
        var result = await dialog.ShowDialog<bool>(this);
        if (!result)
        {
            return false;
        }

        _applicationService.HideTrayIconOnClose = vm.HideTrayIconOnClose;
        _applicationService.NeedExitDialogOnClose = vm.NeedExitDialogOnClose;

        return true;
    }

    private void AdjustWindowSize()
    {
        if (Screens.Primary is not { } screen)
        {
            return;
        }

        const double preferredWidth = 1180;
        const double preferredHeight = 760;
        const double minimumWidth = 1024;
        const double minimumHeight = 640;
        const double screenMargin = 80;

        var availableWidth = Math.Max(800, screen.WorkingArea.Width - screenMargin);
        var availableHeight = Math.Max(560, screen.WorkingArea.Height - screenMargin);

        Width = Math.Min(preferredWidth, availableWidth);
        Height = Math.Min(preferredHeight, availableHeight);
        MinWidth = Math.Min(minimumWidth, Width);
        MinHeight = Math.Min(minimumHeight, Height);
    }

    private static void RunOptionalStartupStep(string stage, Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            StartupDiagnostics.LogException(stage, exception);
        }
    }
}
