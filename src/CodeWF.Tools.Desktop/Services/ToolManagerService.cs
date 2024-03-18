namespace CodeWF.Tools.Desktop.Services;

internal class ToolManagerService : IToolManagerService
{
    private readonly Dictionary<ToolType, string> _groupIcons = new()
    {
        { ToolType.Home, IconHelper.Home },
        { ToolType.Developer, IconHelper.Developer },
        { ToolType.Web, IconHelper.Web },
        { ToolType.Image, IconHelper.Image },
        { ToolType.Test, IconHelper.Test },
        { ToolType.Other, IconHelper.Other }
    };

    public void AddTool(string name, string description, string viewName, string icon, ToolStatus status)
    {
        MenuItems.Add(new ToolMenuItem
        {
            Group = ToolType.Home,
            Level = 0,
            Header = name,
            Description = description,
            ViewName = viewName,
            Icon = icon,
            Status = status
        });
        SendMenuChangedEvent();
    }

    public void AddTool(ToolType group, string name, string description, string viewName, string icon,
        ToolStatus status)
    {
        ToolMenuItem? toolGroup = MenuItems.FirstOrDefault(item => item.Group == group);
        if (toolGroup == null)
        {
            string desc = group.Description();
            toolGroup = new ToolMenuItem
            {
                Group = group,
                Level = 0,
                Header = desc,
                Description = desc,
                Icon = _groupIcons[group]
            };
            MenuItems.Add(toolGroup);
        }

        toolGroup.Children.Add(new ToolMenuItem
        {
            Group = group,
            Level = 1,
            Header = name,
            Description = description,
            ViewName = viewName,
            Icon = icon,
            Status = status
        });
        SendMenuChangedEvent();
    }

    public void RemoveTool(string name)
    {
        for (int i = MenuItems.Count; i >= 0; i--)
        {
            if (MenuItems[i].Header == name)
            {
                MenuItems.RemoveAt(i);
            }

            ToolMenuItem firstMenuItem = MenuItems[i];
            for (int j = 0; j < firstMenuItem.Children.Count; j++)
            {
                if (firstMenuItem.Children[j].Header == name)
                {
                    firstMenuItem.Children.RemoveAt(j);
                }
            }
        }

        SendMenuChangedEvent();
    }

    public ObservableCollection<ToolMenuItem> MenuItems { get; set; } = new();
    public event EventHandler? ToolMenuChanged;

    public void RemoveTool(ToolType type, string toolName)
    {
    }

    private void SendMenuChangedEvent()
    {
        ToolMenuChanged?.Invoke(this, default);
    }
}