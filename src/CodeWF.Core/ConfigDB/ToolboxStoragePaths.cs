namespace CodeWF.Core.ConfigDB;

/// <summary>应用数据统一保存到用户目录，验证时可显式指定独立目录。</summary>
public static class ToolboxStoragePaths
{
    public static string Root => Environment.GetEnvironmentVariable("TOOLBOX_DATA_DIRECTORY") is { Length: > 0 } isolated
        ? Path.GetFullPath(isolated)
        : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CodeWF.Toolbox");

    public static string DatabasePath => Path.Combine(Root, "CodeWF.Toolbox.db");
}
