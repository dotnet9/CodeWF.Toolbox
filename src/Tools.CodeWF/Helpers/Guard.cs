using System.Diagnostics.CodeAnalysis;

namespace Tools.CodeWF.Helpers;

public static class Guard
{
	[return: NotNull]
	public static T NotNull<T>(string parameterName, [NotNull] T? value)
	{
		AssertCorrectParameterName(parameterName);
		return value ?? throw new ArgumentNullException(parameterName, "Parameter cannot be null.");
	}

	private static void AssertCorrectParameterName(string parameterName)
	{
		if (parameterName is null)
		{
			throw new ArgumentNullException(nameof(parameterName), "Parameter cannot be null.");
		}

		if (parameterName.Length == 0)
		{
			throw new ArgumentException("Parameter cannot be empty.", nameof(parameterName));
		}

		if (parameterName.Trim().Length == 0)
		{
			throw new ArgumentException("Parameter cannot be whitespace.", nameof(parameterName));
		}
	}

	public static IEnumerable<T> NotNullOrEmpty<T>(string parameterName, IEnumerable<T> value)
	{
		NotNull(parameterName, value);

		if (!value.Any())
		{
			throw new ArgumentException("Parameter cannot be empty.", parameterName);
		}

		return value;
	}

	public static string NotNullOrEmptyOrWhitespace(string parameterName, string value, bool trim = false)
	{
		NotNullOrEmpty(parameterName, value);

		string trimmedValue = value.Trim();
		if (trimmedValue.Length == 0)
		{
			throw new ArgumentException("Parameter cannot be whitespace.", parameterName);
		}

		if (trim)
		{
			return trimmedValue;
		}
		else
		{
			return value;
		}
	}

	/// <summary>
	/// Corrects the string:
	/// If the string is null, it'll be empty.
	/// Trims the string.
	/// </summary>
	[return: NotNull]
	public static string Correct(string? str)
	{
		return string.IsNullOrWhiteSpace(str)
			? ""
			: str.Trim();
	}
}
