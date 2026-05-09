using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace CodeWF.Toolbox.Diagnostics;

public static class StartupDiagnostics
{
    private const string LogFileName = "codewf-startup.log";
    private static readonly object SyncRoot = new();

    public static void LogStartupEnvironment()
    {
        Log(
            $"Process start. OS={RuntimeInformation.OSDescription}; " +
            $"Framework={RuntimeInformation.FrameworkDescription}; " +
            $"Architecture={RuntimeInformation.ProcessArchitecture}; " +
            $"BaseDirectory={AppContext.BaseDirectory}");
    }

    public static void Log(string message)
    {
        Write(message);
    }

    public static void LogException(string stage, Exception exception)
    {
        Write($"{stage}{Environment.NewLine}{exception}");
    }

    private static void Write(string message)
    {
        var line = $"[{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff zzz}] {message}{Environment.NewLine}";

        foreach (var path in GetCandidateLogPaths())
        {
            try
            {
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                lock (SyncRoot)
                {
                    File.AppendAllText(path, line, Encoding.UTF8);
                }

                return;
            }
            catch
            {
                // Logging must never become the startup failure.
            }
        }
    }

    private static string[] GetCandidateLogPaths()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return
        [
            Path.Combine(AppContext.BaseDirectory, LogFileName),
            string.IsNullOrWhiteSpace(localAppData)
                ? LogFileName
                : Path.Combine(localAppData, "CodeWF.Toolbox", LogFileName)
        ];
    }
}
