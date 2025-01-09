using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

namespace CodeWF.Core.Helpers;

public static class IoHelpers
{
    public static void EnsureContainingDirectoryExists(string fileNameOrPath)
    {
        string fullPath = Path.GetFullPath(fileNameOrPath); // No matter if relative or absolute path is given to this.
        string? dir = Path.GetDirectoryName(fullPath);
        EnsureDirectoryExists(dir);
    }

    /// <summary>
    /// Makes sure that directory <paramref name="dir"/> is created if it does not exist.
    /// </summary>
    /// <remarks>Method does not throw exceptions unless provided directory path is invalid.</remarks>
    public static void EnsureDirectoryExists(string? dir)
    {
        // If root is given, then do not worry.
        if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }

    public static void EnsureFileExists(string filePath)
    {
        if (!File.Exists(filePath))
        {
            EnsureContainingDirectoryExists(filePath);

            File.Create(filePath)?.Dispose();
        }
    }

    public static void OpenFolderInFileExplorer(string dirPath)
    {
        if (Directory.Exists(dirPath))
        {
            // RuntimeInformation.OSDescription on WSL2 reports a string like:
            // 'Linux 5.10.102.1-microsoft-standard-WSL2 #1 SMP Wed Mar 2 00:30:59 UTC 2022'
            if (!RuntimeInformation.OSDescription.ToString(CultureInfo.InvariantCulture).Contains("WSL2"))
            {
                using var process = Process.Start(new ProcessStartInfo
                {
                    FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                        ? "explorer.exe"
                        : (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                            ? "open"
                            : "xdg-open"),
                    Arguments = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? $"\"{dirPath}\"" : dirPath,
                    CreateNoWindow = true
                });
            }
        }
    }
}