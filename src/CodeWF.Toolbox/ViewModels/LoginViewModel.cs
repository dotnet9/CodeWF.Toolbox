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

    public string ConfirmPassword
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

    public bool IsRegisterMode
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            this.RaisePropertyChanged(nameof(PrimaryActionText));
            this.RaisePropertyChanged(nameof(SecondaryActionText));
            this.RaisePropertyChanged(nameof(ModeTitleText));
        }
    }

    public string ModeTitleText =>
        IsRegisterMode ? Localization.LoginWindow.RegisterButton : Localization.LoginWindow.LoginButton;

    public string PrimaryActionText =>
        IsRegisterMode ? Localization.LoginWindow.RegisterButton : Localization.LoginWindow.LoginButton;

    public string SecondaryActionText =>
        IsRegisterMode ? Localization.LoginWindow.SwitchToLogin : Localization.LoginWindow.SwitchToRegister;

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
    public async Task<bool> PrimaryActionAsync(Window owner)
    {
        return IsRegisterMode ? await RegisterAsync(owner) : await LoginAsync(owner);
    }

    public async Task<bool> LoginAsync(Window owner)
    {
        if (!_loginService.HasConfiguredCredentials)
        {
            StatusMessage = Localization.LoginWindow.MissingCredentials;
            return false;
        }

        bool isValid = _loginService.Login(Username ?? string.Empty, Password, out var statusMessage);
        IsConnected = isValid;
        StatusMessage = statusMessage;
        
        if (isValid)
        {
            _isSuccess = true;
            
            await Task.Delay(250);
            ShowMainWindow();
            owner.Close(); 
        }
        
        return isValid;
    }

    public async Task<bool> RegisterAsync(Window owner)
    {
        if (!string.Equals(Password, ConfirmPassword, StringComparison.Ordinal))
        {
            StatusMessage = Localization.LoginWindow.PasswordMismatch;
            return false;
        }

        var isRegistered = _loginService.Register(Username ?? string.Empty, Password, out var statusMessage);
        StatusMessage = statusMessage;
        if (!isRegistered)
        {
            return false;
        }

        await Task.Delay(200);
        return await LoginAsync(owner);
    }

    public void ToggleRegisterMode()
    {
        IsRegisterMode = !IsRegisterMode;
        StatusMessage = IsRegisterMode
            ? Localization.LoginWindow.RegisterHint
            : (_loginService.HasConfiguredCredentials
                ? Localization.LoginWindow.LocalLoginReady
                : Localization.LoginWindow.MissingCredentials);
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
