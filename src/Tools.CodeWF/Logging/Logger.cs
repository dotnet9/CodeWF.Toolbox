using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Tools.CodeWF.Helpers;

namespace Tools.CodeWF.Logging;

/// <summary>
/// Logging class.
///
/// <list type="bullet">
/// <item>Logger is enabled by default but no <see cref="Modes"/> are set by default, so the logger does not log by default.</item>
/// <item>Only <see cref="LogLevel.Critical"/> messages are logged unless set otherwise.</item>
/// <item>The logger is thread-safe.</item>
/// </list>
/// </summary>
public static class Logger
{
	#region PropertiesAndMembers

	private static readonly object Lock = new();
	private static long On = 1;
	private static int LoggingFailedCount = 0;
	private static LogLevel MinimumLevel { get; set; } = LogLevel.Critical;
	private static HashSet<LogMode> Modes { get; } = new();
	public static string FilePath { get; private set; } = "Log.txt";
	public static string EntrySeparator { get; } = Environment.NewLine;

	/// <summary>Gets or sets the maximum log file size in bytes.</summary>
	/// <remarks>
	/// Default value is approximately 10 MB. If set to <c>0</c>, then there is no maximum log file size.
	/// <para>Guarded by <see cref="Lock"/>.</para>
	/// </remarks>
	private static long MaximumLogFileSizeBytes { get; set; } = 10_000_000;

	/// <summary>Gets or sets current log file size in bytes.</summary>
	/// <remarks>Guarded by <see cref="Lock"/>.</remarks>
	private static long LogFileSizeBytes { get; set; }

	#endregion PropertiesAndMembers

	#region Initializers

	/// <summary>
	/// Initializes the logger with default values.
	/// <para>
	/// Default values are set as follows:
	/// <list type="bullet">
	/// <item>For RELEASE mode: <see cref="MinimumLevel"/> is set to <see cref="LogLevel.Info"/>, and logs only to file.</item>
	/// <item>For DEBUG mode: <see cref="MinimumLevel"/> is set to <see cref="LogLevel.Debug"/>, and logs to file, debug and console.</item>
	/// </list>
	/// </para>
	/// </summary>
	/// <param name="logLevel">Use <c>null</c> to use default <see cref="LogLevel"/> or a custom value to force non-default <see cref="LogLevel"/>.</param>
	public static void InitializeDefaults(string filePath, LogLevel? logLevel = null)
	{
		SetFilePath(filePath);

#if RELEASE
		SetMinimumLevel(logLevel ??= LogLevel.Info);
		SetModes(LogMode.Console, LogMode.File);

#else
		SetMinimumLevel(logLevel ??= LogLevel.Debug);
		SetModes(LogMode.Debug, LogMode.Console, LogMode.File);
#endif

		lock (Lock)
		{
			if (MinimumLevel == LogLevel.Trace)
			{
				MaximumLogFileSizeBytes = 0;
			}
		}
	}

	public static void SetMinimumLevel(LogLevel level) => MinimumLevel = level;

	public static void SetModes(params LogMode[] modes)
	{
		if (Modes.Count != 0)
		{
			Modes.Clear();
		}

		if (modes is null)
		{
			return;
		}

		foreach (var mode in modes)
		{
			Modes.Add(mode);
		}
	}

	public static void SetFilePath(string filePath)
	{
		FilePath = Guard.NotNullOrEmptyOrWhitespace(nameof(filePath), filePath, trim: true);
		IoHelpers.EnsureContainingDirectoryExists(FilePath);

		if (File.Exists(FilePath))
		{
			lock (Lock)
			{
				LogFileSizeBytes = new FileInfo(FilePath).Length;
			}
		}
	}

	#endregion Initializers

	#region Methods

	public static void TurnOff() => Interlocked.Exchange(ref On, 0);

	public static void TurnOn() => Interlocked.Exchange(ref On, 1);

	public static bool IsOn() => Interlocked.Read(ref On) == 1;

	#endregion Methods

	#region GeneralLoggingMethods

	public static void Log(LogLevel level, string message, int additionalEntrySeparators = 0,
		bool additionalEntrySeparatorsLogFileOnlyMode = true, [CallerFilePath] string callerFilePath = "",
		[CallerMemberName] string callerMemberName = "", [CallerLineNumber] int callerLineNumber = -1)
	{
		try
		{
			if (Modes.Count == 0 || !IsOn())
			{
				return;
			}

			if (level < MinimumLevel)
			{
				return;
			}

			message = Guard.Correct(message);
			var category = string.IsNullOrWhiteSpace(callerFilePath)
				? ""
				: $"{EnvironmentHelpers.ExtractFileName(callerFilePath)}.{callerMemberName} ({callerLineNumber})";

			var messageBuilder = new StringBuilder();
			messageBuilder.Append(
				$"{DateTime.UtcNow.ToLocalTime():yyyy-MM-dd HH:mm:ss.fff} [{Environment.CurrentManagedThreadId}] {level.ToString().ToUpperInvariant()}\t");

			if (message.Length == 0)
			{
				if (category.Length == 0) // If both empty. It probably never happens though.
				{
					messageBuilder.Append($"{EntrySeparator}");
				}
				else // If only the message is empty.
				{
					messageBuilder.Append($"{category}{EntrySeparator}");
				}
			}
			else
			{
				if (category.Length == 0) // If only the category is empty.
				{
					messageBuilder.Append($"{message}{EntrySeparator}");
				}
				else // If none of them empty.
				{
					messageBuilder.Append($"{category}\t{message}{EntrySeparator}");
				}
			}

			var finalMessage = messageBuilder.ToString();

			for (int i = 0; i < additionalEntrySeparators; i++)
			{
				messageBuilder.Insert(0, EntrySeparator);
			}

			var finalFileMessage = messageBuilder.ToString();
			if (!additionalEntrySeparatorsLogFileOnlyMode)
			{
				finalMessage = finalFileMessage;
			}

			lock (Lock)
			{
				if (Modes.Contains(LogMode.Console))
				{
					lock (Console.Out)
					{
						var color = Console.ForegroundColor;
						switch (level)
						{
							case LogLevel.Warning:
								color = ConsoleColor.Yellow;
								break;

							case LogLevel.Error:
							case LogLevel.Critical:
								color = ConsoleColor.Red;
								break;

							default:
								break; // Keep original color.
						}

						Console.ForegroundColor = color;
						Console.Write(finalMessage);
						Console.ResetColor();
					}
				}

				if (Modes.Contains(LogMode.Debug))
				{
					Debug.Write(finalMessage);
				}

				if (!Modes.Contains(LogMode.File))
				{
					return;
				}

				if (MaximumLogFileSizeBytes > 0)
				{
					// Simplification here is that: 1 character ~ 1 byte.
					LogFileSizeBytes += finalFileMessage.Length;

					if (LogFileSizeBytes > MaximumLogFileSizeBytes)
					{
						LogFileSizeBytes = 0;
						File.Delete(FilePath);
					}
				}

				File.AppendAllText(FilePath, finalFileMessage);
			}
		}
		catch (Exception ex)
		{
			if (Interlocked.Increment(ref LoggingFailedCount) ==
			    1) // If it only failed the first time, try log the failure.
			{
				LogDebug($"Logging failed: {ex}");
			}

			// If logging the failure is successful then clear the failure counter.
			// If it's not the first time the logging failed, then we do not try to log logging failure, so clear the failure counter.
			Interlocked.Exchange(ref LoggingFailedCount, 0);
		}
	}

	#endregion GeneralLoggingMethods

	#region DebugLoggingMethods

	/// <summary>
	/// Logs a string message at <see cref="LogLevel.Debug"/> level.
	///
	/// <para>For information that has short-term usefulness during development and debugging.</para>
	/// </summary>
	/// <remarks>You typically would not enable <see cref="LogLevel.Debug"/> level in production unless you are troubleshooting, due to the high volume of generated logs.</remarks>
	/// <example>For example: <c>Entering method Configure with flag set to true.</c></example>
	public static void LogDebug(string message, [CallerFilePath] string callerFilePath = "",
		[CallerMemberName] string callerMemberName = "", [CallerLineNumber] int callerLineNumber = -1) => Log(
		LogLevel.Debug, message, callerFilePath: callerFilePath, callerMemberName: callerMemberName,
		callerLineNumber: callerLineNumber);

	#endregion DebugLoggingMethods

	#region InfoLoggingMethods

	/// <summary>
	/// Logs a string message at <see cref="LogLevel.Info"/> level.
	///
	/// <para>For tracking the general flow of the application.</para>
	/// <remarks>These logs typically have some long-term value.</remarks>
	/// <example>"Request received for path /api/my-controller"</example>
	/// </summary>
	public static void LogInfo(string message, [CallerFilePath] string callerFilePath = "",
		[CallerMemberName] string callerMemberName = "", [CallerLineNumber] int callerLineNumber = -1) => Log(
		LogLevel.Info, message, callerFilePath: callerFilePath, callerMemberName: callerMemberName,
		callerLineNumber: callerLineNumber);

	#endregion InfoLoggingMethods
}
