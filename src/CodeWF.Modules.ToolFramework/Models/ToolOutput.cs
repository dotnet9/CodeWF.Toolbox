using Avalonia.Media.Imaging;
using ReactiveUI;

namespace CodeWF.Modules.ToolFramework.Models;

public sealed class ToolOutput : ReactiveObject
{
    private string _text = string.Empty;
    private Bitmap? _bitmap;

    public required string Id { get; init; }

    public required string Label { get; set; }

    public bool IsMultiline { get; init; } = true;

    public string Text
    {
        get => _text;
        set => this.RaiseAndSetIfChanged(ref _text, value ?? string.Empty);
    }

    public Bitmap? Bitmap
    {
        get => _bitmap;
        set
        {
            this.RaiseAndSetIfChanged(ref _bitmap, value);
            this.RaisePropertyChanged(nameof(HasBitmap));
            this.RaisePropertyChanged(nameof(HasText));
        }
    }

    public bool HasBitmap => Bitmap != null;

    public bool HasText => Bitmap == null;
}

