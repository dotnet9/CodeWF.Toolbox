using CodeWF.Core.Models;
using System.Collections.ObjectModel;

namespace CodeWF.Core;

public class ToolMenuService : IToolMenuService
{
    public ObservableCollection<ToolMenuItem> MenuItems { get; } = [];
    public event Action? ToolMenuChanged;

    public void AddItem(string name, string? parentName = null, string? description = null, string? viewName = null,
        string? icon = null, ToolStatus? status = null)
    {
        ToolMenuItem? parent = null;
        if (parentName != null)
        {
            parent = MenuItems.FirstOrDefault(item => item.Name == parentName) ??
                     new ToolMenuItem { Name = parentName, Children = [] };
        }

        var toolMenuItem = new ToolMenuItem()
        {
            Name = name,
            Description = description,
            ViewName = viewName,
            Status = status,
            Icon = icon
        };
        if (parent == null)
        {
            MenuItems.Add(toolMenuItem);
        }
        else
        {
            parent.Children.Add(toolMenuItem);
        }

        ToolMenuChanged?.Invoke();
    }

    public void AddSeparator()
    {
        MenuItems.Add(new ToolMenuItem() { IsSeparator = true });
    }

    public void AddGroup(string name, string? icon = null)
    {
        MenuItems.Add(new ToolMenuItem() { Name = name, Icon = icon });
    }
}