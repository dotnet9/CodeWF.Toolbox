using CodeWF.Core.Models;

namespace CodeWF.Core.IServices;

public interface IUserProfileService
{
    event EventHandler? ProfileChanged;

    string? CurrentUsername { get; }

    string? CurrentProfileDirectory { get; }

    IReadOnlyList<string> SearchHistory { get; }

    IReadOnlyList<UserToolUsage> FrequentTools { get; }

    string EnsureProfileDirectory(string username);

    void Activate(string username);

    void Deactivate();

    void RecordSearch(string keyword);

    void RecordToolUsage(ToolMenuItem toolMenuItem);

    IReadOnlyDictionary<string, string> GetToolFieldValues(string toolId);

    void SaveToolFieldValues(string toolId, IReadOnlyDictionary<string, string> values);
}
