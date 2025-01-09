using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace CodeWF.Core.Helpers;

public static class WindowsStartupHelper
{
    private const string KeyPath = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

    public static bool RegistryKeyExists()
    {
        bool result = false;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            RegistryKey? registryKey = Registry.CurrentUser.OpenSubKey(KeyPath, false) ??
                                       throw new InvalidOperationException("Registry operation failed.");
            result = registryKey.GetValueNames().Contains(Constants.RegisterKey);
        }

        return result;
    }

    public static void AddOrRemoveRegistryKey(bool runOnSystemStartup)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new InvalidOperationException("Registry modification can only be done on Windows.");
        }

        string pathToExeFile = EnvironmentHelpers.GetExecutablePath();

        string pathToExecWithArgs = $"{pathToExeFile} {StartupHelper.SilentArgument}";

        if (!File.Exists(pathToExeFile))
        {
            throw new InvalidOperationException($"Path: {pathToExeFile} does not exist.");
        }

        using RegistryKey key = Registry.CurrentUser.OpenSubKey(KeyPath, writable: true) ??
                                throw new InvalidOperationException("Registry operation failed.");

        var existingPath = key.GetValue(Constants.RegisterKey);
        if (existingPath is null && runOnSystemStartup)
        {
            key.SetValue(Constants.RegisterKey, pathToExecWithArgs);
        }
        else if (existingPath is not null && !runOnSystemStartup)
        {
            key.DeleteValue(Constants.RegisterKey, false);
        }
    }
}