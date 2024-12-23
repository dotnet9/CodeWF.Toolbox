using CodeWF.Tools.Extensions;
using ReactiveUI;
using System.ComponentModel;

namespace CodeWF.Modules.Converter.Models;

public enum IconSize
{
    [Description("16x16")] Size16 = 16,
    [Description("24x24")] Size24 = 24,
    [Description("32x32")] Size32 = 32,
    [Description("48x48")] Size48 = 48,
    [Description("64x64")] Size64 = 64,
    [Description("128x128")] Size128 = 128,
    [Description("256x256")] Size256 = 256
}

public class IconSizeItem(IconSize size) : ReactiveObject
{
    private bool _isSelected = true;

    public bool IsSelected
    {
        get => _isSelected;
        set => this.RaiseAndSetIfChanged(ref _isSelected, value);
    }

    public string Content { get; set; } = size.GetDescription();
    public IconSize Size { get; set; } = size;
}