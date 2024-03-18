namespace CodeWF.Tools.Core.Models;

/// <summary>
///     IP查询信息
/// </summary>
/// <param name="QueryFrom">查询源</param>
/// <param name="IP">查询IP</param>
/// <param name="Result">查询结果</param>
public record IPQueryInfo(string QueryFrom, string? IP, string? Result);