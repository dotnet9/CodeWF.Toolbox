namespace Tools.CodeWF.Extensions;

public static class StringExtensions
{
	/// <summary>
	/// Removes one trailing occurrence of the specified string
	/// </summary>
	public static string TrimEnd(this string me, string trimString, StringComparison comparisonType)
	{
		if (me.EndsWith(trimString, comparisonType))
		{
			return me[..^trimString.Length];
		}

		return me;
	}
}
