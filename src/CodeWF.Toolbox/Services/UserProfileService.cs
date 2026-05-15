using CodeWF.Core.IServices;
using CodeWF.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace CodeWF.Toolbox.Services;

internal sealed partial class UserProfileService : IUserProfileService
{
    private const int MaxSearchHistory = 10;
    private const int MaxFrequentTools = 12;
    private readonly object _syncRoot = new();
    private UserProfileData _profile = new();

    public event EventHandler? ProfileChanged;

    public string? CurrentUsername { get; private set; }

    public string? CurrentProfileDirectory { get; private set; }

    public IReadOnlyList<string> SearchHistory => _profile.SearchHistory;

    public IReadOnlyList<UserToolUsage> FrequentTools =>
        _profile.ToolUsage
            .OrderByDescending(tool => tool.Count)
            .ThenByDescending(tool => tool.LastUsedAt)
            .Take(6)
            .ToList();

    public string EnsureProfileDirectory(string username)
    {
        var root = Path.Combine(AppContext.BaseDirectory, "Users");
        Directory.CreateDirectory(root);

        var folderName = SanitizeFolderName(username);
        var directory = Path.Combine(root, folderName);
        Directory.CreateDirectory(directory);
        return directory;
    }

    public void Activate(string username)
    {
        lock (_syncRoot)
        {
            CurrentUsername = username.Trim();
            CurrentProfileDirectory = EnsureProfileDirectory(CurrentUsername);
            _profile = LoadProfile(CurrentProfileDirectory, CurrentUsername);
        }

        ProfileChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Deactivate()
    {
        lock (_syncRoot)
        {
            CurrentUsername = null;
            CurrentProfileDirectory = null;
            _profile = new UserProfileData();
        }

        ProfileChanged?.Invoke(this, EventArgs.Empty);
    }

    public void RecordSearch(string keyword)
    {
        keyword = keyword.Trim();
        if (string.IsNullOrWhiteSpace(keyword) || keyword.Length < 2)
        {
            return;
        }

        lock (_syncRoot)
        {
            if (CurrentProfileDirectory == null)
            {
                return;
            }

            _profile.SearchHistory.RemoveAll(item => string.Equals(item, keyword, StringComparison.CurrentCultureIgnoreCase));
            _profile.SearchHistory.Insert(0, keyword);
            if (_profile.SearchHistory.Count > MaxSearchHistory)
            {
                _profile.SearchHistory.RemoveRange(MaxSearchHistory, _profile.SearchHistory.Count - MaxSearchHistory);
            }

            SaveProfile();
        }

        ProfileChanged?.Invoke(this, EventArgs.Empty);
    }

    public void RecordToolUsage(ToolMenuItem toolMenuItem)
    {
        if (string.IsNullOrWhiteSpace(toolMenuItem.ViewName) || toolMenuItem.Status != ToolStatus.Complete)
        {
            return;
        }

        lock (_syncRoot)
        {
            if (CurrentProfileDirectory == null)
            {
                return;
            }

            var usage = _profile.ToolUsage.FirstOrDefault(tool =>
                string.Equals(tool.ViewName, toolMenuItem.ViewName, StringComparison.OrdinalIgnoreCase));
            if (usage == null)
            {
                usage = new UserToolUsage
                {
                    ViewName = toolMenuItem.ViewName,
                    Name = toolMenuItem.Name,
                    Description = toolMenuItem.Description,
                    Icon = toolMenuItem.Icon
                };
                _profile.ToolUsage.Add(usage);
            }

            usage.Name = toolMenuItem.Name;
            usage.Description = toolMenuItem.Description;
            usage.Icon = toolMenuItem.Icon;
            usage.Count++;
            usage.LastUsedAt = DateTimeOffset.UtcNow;

            _profile.ToolUsage = _profile.ToolUsage
                .OrderByDescending(tool => tool.Count)
                .ThenByDescending(tool => tool.LastUsedAt)
                .Take(MaxFrequentTools)
                .ToList();
            SaveProfile();
        }

        ProfileChanged?.Invoke(this, EventArgs.Empty);
    }

    public IReadOnlyDictionary<string, string> GetToolFieldValues(string toolId)
    {
        lock (_syncRoot)
        {
            return _profile.ToolFieldValues.TryGetValue(toolId, out var values)
                ? new Dictionary<string, string>(values, StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    public void SaveToolFieldValues(string toolId, IReadOnlyDictionary<string, string> values)
    {
        if (string.IsNullOrWhiteSpace(toolId))
        {
            return;
        }

        lock (_syncRoot)
        {
            if (CurrentProfileDirectory == null)
            {
                return;
            }

            _profile.ToolFieldValues[toolId] = new Dictionary<string, string>(values, StringComparer.OrdinalIgnoreCase);
            SaveProfile();
        }
    }

    private UserProfileData LoadProfile(string profileDirectory, string username)
    {
        var path = GetProfilePath(profileDirectory);
        if (!File.Exists(path))
        {
            var data = new UserProfileData { Username = username };
            File.WriteAllText(path, JsonSerializer.Serialize(data, UserProfileJsonContext.Default.UserProfileData));
            return data;
        }

        try
        {
            var profile = JsonSerializer.Deserialize(File.ReadAllText(path), UserProfileJsonContext.Default.UserProfileData)
                          ?? new UserProfileData();
            return NormalizeProfile(profile, username);
        }
        catch
        {
            return new UserProfileData { Username = username };
        }
    }

    private void SaveProfile()
    {
        if (CurrentProfileDirectory == null)
        {
            return;
        }

        _profile.Username = CurrentUsername ?? _profile.Username;
        File.WriteAllText(GetProfilePath(CurrentProfileDirectory), JsonSerializer.Serialize(_profile, UserProfileJsonContext.Default.UserProfileData));
    }

    private static UserProfileData NormalizeProfile(UserProfileData profile, string username)
    {
        profile.Username = string.IsNullOrWhiteSpace(profile.Username) ? username : profile.Username;
        profile.SearchHistory ??= [];
        profile.ToolUsage ??= [];

        var sourceFieldValues = profile.ToolFieldValues ?? [];
        profile.ToolFieldValues = sourceFieldValues.ToDictionary(
            pair => pair.Key,
            pair => new Dictionary<string, string>(
                pair.Value ?? [],
                StringComparer.OrdinalIgnoreCase),
            StringComparer.OrdinalIgnoreCase);
        return profile;
    }

    private static string GetProfilePath(string profileDirectory)
    {
        return Path.Combine(profileDirectory, "profile.json");
    }

    private static string SanitizeFolderName(string username)
    {
        var sanitized = InvalidPathPartRegex().Replace(username.Trim(), "_");
        return string.IsNullOrWhiteSpace(sanitized) ? "user" : sanitized;
    }

    [GeneratedRegex(@"[\\/:*?""<>|]")]
    private static partial Regex InvalidPathPartRegex();

}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(UserProfileData))]
internal partial class UserProfileJsonContext : JsonSerializerContext;

internal sealed class UserProfileData
{
    public string Username { get; set; } = string.Empty;

    public List<string> SearchHistory { get; set; } = [];

    public List<UserToolUsage> ToolUsage { get; set; } = [];

    public Dictionary<string, Dictionary<string, string>> ToolFieldValues { get; set; } =
        new(StringComparer.OrdinalIgnoreCase);
}
