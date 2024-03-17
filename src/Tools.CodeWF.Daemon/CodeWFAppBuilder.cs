using Tools.CodeWF.Helpers;
using Tools.CodeWF.Logging;
using Tools.CodeWF.Services.Terminate;

namespace Tools.CodeWF.Daemon;

public enum ExitCode
{
	Ok,
	FailedAlreadyRunningSignaled,
	FailedAlreadyRunningError,
}

public class CodeWFApplication
{
	public CodeWFAppBuilder AppConfig { get; }
	public Global? Global { get; private set; }
	public string ConfigFilePath { get; }
	public Config Config { get; }
	public TerminateService TerminateService { get; }

	public CodeWFApplication(CodeWFAppBuilder wasabiAppBuilder)
	{
		AppConfig = wasabiAppBuilder;

		ConfigFilePath = Path.Combine(Config.DataDir, "Config.json");
		Directory.CreateDirectory(Config.DataDir);
		Config = new Config();

		SetupLogger();
		Logger.LogDebug(
			$"Wasabi was started with these argument(s): {string.Join(" ", AppConfig.Arguments.DefaultIfEmpty("none"))}.");
		TerminateService = new(TerminateApplicationAsync, AppConfig.Terminate);
	}

	public async Task<ExitCode> RunAsync(Func<Task> afterStarting)
	{
		if (AppConfig.Arguments.Contains("--version"))
		{
			Console.WriteLine($"{AppConfig.AppName} {Constants.ClientVersion}");
			return ExitCode.Ok;
		}

		if (AppConfig.Arguments.Contains("--help"))
		{
			ShowHelp();
			return ExitCode.Ok;
		}

		try
		{
			TerminateService.Activate();

			BeforeStarting();

			await afterStarting();
			return ExitCode.Ok;
		}
		finally
		{
			BeforeStopping();
		}
	}

	private void BeforeStarting()
	{
		AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
		TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

		Logger.LogSoftwareStarted(AppConfig.AppName);

		Global = CreateGlobal();
	}

	private void BeforeStopping()
	{
		AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
		TaskScheduler.UnobservedTaskException -= TaskScheduler_UnobservedTaskException;

		// Start termination/disposal of the application.
		TerminateService.Terminate();
		Logger.LogSoftwareStopped(AppConfig.AppName);
	}

	private Global CreateGlobal()
		=> new();

	private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
	{
		AppConfig.UnobservedTaskExceptionsEventHandler?.Invoke(this, e.Exception);
	}

	private void CurrentDomain_UnhandledException(object? sender, UnhandledExceptionEventArgs e)
	{
		if (e.ExceptionObject is Exception ex)
		{
			AppConfig.UnhandledExceptionEventHandler?.Invoke(this, ex);
		}
	}

	private async Task TerminateApplicationAsync()
	{
		Logger.LogSoftwareStopped(AppConfig.AppName);
		await Task.CompletedTask;
	}

	private void SetupLogger()
	{
		LogLevel logLevel = LogLevel.Info;
		Logger.InitializeDefaults(Path.Combine(Config.DataDir, "Logs.txt"), logLevel);
	}

	private void ShowHelp()
	{
		Console.WriteLine($"{AppConfig.AppName} {Constants.ClientVersion}");
		Console.WriteLine($"Usage: {AppConfig.AppName} [OPTION]...");
		Console.WriteLine();
		Console.WriteLine("Available options are:");
	}
}

public record CodeWFAppBuilder(string AppName, string[] Arguments)
{
	internal EventHandler<Exception>? UnhandledExceptionEventHandler { get; init; }
	internal EventHandler<AggregateException>? UnobservedTaskExceptionsEventHandler { get; init; }
	internal Action Terminate { get; init; } = () => { };

	public CodeWFAppBuilder OnUnhandledExceptions(EventHandler<Exception> handler) =>
		this with { UnhandledExceptionEventHandler = handler };

	public CodeWFAppBuilder OnUnobservedTaskExceptions(EventHandler<AggregateException> handler) =>
		this with { UnobservedTaskExceptionsEventHandler = handler };

	public CodeWFAppBuilder OnTermination(Action action) =>
		this with { Terminate = action };

	public CodeWFAppBuilder Build() =>
		new(this);

	public static CodeWFAppBuilder Create(string appName, string[] args) =>
		new(appName, args);
}
