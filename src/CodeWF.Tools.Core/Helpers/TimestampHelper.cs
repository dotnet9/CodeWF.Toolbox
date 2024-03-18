namespace CodeWF.Tools.Core.Helpers;

public class TimestampHelper
{
    public static long GetCurrentTimestamp(TimestampType type = TimestampType.Second)
    {
        return GetTimestamp(DateTimeOffset.UtcNow, type);
    }

    public static long GetTimestamp(DateTimeOffset date, TimestampType type = TimestampType.Second)
    {
        return type == TimestampType.Second
            ? date.ToUnixTimeSeconds()
            : date.ToUnixTimeMilliseconds();
    }

    public static DateTimeOffset GetTime(long timestamp, TimestampType type = TimestampType.Second)
    {
        return type == TimestampType.Second
            ? DateTimeOffset.FromUnixTimeSeconds(timestamp)
            : DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
    }
}

public enum TimestampType
{
    [Description("秒(s)")] Second,
    [Description("毫秒(ms)")] Milliseconds
}