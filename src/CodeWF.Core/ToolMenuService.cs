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
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("菜单名称不能为空。", nameof(name));
        }

        var toolMenuItem = new ToolMenuItem()
        {
            Name = name,
            Description = description,
            ViewName = viewName,
            Status = status,
            Icon = icon
        };

        if (string.IsNullOrWhiteSpace(parentName))
        {
            MenuItems.Add(toolMenuItem);
        }
        else
        {
            var parent = EnsureGroup(parentName);
            parent.Children.Add(toolMenuItem);
        }

        NotifyChanged();
    }

    public void AddSeparator()
    {
        if (MenuItems.Count == 0 || MenuItems[^1].IsSeparator)
        {
            return;
        }

        MenuItems.Add(new ToolMenuItem() { IsSeparator = true });
        NotifyChanged();
    }

    public void AddGroup(string name, string? icon = null)
    {
        var group = EnsureGroup(name);
        group.Icon ??= icon;
        NotifyChanged();
    }

    private ToolMenuItem EnsureGroup(string name)
    {
        // 模块加载顺序调整时，子菜单可能先于分组注册；这里统一兜底创建分组。
        var group = MenuItems.FirstOrDefault(item => !item.IsSeparator && item.Name == name);
        if (group != null)
        {
            return group;
        }

        group = new ToolMenuItem { Name = name, Children = [] };
        MenuItems.Add(group);
        return group;
    }

    private void NotifyChanged()
    {
        ToolMenuChanged?.Invoke();
    }
}
