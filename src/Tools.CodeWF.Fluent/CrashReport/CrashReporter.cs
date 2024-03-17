using System.Diagnostics;
using Tools.CodeWF.Extensions;
using Tools.CodeWF.Logging;
using Tools.CodeWF.Microservices;
using Tools.CodeWF.Models;

namespace Tools.CodeWF.Fluent.CrashReport;

public static class CrashReporter
{
	public static void Invoke(Exception exceptionToReport)
	{
		try
		{
			var serializedException = exceptionToReport.ToSerializableException();
			var base64ExceptionString = SerializableException.ToBase64String(serializedException);
			var args = $"crashreport -exception=\"{base64ExceptionString}\"";

			var path = Process.GetCurrentProcess().MainModule?.FileName;
			if (string.IsNullOrEmpty(path))
			{
				throw new InvalidOperationException($"Invalid path: '{path}'");
			}

			ProcessStartInfo startInfo = ProcessStartInfoFactory.Make(
				processPath: path,
				arguments: args,
				openConsole: false);

			using Process? p = Process.Start(startInfo);
		}
		catch (Exception ex)
		{
			Logger.LogWarning($"There was a problem while invoking crash report: '{ex}'.");
		}
	}
}
