namespace CodeWF.Utils;

public static class EnumerableExtension
{
    public static string JoinAsString<T>(this IEnumerable<T> source, string separator)
    {
        return string.Join(separator, source);
    }

    public static IEnumerable<T> Random<T>(this IEnumerable<T> listT, int count)
    {
        return listT.OrderBy(l => Guid.NewGuid()).Take(count);
    }
}