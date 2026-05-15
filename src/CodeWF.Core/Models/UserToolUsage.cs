namespace CodeWF.Core.Models;

public sealed class UserToolUsage
{
    public string ViewName { get; set; } = string.Empty;

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Icon { get; set; }

    public int Count { get; set; }

    public DateTimeOffset LastUsedAt { get; set; } = DateTimeOffset.UtcNow;
}
