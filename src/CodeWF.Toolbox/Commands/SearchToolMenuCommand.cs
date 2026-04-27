using CodeWF.EventBus;

namespace CodeWF.Toolbox.Commands;

public sealed class SearchToolMenuCommand : Command
{
    public SearchToolMenuCommand(string? keyword)
    {
        Keyword = keyword ?? string.Empty;
    }

    public string Keyword { get; }
}
