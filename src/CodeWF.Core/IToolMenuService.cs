using CodeWF.Core.Models;
using System.Collections.ObjectModel;

namespace CodeWF.Core;

public interface IToolMenuService
{
    ObservableCollection<ToolMenuItem> MenuItems { get; }
    event Action? ToolMenuChanged;

    void AddGroup(string name, string? icon = null);

    void AddItem(string name, string? parentName = null, string? description = null, string? viewName = null,
        string? icon = null, ToolStatus? status = null);

    void AddSeparator();
}