using ReactiveUI;
using System.Reactive;

namespace CodeWF.Modules.ToolFramework.Models;

public sealed class ToolActionItem
{
    public required string Label { get; init; }

    public bool IsPrimary { get; init; }

    public required ReactiveCommand<Unit, Unit> Command { get; init; }
}

