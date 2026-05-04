using CodeWF.Modules.ToolFramework.Models;

namespace CodeWF.Modules.ToolFramework.Services;

public sealed class ToolRegistry
{
    private readonly Dictionary<string, Func<ToolSpec>> _factories = new(StringComparer.OrdinalIgnoreCase);

    public ToolRegistry()
    {
        foreach (var factory in ToolCatalog.CreateFactories())
        {
            var probe = factory();
            _factories[probe.Id] = factory;
        }
    }

    public IReadOnlyList<ToolSpec> GetMenuTools()
    {
        return _factories.Values
            .Select(factory => factory())
            .OrderBy(tool => Array.IndexOf(ToolCatalog.CategoryOrder, tool.Category))
            .ThenBy(tool => tool.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    public ToolSpec? Create(string id)
    {
        return _factories.TryGetValue(id, out var factory) ? factory() : null;
    }
}

