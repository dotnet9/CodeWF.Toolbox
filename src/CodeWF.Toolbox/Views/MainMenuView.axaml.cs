using global::Avalonia.Controls;
using global::Avalonia.Interactivity;
using CodeWF.Toolbox.ViewModels;
using Prism.Ioc;
using CodeWF.Toolbox.Services;

namespace CodeWF.Toolbox.Views;

public partial class MainMenuView : UserControl
{
    public MainMenuView()
    {
        InitializeComponent();
    }

    private void SplitDocument(object? sender, RoutedEventArgs e) => ContainerLocator.Container.Resolve<ToolWorkspace>().SplitActive();
    private void CloseAllDocuments(object? sender, RoutedEventArgs e) => ContainerLocator.Container.Resolve<ToolWorkspace>().CloseAll();

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
