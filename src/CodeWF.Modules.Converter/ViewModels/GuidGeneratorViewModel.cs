using Avalonia.Controls;
using ReactiveUI;
using System.Reactive;
using System.Text;

namespace CodeWF.Modules.Converter.ViewModels;

public class GuidGeneratorViewModel : ReactiveObject
{
    private static readonly string[] Formats = ["D", "N", "B", "P"];

    public GuidGeneratorViewModel()
    {
        GenerateCommand = ReactiveCommand.Create(Generate);
        CopyCommand = ReactiveCommand.CreateFromTask(CopyAsync);
        ClearCommand = ReactiveCommand.Create(Clear);
        Generate();
    }

    public Control? ClipboardOwner { get; set; }

    public int Count
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, Math.Clamp(value, 1, 100));
    } = 5;

    public int FormatIndex
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, Math.Clamp(value, 0, Formats.Length - 1));
    }

    public bool Uppercase
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string OutputText
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = string.Empty;

    public ReactiveCommand<Unit, Unit> GenerateCommand { get; }

    public ReactiveCommand<Unit, Unit> CopyCommand { get; }

    public ReactiveCommand<Unit, Unit> ClearCommand { get; }

    private void Generate()
    {
        var builder = new StringBuilder();
        var format = Formats[FormatIndex];

        for (var i = 0; i < Count; i++)
        {
            var value = Guid.NewGuid().ToString(format);
            builder.AppendLine(Uppercase ? value.ToUpperInvariant() : value);
        }

        OutputText = builder.ToString().TrimEnd();
    }

    private void Clear()
    {
        OutputText = string.Empty;
    }

    private async Task CopyAsync()
    {
        await (TopLevel.GetTopLevel(ClipboardOwner)?.Clipboard?.SetTextAsync(OutputText) ?? Task.CompletedTask);
    }
}
