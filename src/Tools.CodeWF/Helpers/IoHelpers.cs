namespace Tools.CodeWF.Helpers;

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
}
