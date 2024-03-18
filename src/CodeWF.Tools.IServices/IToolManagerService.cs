namespace CodeWF.Tools.IServices;

public interface IToolManagerService
{
    ObservableCollection<ToolMenuItem> MenuItems { get; set; }
    void AddTool(string name, string description, string viewName, string icon, ToolStatus status);

    void AddTool(ToolType group, string name, string description, string viewName, string icon,
        ToolStatus status);

    void RemoveTool(string name);
    event EventHandler ToolMenuChanged;
}

public enum ToolType
{
    [Description("Home")] Home,
    [Description("开发类")] Developer,
    [Description("网站开发")] Web,
    [Description("图片处理")] Image,
    [Description("测试")] Test,
    [Description("未分类")] Other
}

public enum ToolStatus
{
    [Description("计划中")] Planned,
    [Description("开发中")] Developing,
    [Description("开发完成")] Complete
}

public class ToolMenuItem
{
    public ToolType Group { get; set; }
    public int Level { get; set; }
    public string? Header { get; set; }
    public string? Description { get; set; }
    public string? ViewName { get; set; }
    public ToolStatus Status { get; set; }
    public string? Icon { get; set; }
    public bool IsSeparator { get; set; }

    public ObservableCollection<ToolMenuItem> Children { get; set; } = new();

    public override string ToString()
    {
        return $"{Header}";
    }
}