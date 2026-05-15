using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CodeWF.Toolbox.Services;
using ReactiveUI;
using System;
using System.Threading.Tasks;

namespace CodeWF.Toolbox.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private readonly ILoginService _loginService;

    // 关闭登录窗口时用它区分“登录成功关闭”和“用户主动关闭”。
    private bool _isSuccess;

    public string? Username 
    {    
        get; 
        set=>this.RaiseAndSetIfChanged(ref field, value);
    } 
    public string Password
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = string.Empty;
    public string StatusMessage
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    public bool IsConnected
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public LoginViewModel(ILoginService loginService)
    {
        _loginService = loginService;
        Username = _loginService.ConfiguredUsername;
        Password = _loginService.IsUsingDefaultCredentials ? _loginService.DefaultPassword : string.Empty;
        StatusMessage = _loginService.HasConfiguredCredentials
            ? Localization.LoginWindow.LocalLoginReady
            : Localization.LoginWindow.MissingCredentials;
    }

    /// <summary>
    /// 登录方法
    /// </summary>
    public async Task<bool> LoginAsync(Window owner)
    {
        if (!_loginService.HasConfiguredCredentials)
        {
            StatusMessage = Localization.LoginWindow.MissingCredentials;
            return false;
        }

        bool isValid = _loginService.Login(Username ?? string.Empty, Password);
        IsConnected = isValid;
        StatusMessage = isValid ? Localization.LoginWindow.LoginSuccess : Localization.LoginWindow.LoginFailed;
        
        if (isValid)
        {
            _isSuccess = true;
            
            await Task.Delay(250);
            ShowMainWindow();
            owner.Close(); 
        }
        
        return isValid;
    }

    public async Task RaiseClosingHandlerAsync()
    {
        if(!_isSuccess)
        {
            Environment.Exit(0);
        }
    }

    private void ShowMainWindow()
    {
        if (App.Instance.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow?.Show();
        }
    }
}
