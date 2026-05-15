using System;

namespace CodeWF.Toolbox.Services;

public interface ILoginService
{
    /// <summary>
    /// 登录状态变化。
    /// </summary>
    event EventHandler? LoginStateChanged;

    /// <summary>
    /// 获取当前是否已登录
    /// </summary>
    bool IsLoggedIn { get; }

    /// <summary>
    /// 获取当前登录的用户名
    /// </summary>
    string? CurrentUsername { get; }

    /// <summary>
    /// App.config 中是否配置了本地登录用户名和密码哈希。
    /// </summary>
    bool HasConfiguredCredentials { get; }

    /// <summary>
    /// App.config 中配置的本地用户名。
    /// </summary>
    string? ConfiguredUsername { get; }

    /// <summary>
    /// 默认演示密码。仅用于默认配置时填充登录框，配置中仍只保存 MD5。
    /// </summary>
    string DefaultPassword { get; }

    /// <summary>
    /// 当前配置是否使用内置默认账号。
    /// </summary>
    bool IsUsingDefaultCredentials { get; }

    /// <summary>
    /// 执行登录操作
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="password">密码</param>
    /// <returns>登录是否成功</returns>
    bool Login(string username, string password);

    /// <summary>
    /// 执行登出操作
    /// </summary>
    void Logout();

    /// <summary>
    /// 验证用户凭据
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="password">密码</param>
    /// <returns>凭据是否有效</returns>
    bool ValidateCredentials(string username, string password);
}
