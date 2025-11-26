namespace CodeWF.Toolbox.Services;

public class LoginService : ILoginService
{
    private bool _isLoggedIn = false;
    private string? _currentUsername = null;

    public bool IsLoggedIn => _isLoggedIn;

    public string? CurrentUsername => _currentUsername;

    public bool Login(string username, string password)
    {
        if (ValidateCredentials(username, password))
        {
            _isLoggedIn = true;
            _currentUsername = username;
            // 更新全局登录状态
            App.IsLoggedIn = true;
            return true;
        }
        return false;
    }

    public void GuestLogin()
    {
        _isLoggedIn = true;
        _currentUsername = "guest";
        App.IsLoggedIn = true;
    }

    public void Logout()
    {
        _isLoggedIn = false;
        _currentUsername = null;
        App.IsLoggedIn = false;
    }

    public bool ValidateCredentials(string username, string password)
    {
        // 简单验证逻辑：用户名admin，密码123456
        return username == "admin" && password == "123456";
    }
}
