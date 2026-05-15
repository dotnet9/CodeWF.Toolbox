using CodeWF.Tools.Helpers;
using CodeWF.Core.IServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;

namespace CodeWF.Toolbox.Services;

public partial class LoginService : ILoginService
{
    private const string LoginUsernameKey = "LoginUsername";
    private const string LoginPasswordHashKey = "LoginPasswordHash";
    private const string BuiltInDefaultUsername = "CodeWF";
    private const string BuiltInDefaultPassword = "codewf.com";
    private const string BuiltInDefaultPasswordHash = "37b9587481f58ad8681396d6fae12744";
    private readonly IUserProfileService _userProfileService;
    private readonly object _syncRoot = new();

    private bool _isLoggedIn;
    private string? _currentUsername;
    private FileStream? _accountLockStream;
    private Mutex? _accountMutex;

    public event EventHandler? LoginStateChanged;

    public LoginService(IUserProfileService userProfileService)
    {
        _userProfileService = userProfileService;
        EnsureSeedAccount();
    }

    public bool IsLoggedIn => _isLoggedIn;

    public string? CurrentUsername => _currentUsername;

    public bool HasConfiguredCredentials => LoadAccountStore().Accounts.Count > 0;

    public string? ConfiguredUsername =>
        GetConfigValue(LoginUsernameKey) ?? LoadAccountStore().Accounts.FirstOrDefault()?.Username;

    public string DefaultPassword => BuiltInDefaultPassword;

    public bool IsUsingDefaultCredentials =>
        LoadAccountStore().Accounts.Any(account =>
            string.Equals(account.Username, BuiltInDefaultUsername, StringComparison.Ordinal)
            && string.Equals(account.PasswordHash, BuiltInDefaultPasswordHash, StringComparison.OrdinalIgnoreCase));

    public bool Login(string username, string password, out string statusMessageKey)
    {
        username = username.Trim();
        if (!ValidateCredentials(username, password))
        {
            statusMessageKey = Localization.LoginWindow.LoginFailed;
            return false;
        }

        if (!TryAcquireAccountLock(username))
        {
            statusMessageKey = Localization.LoginWindow.AccountAlreadyRunning;
            return false;
        }

        _isLoggedIn = true;
        _currentUsername = username;
        App.IsLoggedIn = true;
        _userProfileService.Activate(username);
        LoginStateChanged?.Invoke(this, EventArgs.Empty);

        statusMessageKey = Localization.LoginWindow.LoginSuccess;
        return true;
    }

    public bool Register(string username, string password, out string statusMessageKey)
    {
        username = username.Trim();
        if (!IsValidUsername(username) || string.IsNullOrWhiteSpace(password))
        {
            statusMessageKey = Localization.LoginWindow.InvalidRegistration;
            return false;
        }

        lock (_syncRoot)
        {
            var store = LoadAccountStore();
            if (store.Accounts.Any(account => string.Equals(account.Username, username, StringComparison.OrdinalIgnoreCase)))
            {
                statusMessageKey = Localization.LoginWindow.UserAlreadyExists;
                return false;
            }

            store.Accounts.Add(new LoginAccountRecord
            {
                Username = username,
                PasswordHash = ComputeMd5(password),
                CreatedAt = DateTimeOffset.UtcNow
            });
            SaveAccountStore(store);
            _userProfileService.EnsureProfileDirectory(username);
        }

        statusMessageKey = Localization.LoginWindow.RegisterSuccess;
        return true;
    }

    public void Logout()
    {
        _isLoggedIn = false;
        _currentUsername = null;
        App.IsLoggedIn = false;
        _userProfileService.Deactivate();
        ReleaseAccountLock();
        LoginStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool ValidateCredentials(string username, string password)
    {
        var store = LoadAccountStore();
        var account = store.Accounts.FirstOrDefault(item =>
            string.Equals(item.Username, username.Trim(), StringComparison.Ordinal));
        if (account == null)
        {
            return false;
        }

        return string.Equals(ComputeMd5(password), NormalizeHash(account.PasswordHash), StringComparison.OrdinalIgnoreCase);
    }

    private void EnsureSeedAccount()
    {
        var username = GetConfigValue(LoginUsernameKey);
        var passwordHash = NormalizeHash(GetConfigValue(LoginPasswordHashKey));
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(passwordHash))
        {
            return;
        }

        lock (_syncRoot)
        {
            var store = LoadAccountStore();
            if (store.Accounts.Any(account => string.Equals(account.Username, username, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            store.Accounts.Add(new LoginAccountRecord
            {
                Username = username,
                PasswordHash = passwordHash,
                CreatedAt = DateTimeOffset.UtcNow
            });
            SaveAccountStore(store);
            _userProfileService.EnsureProfileDirectory(username);
        }
    }

    private bool TryAcquireAccountLock(string username)
    {
        ReleaseAccountLock();

        var lockName = $"CodeWF.Toolbox.Account.{ComputeSha256(username)}";
        try
        {
            _accountMutex = new Mutex(true, lockName, out var createdNew);
            if (!createdNew)
            {
                ReleaseAccountLock();
                return false;
            }
        }
        catch
        {
            _accountMutex = null;
        }

        try
        {
            var directory = _userProfileService.EnsureProfileDirectory(username);
            var lockPath = Path.Combine(directory, ".session.lock");
            _accountLockStream = new FileStream(lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            _accountLockStream.SetLength(0);
            var payload = Encoding.UTF8.GetBytes($"{Environment.ProcessId}{Environment.NewLine}{DateTimeOffset.UtcNow:O}");
            _accountLockStream.Write(payload);
            _accountLockStream.Flush(true);
            return true;
        }
        catch (IOException)
        {
            ReleaseAccountLock();
            return false;
        }
        catch
        {
            ReleaseAccountLock();
            throw;
        }
    }

    private void ReleaseAccountLock()
    {
        _accountLockStream?.Dispose();
        _accountLockStream = null;

        try
        {
            _accountMutex?.ReleaseMutex();
        }
        catch
        {
            // ignored
        }

        _accountMutex?.Dispose();
        _accountMutex = null;
    }

    private LoginAccountStore LoadAccountStore()
    {
        var path = GetAccountStorePath();
        if (!File.Exists(path))
        {
            return new LoginAccountStore();
        }

        try
        {
            var store = JsonSerializer.Deserialize(File.ReadAllText(path), LoginJsonContext.Default.LoginAccountStore)
                        ?? new LoginAccountStore();
            store.Accounts ??= [];
            return store;
        }
        catch
        {
            return new LoginAccountStore();
        }
    }

    private void SaveAccountStore(LoginAccountStore store)
    {
        var path = GetAccountStorePath();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(store, LoginJsonContext.Default.LoginAccountStore));
    }

    private static string GetAccountStorePath()
    {
        return Path.Combine(AppContext.BaseDirectory, "Users", "accounts.json");
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

    private static string ComputeSha256(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value.ToUpperInvariant()));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string? NormalizeHash(string? hash)
    {
        return string.IsNullOrWhiteSpace(hash)
            ? null
            : hash.Trim().Replace("-", string.Empty).ToLowerInvariant();
    }

    private static bool IsValidUsername(string username)
    {
        return UsernameRegex().IsMatch(username);
    }

    [GeneratedRegex(@"^[A-Za-z0-9._@-]{2,64}$")]
    private static partial Regex UsernameRegex();

}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(LoginAccountStore))]
internal partial class LoginJsonContext : JsonSerializerContext;

internal sealed class LoginAccountStore
{
    public List<LoginAccountRecord> Accounts { get; set; } = [];
}

internal sealed class LoginAccountRecord
{
    public string Username { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
