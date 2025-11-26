using Avalonia.Markup.Xaml;
using CodeWF.Core.IServices;
using CodeWF.Toolbox.Services;
using CodeWF.Toolbox.ViewModels;
using Ursa.Controls;

namespace CodeWF.Toolbox.Views;

public partial class LoginWindow : UrsaWindow
{
    public LoginWindow(IApplicationService applicationService, ILoginService loginService)
    {
        DataContext = new LoginViewModel(loginService);
        InitializeComponent();
        applicationService.Load();
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
