using ReactiveUI;
using System.Collections.ObjectModel;

namespace CodeWF.Modules.ToolFramework.Models;

public sealed class ToolField : ReactiveObject
{
    private string _text = string.Empty;
    private decimal _number;
    private bool _boolean;
    private ToolOption? _selectedOption;

    public required string Id { get; init; }

    public required string Label { get; set; }

    public string Placeholder { get; set; } = string.Empty;

    public ToolFieldKind Kind { get; init; }

    public ObservableCollection<ToolOption> Options { get; } = [];

    public string Text
    {
        get => _text;
        set => this.RaiseAndSetIfChanged(ref _text, value ?? string.Empty);
    }

    public decimal Number
    {
        get => _number;
        set => this.RaiseAndSetIfChanged(ref _number, value);
    }

    public bool Boolean
    {
        get => _boolean;
        set => this.RaiseAndSetIfChanged(ref _boolean, value);
    }

    public ToolOption? SelectedOption
    {
        get => _selectedOption;
        set => this.RaiseAndSetIfChanged(ref _selectedOption, value);
    }

    public bool IsText => Kind == ToolFieldKind.Text;

    public bool IsMultiLine => Kind == ToolFieldKind.MultiLine;

    public bool IsNumber => Kind == ToolFieldKind.Number;

    public bool IsBoolean => Kind == ToolFieldKind.Boolean;

    public bool IsSelect => Kind == ToolFieldKind.Select;

    public bool IsFile => Kind == ToolFieldKind.File;

    public bool IsSaveFile => Kind == ToolFieldKind.SaveFile;

    public bool IsPath => IsFile || IsSaveFile;
}

