using System.ComponentModel;

namespace CodeWF.Tools.Module.Developer.Models;

public enum TimestampType
{
    [Description("秒(s)")] Second,
    [Description("毫秒(ms)")] Milliseconds
}