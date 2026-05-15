using Avalonia.Controls;
using Avalonia.Interactivity;
using CodeWF.Toolbox.ViewModels;
using Prism.Ioc;

namespace CodeWF.Toolbox.Views;

public partial class MainMenuView : UserControl
{
    public MainMenuView()
    {
        InitializeComponent();
    }

    private async void OpenSettingButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainMenuViewModel viewModel)
        {
            await viewModel.RaiseOpenSettingHandlerAsync();
            return;
        }

        var settingView = ContainerLocator.Container.Resolve<SettingView>();
        if (App.Instance.MainWindow is Window owner)
        {
            await settingView.ShowDialog(owner);
            return;
        }

        settingView.Show();
    }
}
