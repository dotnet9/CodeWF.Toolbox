using Avalonia.Markup.Xaml;
using CodeWF.AvaloniaControls.Controls;
using CodeWF.Core.IServices;
using CodeWF.Toolbox.Services;
using CodeWF.Toolbox.ViewModels;

namespace CodeWF.Toolbox.Views;

public partial class LoginWindow : CodeWFWindow
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
