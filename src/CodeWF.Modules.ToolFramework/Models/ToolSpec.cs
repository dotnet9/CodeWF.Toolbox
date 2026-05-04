namespace CodeWF.Modules.ToolFramework.Models;

public sealed class ToolSpec
{
    public required string Id { get; init; }

    public required string Category { get; init; }

    public required string Name { get; init; }

    public required string Description { get; init; }

    public string Icon { get; init; } = Icons.Tool;

    public bool AutoRun { get; init; } = true;

    public List<ToolField> Fields { get; } = [];

    public List<ToolOutput> Outputs { get; } = [];

    public Func<ToolRunContext, CancellationToken, Task>? RunAsync { get; init; }
}

