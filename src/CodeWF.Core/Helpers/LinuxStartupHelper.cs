namespace CodeWF.Core.Helpers;

public static class LinuxStartupHelper
{
    public static readonly string FilePath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "autostart",
            Constants.RegisterKey);

    public static async Task AddOrRemoveDesktopFileAsync(bool runOnSystemStartup)
    {
        string pathToDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config",
            "autostart");
        string pathToDesktopFile = Path.Combine(pathToDir, Constants.RegisterKey);

        IoHelpers.EnsureContainingDirectoryExists(pathToDesktopFile);

        if (runOnSystemStartup)
        {
            string pathToExec = EnvironmentHelpers.GetExecutablePath();

            string pathToExecWithArgs = $"{pathToExec} {StartupHelper.SilentArgument}";

            IoHelpers.EnsureFileExists(pathToExec);

            string fileContents = string.Join(
                "\n",
                "[Desktop Entry]",
                $"Name={Constants.AppName}",
                "Type=Application",
                $"Exec={pathToExecWithArgs}",
                "Hidden=false",
                "Terminal=false",
                "X-GNOME-Autostart-enabled=true");

            await File.WriteAllTextAsync(pathToDesktopFile, fileContents).ConfigureAwait(false);
        }
        else
        {
            File.Delete(pathToDesktopFile);
        }
    }

    public static readonly string ExpectedDesktopFileContent = string.Join(
        "\n",
        "[Desktop Entry]",
        $"Name={Constants.AppName}",
        "Type=Application",
        $"Exec={EnvironmentHelpers.GetExecutablePath()} {StartupHelper.SilentArgument}",
        "Hidden=false",
        "Terminal=false",
        "X-GNOME-Autostart-enabled=true");

    public static string GetFileContent()
    {
        return string.Join("\n", File.ReadAllLines(FilePath));
    }
}