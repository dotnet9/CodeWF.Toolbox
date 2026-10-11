namespace CodeWF.Core.IServices;

/// <summary>检查更新结果，区分无更新和检查失败。</summary>
public sealed record UpdateCheckResult(UpdateInfo? Update, bool Succeeded, string? Error)
{
    public static UpdateCheckResult Latest() => new(null, true, null);

    public static UpdateCheckResult Failed(string error) => new(null, false, error);
}

/// <summary>发现新版本时的信息；当前仅提醒并打开发布页，不做应用内下载。</summary>
public sealed record UpdateInfo(
    Version Version,
    string Tag,
    string PageUrl,
    string? AssetUrl = null,
    string? AssetName = null,
    string? ChecksumUrl = null);

/// <summary>检查 GitHub Releases 更新。</summary>
public interface IUpdateChecker
{
    Task<UpdateCheckResult> CheckAsync(Version current, CancellationToken cancellationToken = default);
}
