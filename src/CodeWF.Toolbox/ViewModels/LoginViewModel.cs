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

    // 登录成功事件
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
    }
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
        // 初始化状态信息
        StatusMessage = Localization.LoginWindow.NotConnected;
    }

    /// <summary>
    /// 登录方法
    /// </summary>
    public async Task<bool> LoginAsync(Window owner)
    {        
        // 使用登录服务进行验证
        bool isValid = _loginService.Login(Username, Password);
        IsConnected = isValid;
        StatusMessage = isValid ? Localization.LoginWindow.LoginSuccess : Localization.LoginWindow.LoginFailed;
        
        if (isValid)
        {
            // 触发登录成功事件
            _isSuccess = true;
            
            // 登录成功，准备关闭窗口
            await Task.Delay(500); // 给用户一点时间看到成功消息
            ShowMainWindow();
            owner.Close(); 
        }
        
        return isValid;
    }

    /// <summary>
    /// 游客登录方法
    /// </summary>
    public async Task GuestLoginAsync(Window owner)
    {        
        // 使用登录服务进行游客登录
        _loginService.GuestLogin();
        Username = "guest";
        IsConnected = true;
        StatusMessage = Localization.LoginWindow.GuestLoginSuccess;

        // 触发登录成功事件
        _isSuccess = true;
        
        // 登录成功，准备关闭窗口
        await Task.Delay(500); // 给用户一点时间看到成功消息
        ShowMainWindow();
        owner.Close();
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
