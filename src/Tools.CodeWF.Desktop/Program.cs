using Avalonia;
using Avalonia.ReactiveUI;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Concurrency;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using ReactiveUI;
using Tools.CodeWF.Daemon;
using Tools.CodeWF.Fluent;
using Tools.CodeWF.Fluent.CrashReport;
using Tools.CodeWF.Fluent.Helpers;
using Tools.CodeWF.Logging;

namespace Tools.CodeWF.Desktop;

internal sealed class Program
{
	// Initialization code. Don't use any Avalonia, third-party APIs or any
	// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
	// yet and stuff might break.
	[STAThread]
	public static async Task<int> Main(string[] args)
	{
		// Crash reporting must be before the "single instance checking".
		Logger.InitializeDefaults(Path.Combine(Config.DataDir, "Logs.txt"), LogLevel.Info);
		try
		{
		}
		catch (Exception ex)
		{
			// If anything happens here just log it and exit.
			Logger.LogCritical(ex);
			return 1;
		}

		try
		{
			var app = CodeWFAppBuilder.Create("码界工坊工具箱", args)
				.EnsureSingleInstance()
				.OnUnhandledExceptions(LogUnhandledException)
				.OnUnobservedTaskExceptions(LogUnobservedTaskException)
				.OnTermination(TerminateApplication)
				.Build();

			var exitCode=await app.
			BuildAvaloniaApp()
				.StartWithClassicDesktopLifetime(args);
			await Task.CompletedTask;
		}
		catch (Exception ex)
		{
			CrashReporter.Invoke(ex);
			Logger.LogCritical(ex);
			return 1;
		}
	}

	// Avalonia configuration, don't remove; also used by visual designer.
	public static AppBuilder BuildAvaloniaApp()
		=> AppBuilder.Configure<App>()
			.UsePlatformDetect()
			.WithInterFont()
			.LogToTrace()
			.UseReactiveUI();

	/// <summary>
	/// Do not call this method it should only be called by TerminateService.
	/// </summary>
	private static void TerminateApplication()
	{
		Dispatcher.UIThread.Post(() =>
			(Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow?.Close());
	}

	private static void LogUnobservedTaskException(object? sender, AggregateException e)
	{
		ReadOnlyCollection<Exception> innerExceptions = e.Flatten().InnerExceptions;

		switch (innerExceptions)
		{
			case [SocketException { SocketErrorCode: SocketError.OperationAborted }]:
			// Source of this exception is NBitcoin library.
			case [OperationCanceledException { Message: "The peer has been disconnected" }]:
				// Until https://github.com/MetacoSA/NBitcoin/pull/1089 is resolved.
				Logger.LogTrace(e);
				break;

			default:
				Logger.LogDebug(e);
				break;
		}
	}

	private static void LogUnhandledException(object? sender, Exception e) =>
		Logger.LogWarning(e);
}

public static class CodeWFAppExtensions
{
	public static async Task<ExitCode> RunAsGuiAsync(this CodeWFApplication app)
	{
		return await app.RunAsync(
			afterStarting: () =>
			{
				RxApp.DefaultExceptionHandler = Observer.Create<Exception>(ex =>
				{
					if (Debugger.IsAttached)
					{
						Debugger.Break();
					}

					Logger.LogError(ex);

					RxApp.MainThreadScheduler.Schedule(() => throw new ApplicationException("Exception has been thrown in unobserved ThrownExceptions", ex));
				});

				Logger.LogInfo("Wasabi GUI started.");
				bool runGuiInBackground = app.AppConfig.Arguments.Any(arg => arg.Contains(StartupHelper.SilentArgument));
				UiConfig uiConfig = LoadOrCreateUiConfig(Config.DataDir);

				using CancellationTokenSource stopLoadingCts = new();

				AppBuilder appBuilder = AppBuilder
					.Configure(() => new App(
						backendInitialiseAsync: async () =>
						{
							// macOS require that Avalonia is started with the UI thread. Hence this call must be delayed to this point.
							await app.Global!.InitializeNoWalletAsync(initializeSleepInhibitor: true, app.TerminateService, stopLoadingCts.Token).ConfigureAwait(false);

							// Make sure that wallet startup set correctly regarding RunOnSystemStartup
							await StartupHelper.ModifyStartupSettingAsync(uiConfig.RunOnSystemStartup).ConfigureAwait(false);
						}, startInBg: runGuiInBackground))
					.UseReactiveUI()
					.SetupAppBuilder()
					.AfterSetup(_ => ThemeHelper.ApplyTheme(uiConfig.DarkModeEnabled ? Theme.Dark : Theme.Light));

				if (app.TerminateService.CancellationToken.IsCancellationRequested)
				{
					Logger.LogDebug("Skip starting Avalonia UI as requested the application to stop.");
					stopLoadingCts.Cancel();
				}
				else
				{
					appBuilder.StartWithClassicDesktopLifetime(app.AppConfig.Arguments);
				}

				return Task.CompletedTask;
			});
	}

	private static UiConfig LoadOrCreateUiConfig(string dataDir)
	{
		Directory.CreateDirectory(dataDir);

		UiConfig uiConfig = new(Path.Combine(dataDir, "UiConfig.json"));
		uiConfig.LoadFile(createIfMissing: true);

		return uiConfig;
	}
}
