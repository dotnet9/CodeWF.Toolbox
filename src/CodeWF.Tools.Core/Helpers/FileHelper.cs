namespace CodeWF.Tools.Core.Helpers;

public static class FileHelper
{
    static FileHelper()
    {
        Root = Path.Combine(Path.GetTempPath(), AppInfo.AppInfo.ToolName, "Cache");
        if (!Directory.Exists(Root))
        {
            Directory.CreateDirectory(Root);
        }
    }

    public static string Root { get; }

    public static string GetTempFileName()
    {
        string dir = Root;
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        string fn = Path.Combine(dir, Path.GetRandomFileName());
        XTrace.WriteLine("分配临时文件：{0}", fn);
        return fn;
    }

    public static void DeleteFileIfExist(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}