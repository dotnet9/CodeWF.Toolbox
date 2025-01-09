using System.Reflection;
using System.Runtime.InteropServices;

namespace CodeWF.Core.Helpers;

public static class EnvironmentHelpers
{
    public static string GetFullBaseDirectory()
    {
        var fullBaseDirectory = Path.GetFullPath(AppContext.BaseDirectory);

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (!fullBaseDirectory.StartsWith('/'))
            {
                fullBaseDirectory = fullBaseDirectory.Insert(0, "/");
            }
        }

        return fullBaseDirectory;
    }

    public static string GetExecutablePath()
    {
        var fullBaseDir = GetFullBaseDirectory();
        var wassabeeFileName = Path.Combine(fullBaseDir, Constants.ExecutableName);
        wassabeeFileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? $"{wassabeeFileName}.exe"
            : $"{wassabeeFileName}";
        if (File.Exists(wassabeeFileName))
        {
            return wassabeeFileName;
        }

        var assemblyName = Assembly.GetEntryAssembly()?.GetName().Name ??
                           throw new NullReferenceException("Assembly or Assembly's Name was null.");
        var fluentExecutable = Path.Combine(fullBaseDir, assemblyName);
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? $"{fluentExecutable}.exe" : $"{fluentExecutable}";
    }
}