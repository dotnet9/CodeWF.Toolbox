using Tools.CodeWF.Daemon;
using Tools.CodeWF.Helpers;
using Tools.CodeWF.Services.Terminate;

namespace Tools.CodeWF.Fluent;

public static class Services
{
	public static string DataDir { get; private set; } = null!;


	public static UiConfig UiConfig { get; private set; } = null!;
	public static TerminateService TerminateService { get; private set; } = null!;

	public static Config Config { get; set; } = null!;

	public static bool IsInitialized { get; private set; }

	/// <summary>
	/// Initializes global services used by fluent project.
	/// </summary>
	public static void Initialize(Global global, UiConfig uiConfig, TerminateService terminateService)
	{
		Guard.NotNull(nameof(uiConfig), uiConfig);
		Guard.NotNull(nameof(terminateService), terminateService);

		DataDir = global.DataDir;
		UiConfig = uiConfig;
		Config = global.Config;
		TerminateService = terminateService;

		IsInitialized = true;
	}
}
