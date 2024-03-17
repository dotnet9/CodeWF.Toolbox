using DynamicData;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Reactive.Concurrency;
using Tools.CodeWF.Helpers;
using Tools.CodeWF.Services.Terminate;

namespace Tools.CodeWF.Daemon;

public class Global
{
	public Global(string dataDir, string configFilePath, Config config)
	{
		DataDir = dataDir;
		ConfigFilePath = configFilePath;
		Config = config;


		Cache = new MemoryCache(new MemoryCacheOptions
		{
			SizeLimit = 1_000,
			ExpirationScanFrequency = TimeSpan.FromSeconds(30)
		});
	}

	public const string ThemeBackgroundBrushResourceKey = "ThemeBackgroundBrush";
	public const string ApplicationAccentForegroundBrushResourceKey = "ApplicationAccentForegroundBrush";

	/// <summary>Lock that makes sure the application initialization and dispose methods do not run concurrently.</summary>
	private AsyncLock InitializationAsyncLock { get; } = new();

	/// <summary>Cancellation token to cancel <see cref="InitializeNoWalletAsync(TerminateService)"/> processing.</summary>
	private CancellationTokenSource StoppingCts { get; } = new();

	public string DataDir { get; }
	public string ConfigFilePath { get; }
	public Config Config { get; }

	public IMemoryCache Cache { get; private set; }

	public Uri? OnionServiceUri { get; private set; }
}
