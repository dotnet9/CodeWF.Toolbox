using System;
using System.IO;

namespace CodeWF.Toolbox.Services;

/// <summary>默认使用原有用户目录；显式指定时可在独立目录运行验证。</summary>
public static class ToolboxDataPaths
{
    public static string Resource(string name)
    {
        var adjacent = Path.Combine(AppContext.BaseDirectory, name);
        if (Directory.Exists(adjacent) || File.Exists(adjacent)) return adjacent;
        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "Resources", name));
    }

    public static string Root => Environment.GetEnvironmentVariable("TOOLBOX_DATA_DIRECTORY") is { Length: > 0 } isolated
        ? Path.GetFullPath(isolated)
        : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CodeWF.Toolbox");
}
