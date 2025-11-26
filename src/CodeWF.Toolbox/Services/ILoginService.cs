namespace CodeWF.Toolbox.Services;

public interface ILoginService
{
    /// <summary>
    /// 获取当前是否已登录
    /// </summary>
    bool IsLoggedIn { get; }

    /// <summary>
    /// 获取当前登录的用户名
    /// </summary>
    string? CurrentUsername { get; }

    /// <summary>
    /// 执行登录操作
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="password">密码</param>
    /// <returns>登录是否成功</returns>
    bool Login(string username, string password);

    /// <summary>
    /// 执行游客登录
    /// </summary>
    void GuestLogin();

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
