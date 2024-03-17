using Tools.CodeWF.Helpers;

namespace Tools.CodeWF.Fluent.Helpers;

public static class LinuxStartupHelper
{
	public static async Task AddOrRemoveDesktopFileAsync(bool runOnSystemStartup)
	{
		string pathToDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config",
			"autostart");
		string pathToDesktopFile = Path.Combine(pathToDir, "Tools.CodeWF.desktop");

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
}
