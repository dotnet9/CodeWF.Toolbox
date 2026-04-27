using System;

namespace CodeWF.Toolbox.Models;

/// <summary>
/// 应用内对外链接统一放在这里，避免 XAML 中散落硬编码地址。
/// </summary>
public static class AppLinks
{
    public static readonly Uri RepositoryUrl = new("https://github.com/dotnet9/CodeWF.Toolbox");
    public static readonly Uri OnlineToolUrl = new("https://dotnet9.com/tool");
    public static readonly Uri DeveloperGuideUrl = new("https://github.com/dotnet9/CodeWF.Toolbox/tree/develop/docs");
}
