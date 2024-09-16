using Avalonia.Controls;
using AvaloniaEdit;
using CodeWF.Tools.Extensions;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;

namespace CodeWF.Modules.Development.ViewModels;

public class JsonPrettifyViewModel : ReactiveObject
{
    public TextEditor? FormatEditor { get; set; }
    public TextBox? NoFormatEditor { get; set; }

    public JsonPrettifyViewModel()
    {
        RaiseCopyCommand = ReactiveCommand.CreateFromTask(RaiseCopyHandlerAsync);

        this.WhenAnyValue(x => x.RawJson)
            .Throttle(TimeSpan.FromMilliseconds(400))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => RawJsonChanged());

        this.WhenAnyValue(x => x.IsSortKey)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => RawJsonChanged());

        this.WhenAnyValue(x => x.IndentSize)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => RawJsonChanged());
    }

    private bool _isSortKey = false;

    public bool IsSortKey
    {
        get => _isSortKey;
        set => this.RaiseAndSetIfChanged(ref _isSortKey, value);
    }

    public List<int> IndentSizes { get; } = Enumerable.Range(0, 10).ToList();

    private int _indentSize = 2;

    public int IndentSize
    {
        get => _indentSize;
        set => this.RaiseAndSetIfChanged(ref _indentSize, value);
    }


    private string? _rawJson;

    public string? RawJson
    {
        get => _rawJson;
        set => this.RaiseAndSetIfChanged(ref _rawJson, value);
    }

    private string? _errorMessage;

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    public ReactiveCommand<Unit, Unit> RaiseCopyCommand { get; }

    private void RawJsonChanged()
    {
        if (FormatEditor == null || NoFormatEditor == null)
        {
            return;
        }

        FormatEditor.IsVisible = IndentSize > 0;
        NoFormatEditor.IsVisible = IndentSize <= 0;
        if (RawJson.JsonPrettify(IndentSize, IsSortKey, out var newJson, out var errorMessage))
        {
            if (IndentSize > 0)
            {
                FormatEditor.Text = newJson;
            }
            else
            {
                NoFormatEditor.Text = newJson;
            }

            ErrorMessage = default;
        }
        else
        {
            FormatEditor!.Text = NoFormatEditor.Text = string.Empty;
            ErrorMessage = errorMessage;
        }
    }

    private async Task RaiseCopyHandlerAsync()
    {
        string? newJson = IndentSize > 0 ? FormatEditor?.Text : NoFormatEditor?.Text;

        TopLevel.GetTopLevel(FormatEditor)?.Clipboard?.SetTextAsync(newJson);
    }
}