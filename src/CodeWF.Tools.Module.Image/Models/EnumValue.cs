namespace CodeWF.Tools.Module.Image.Models;

public class EnumValue : ViewModelBase
{
    public string? Name { get; set; }
    public string? Description { get; set; }

    private bool _isSelected = true;

    public bool IsSelected
    {
        get => _isSelected;
        set => this.RaiseAndSetIfChanged(ref _isSelected, value);
    }
}