using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Tools.CodeWF.Helpers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Tools.CodeWF.Daemon;

public class Config
{
	public static readonly IDictionary EnvironmentVariables = Environment.GetEnvironmentVariables();

	public static string DataDir { get; } = GetStringValue(
		"datadir",
		EnvironmentHelpers.GetDataDir(Path.Combine("Tools.CodeWF", "Client")),
		Environment.GetCommandLineArgs()).EffectiveValue;

	private static StringValue GetStringValue(string key, string value, string[] cliArgs)
	{
		if (GetOverrideValue(key, cliArgs, out string? overrideValue, out ValueSource? valueSource))
		{
			return new StringValue(value, overrideValue, valueSource.Value);
		}

		return new StringValue(value, value, ValueSource.Disk);
	}

	private static bool GetOverrideValue(string key, string[] cliArgs, [NotNullWhen(true)] out string? overrideValue,
		[NotNullWhen(true)] out ValueSource? valueSource)
	{
		// CLI arguments have higher precedence than environment variables.
		if (GetCliArgsValue(key, cliArgs, out string? argsValue))
		{
			valueSource = ValueSource.CommandLineArgument;
			overrideValue = argsValue;
			return true;
		}

		if (GetEnvironmentVariable(key, out string? envVarValue))
		{
			valueSource = ValueSource.EnvironmentVariable;
			overrideValue = envVarValue;
			return true;
		}

		valueSource = null;
		overrideValue = null;
		return false;
	}

	private static bool GetCliArgsValue(string key, string[] cliArgs, [NotNullWhen(true)] out string? cliArgsValue)
	{
		if (ArgumentHelpers.TryGetValue(key, cliArgs, out cliArgsValue))
		{
			return true;
		}

		cliArgsValue = null;
		return false;
	}

	private static bool GetEnvironmentVariable(string key, [NotNullWhen(true)] out string? envValue)
	{
		string envKey = $"CODEWF-{key.ToUpperInvariant()}";

		if (EnvironmentVariables.Contains(envKey))
		{
			if (EnvironmentVariables[envKey] is string envVar)
			{
				envValue = envVar;
				return true;
			}
		}

		envValue = null;
		return false;
	}

	/// <summary>Source of application config value.</summary>
	private enum ValueSource
	{
		/// <summary>Value stored in JSON config on disk.</summary>
		Disk,

		/// <summary>CLI argument passed by user to override disk config value.</summary>
		CommandLineArgument,

		/// <summary>Environment variable overriding disk config value.</summary>
		EnvironmentVariable
	}

	private interface IValue
	{
		ValueSource ValueSource { get; }
		bool Overridden => ValueSource != ValueSource.Disk;
	}

	private interface ITypedValue<T> : IValue
	{
		T Value { get; }
		T EffectiveValue { get; }
	}

	private record StringValue(string Value, string EffectiveValue, ValueSource ValueSource) : ITypedValue<string>;
}
