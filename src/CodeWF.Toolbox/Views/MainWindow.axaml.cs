using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CodeWF.Toolbox.Services;

namespace CodeWF.Toolbox.Views;

public partial class MainWindow : Window
{
    public MainWindow(IApplicationService applicationService)
    {
        InitializeComponent();
        AdjustWindowSize();
        applicationService.Load();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
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