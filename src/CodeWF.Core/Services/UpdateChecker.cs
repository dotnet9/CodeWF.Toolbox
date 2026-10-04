using System.Net;
using System.Net.Http;
using CodeWF.Core.IServices;

namespace CodeWF.Core.Services;

/// <summary>
/// 通过 GitHub 网页端点检查更新，不碰 api.github.com 的每小时配额（未认证 60 次/小时/IP，
/// 走代理时被同出口用户共享、极易耗尽）：releases/latest 的 302 落点就是最新发布 tag（只读响应头），
/// 需要资产时再经 expanded_assets/{tag}（发布页懒加载资产列表的接口）解析安装包直链。
/// 仓库还没有发布（404）视为已是最新。任何网络/解析异常都吞掉返回失败结果，不打扰用户。
/// </summary>
public sealed class UpdateChecker : IUpdateChecker
{
    private const string DefaultWebBase = "https://github.com";

    private readonly HttpClient _http;
    private readonly string _owner;
    private readonly string _repo;
    private readonly string _webBase;

    public UpdateChecker(string owner, string repo, HttpClient? http = null, string? webBase = null)
    {
        _owner = owner;
        _repo = repo;
        _http = http ?? new HttpClient { Timeout = TimeSpan.FromSeconds(12) };
        _webBase = (webBase ?? DefaultWebBase).TrimEnd('/');
    }

    public async Task<UpdateCheckResult> CheckAsync(Version current, CancellationToken cancellationToken = default)
    {
        try
        {
            string? tag = await FetchLatestTagViaRedirectAsync(cancellationToken).ConfigureAwait(false);
            if (tag is null)
            {
                return UpdateCheckResult.Latest();
            }

            Version? candidate = UpdateVersion.Parse(tag);
            if (!UpdateVersion.IsNewer(candidate, current))
            {
                return UpdateCheckResult.Latest();
            }

            // 资产列表拿不到不影响提醒：降级为只带发布页链接
            GitHubAsset[]? assets = await FetchAssetsAsync(tag, cancellationToken).ConfigureAwait(false);
            (string? assetUrl, string? assetName, string? checksumUrl) = PickAsset(assets);

            return new UpdateCheckResult(new UpdateInfo(
                candidate!,
                tag,
                $"{_webBase}/{_owner}/{_repo}/releases/tag/{tag}",
                assetUrl,
                assetName,
                checksumUrl), true, null);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return UpdateCheckResult.Failed("检查更新超时");
        }
        catch (OperationCanceledException)
        {
            return UpdateCheckResult.Failed("操作已取消");
        }
        catch (Exception ex)
        {
            return UpdateCheckResult.Failed(ex.Message);
        }
    }

    /// <summary>releases/latest 的 302 落点就是 /releases/tag/{tag}；只取响应头，不下载页面。
    /// 404（还没有任何发布）返回 null，按已是最新处理。</summary>
    private async Task<string?> FetchLatestTagViaRedirectAsync(CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{_webBase}/{_owner}/{_repo}/releases/latest");
        request.Headers.TryAddWithoutValidation("User-Agent", "CodeWF-Toolbox-UpdateChecker");
        using HttpResponseMessage response = await _http
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        // HttpClient 自动跟随重定向时最终地址写在 RequestMessage 上；跟随被禁用时读 Location 头
        Uri? finalUri = IsRedirect(response.StatusCode)
            ? response.Headers.Location
            : response.RequestMessage?.RequestUri;
        return ExtractTag(finalUri);
    }

    /// <summary>expanded_assets 是发布页懒加载资产列表的接口；失败返回 null。</summary>
    private async Task<GitHubAsset[]?> FetchAssetsAsync(string tag, CancellationToken cancellationToken)
    {
        try
        {
            string url = $"{_webBase}/{_owner}/{_repo}/releases/expanded_assets/{tag}";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.TryAddWithoutValidation("User-Agent", "CodeWF-Toolbox-UpdateChecker");
            using HttpResponseMessage response = await _http.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            string html = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            return ParseDownloadAssets(html, tag).ToArray();
        }
        catch
        {
            return null;
        }
    }

    private static bool IsRedirect(HttpStatusCode statusCode)
        => statusCode is HttpStatusCode.Moved
            or HttpStatusCode.Found
            or HttpStatusCode.SeeOther
            or HttpStatusCode.TemporaryRedirect
            or HttpStatusCode.PermanentRedirect;

    /// <summary>从 …/releases/tag/{tag} 的落点地址抠出 tag；跟随与未跟随两种响应都要顾及。</summary>
    private static string? ExtractTag(Uri? uri)
    {
        if (uri is null)
        {
            return null;
        }

        const string marker = "/releases/tag/";
        string path = uri.IsAbsoluteUri ? uri.AbsolutePath : uri.OriginalString;
        int start = path.IndexOf(marker, StringComparison.Ordinal);
        if (start < 0)
        {
            return null;
        }

        string tag = Uri.UnescapeDataString(path[(start + marker.Length)..]).TrimEnd('/');
        return tag.Length > 0 ? tag : null;
    }

    /// <summary>从 expanded_assets 的 HTML 里抠出 /releases/download/{tag}/{文件名} 资产（简单字符串扫描，不引 HTML 解析器）。</summary>
    private List<GitHubAsset> ParseDownloadAssets(string html, string tag)
    {
        var assets = new List<GitHubAsset>();
        const string marker = "/releases/download/";
        int index = 0;
        while ((index = html.IndexOf("href=\"", index, StringComparison.Ordinal)) >= 0)
        {
            int start = index + "href=\"".Length;
            int end = html.IndexOf('"', start);
            if (end < 0)
            {
                break;
            }

            index = end + 1;
            string href = html[start..end];
            int pathStart = href.IndexOf(marker, StringComparison.Ordinal);
            if (pathStart < 0)
            {
                continue;
            }

            string remainder = href[(pathStart + marker.Length)..]; // {tag}/{文件名}
            int separator = remainder.IndexOf('/');
            if (separator <= 0)
            {
                continue;
            }

            string hrefTag = Uri.UnescapeDataString(remainder[..separator]);
            string name = Uri.UnescapeDataString(remainder[(separator + 1)..]);
            if (name.Length == 0 || !string.Equals(hrefTag, tag, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // 同一资产在页面里可能以根相对与绝对两种形式各出现一次
            if (assets.Any(a => string.Equals(a.Name, name, StringComparison.Ordinal)))
            {
                continue;
            }

            bool absolute = href.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || href.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
            assets.Add(new GitHubAsset(name, absolute ? href : _webBase + href));
        }

        return assets;
    }

    /// <summary>Windows x64 安装器优先，其次 zip；没有匹配包时交给用户打开发布页。</summary>
    private static (string? Url, string? Name, string? ChecksumUrl) PickAsset(GitHubAsset[]? assets)
    {
        if (assets is null || assets.Length == 0)
        {
            return (null, null, null);
        }

        GitHubAsset? preferred = assets.FirstOrDefault(a =>
                a.Name.Contains("win-x64", StringComparison.OrdinalIgnoreCase) &&
                a.Name.EndsWith("-setup.exe", StringComparison.OrdinalIgnoreCase))
            ?? assets.FirstOrDefault(a =>
                a.Name.Contains("win-x64", StringComparison.OrdinalIgnoreCase) &&
                a.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase));
        if (preferred is null)
        {
            return (null, null, null);
        }

        GitHubAsset? checksum = assets.FirstOrDefault(a =>
            string.Equals(a.Name, preferred.Name + ".sha256", StringComparison.OrdinalIgnoreCase));
        return (preferred.Url, preferred.Name, checksum?.Url);
    }
}

public sealed record GitHubAsset(string Name, string Url);

/// <summary>版本号比较。tag 形如 v1.2.3 / 1.2 / 1.2.3.4 都能吃；两边补齐四段再比，避免三段 Version 被误判。</summary>
public static class UpdateVersion
{
    public static Version? Parse(string? tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            return null;
        }

        string text = tag.Trim();
        if (text.Length > 0 && (text[0] == 'v' || text[0] == 'V'))
        {
            text = text[1..];
        }

        // 去掉 1.2.3-beta.1 这类后缀
        int dash = text.IndexOfAny(new[] { '-', '+' });
        if (dash > 0)
        {
            text = text[..dash];
        }

        return Version.TryParse(text, out Version? version) ? Normalize(version) : null;
    }

    public static bool IsNewer(Version? candidate, Version? current)
    {
        if (candidate is null)
        {
            return false;
        }

        return current is null || candidate > Normalize(current);
    }

    private static Version Normalize(Version version)
        => new(
            Math.Max(version.Major, 0),
            Math.Max(version.Minor, 0),
            Math.Max(version.Build, 0),
            Math.Max(version.Revision, 0));
}
