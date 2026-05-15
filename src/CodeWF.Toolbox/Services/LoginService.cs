using CodeWF.Tools.Helpers;
using System;
using System.Security.Cryptography;
using System.Text;

namespace CodeWF.Toolbox.Services;

public class LoginService : ILoginService
{
    private const string LoginUsernameKey = "LoginUsername";
    private const string LoginPasswordHashKey = "LoginPasswordHash";
    private const string BuiltInDefaultUsername = "CodeWF";
    private const string BuiltInDefaultPassword = "codewf.com";
    private const string BuiltInDefaultPasswordHash = "37b9587481f58ad8681396d6fae12744";

    private bool _isLoggedIn;
    private string? _currentUsername;

    public event EventHandler? LoginStateChanged;

    public bool IsLoggedIn => _isLoggedIn;

    public string? CurrentUsername => _currentUsername;

    public bool HasConfiguredCredentials =>
        !string.IsNullOrWhiteSpace(ConfiguredUsername)
        && !string.IsNullOrWhiteSpace(ConfiguredPasswordHash);

    public string? ConfiguredUsername => GetConfigValue(LoginUsernameKey);

    public string DefaultPassword => BuiltInDefaultPassword;

    public bool IsUsingDefaultCredentials =>
        string.Equals(ConfiguredUsername, BuiltInDefaultUsername, StringComparison.Ordinal)
        && string.Equals(NormalizeHash(ConfiguredPasswordHash), BuiltInDefaultPasswordHash, StringComparison.OrdinalIgnoreCase);

    private static string? ConfiguredPasswordHash => GetConfigValue(LoginPasswordHashKey);

    public bool Login(string username, string password)
    {
        if (ValidateCredentials(username, password))
        {
            _isLoggedIn = true;
            _currentUsername = username.Trim();
            App.IsLoggedIn = true;
            LoginStateChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }

        return false;
    }

    public void Logout()
    {
        _isLoggedIn = false;
        _currentUsername = null;
        App.IsLoggedIn = false;
        LoginStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool ValidateCredentials(string username, string password)
    {
        if (!HasConfiguredCredentials)
        {
            return false;
        }

        return string.Equals(username.Trim(), ConfiguredUsername, StringComparison.Ordinal)
               && string.Equals(ComputeMd5(password), NormalizeHash(ConfiguredPasswordHash), StringComparison.OrdinalIgnoreCase);
    }

    private static string? GetConfigValue(string key)
    {
        try
        {
            return AppConfigHelper.TryGet<string>(key, out var value)
                ? value?.Trim()
                : null;
        }
        catch
        {
            return null;
        }
    }

    private static string ComputeMd5(string value)
    {
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string? NormalizeHash(string? hash)
    {
        return string.IsNullOrWhiteSpace(hash)
            ? null
            : hash.Trim().Replace("-", string.Empty).ToLowerInvariant();
    }
}
