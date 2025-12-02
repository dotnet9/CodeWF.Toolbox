using Avalonia.Markup.Xaml;
using CodeWF.AvaloniaControls.Helpers;
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
        this.EnableOSVersionAwareDecorations();
        applicationService.Load();
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
