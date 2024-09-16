using Avalonia.Controls;
using AvaloniaEdit;
using ReactiveUI;
using System.Diagnostics.Contracts;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;

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
        ErrorMessage = string.Empty;
        if (FormatEditor == null || NoFormatEditor == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(RawJson))
        {
            FormatEditor.Text = NoFormatEditor.Text = default;
            return;
        }


        try
        {
            var newJson = FormatJson(RawJson, IndentSize, IsSortKey);

            FormatEditor.IsVisible = IndentSize > 0;
            NoFormatEditor.IsVisible = IndentSize <= 0;
            if (IndentSize > 0)
            {
                FormatEditor.Text = newJson;
            }
            else
            {
                NoFormatEditor.Text = newJson;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    public static string FormatJson(string jsonString, int indent, bool sortKeys)
    {
        var writerOptions = new JsonWriterOptions
        {
            Indented = indent > 0, IndentSize = indent, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        using var jsonDoc = JsonDocument.Parse(jsonString);
        JsonElement sortedElement =
            sortKeys ? SortJsonElement(jsonDoc.RootElement, writerOptions) : jsonDoc.RootElement;
        using var stream = new MemoryStream();


        using (var writer = new Utf8JsonWriter(stream, writerOptions))
        {
            sortedElement.WriteTo(writer);
        }

        string formattedJsonString = System.Text.Encoding.UTF8.GetString(stream.ToArray());
        return formattedJsonString;
    }

    private static JsonElement SortJsonElement(JsonElement element, JsonWriterOptions writerOptions)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                var sortedObject = new SortedDictionary<string, JsonElement>();
                foreach (var property in element.EnumerateObject())
                {
                    sortedObject[property.Name] = SortJsonElement(property.Value, writerOptions);
                }

                using (var stream = new MemoryStream())
                {
                    using (var writer = new Utf8JsonWriter(stream, writerOptions))
                    {
                        writer.WriteStartObject();
                        foreach (var kvp in sortedObject)
                        {
                            writer.WritePropertyName(kvp.Key);
                            kvp.Value.WriteTo(writer);
                        }

                        writer.WriteEndObject();
                    }

                    return JsonDocument.Parse(stream.ToArray()).RootElement;
                }

            case JsonValueKind.Array:
                using (var stream = new MemoryStream())
                {
                    using (var writer = new Utf8JsonWriter(stream, writerOptions))
                    {
                        writer.WriteStartArray();
                        foreach (var item in element.EnumerateArray())
                        {
                            var sortedItem = SortJsonElement(item, writerOptions);
                            sortedItem.WriteTo(writer);
                        }

                        writer.WriteEndArray();
                    }

                    return JsonDocument.Parse(stream.ToArray()).RootElement;
                }

            case JsonValueKind.Undefined:
            case JsonValueKind.String:
            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:
            default:
                return element;
        }
    }

    private async Task RaiseCopyHandlerAsync()
    {
        string? newJson = IndentSize > 0 ? FormatEditor?.Text : NoFormatEditor?.Text;

        TopLevel.GetTopLevel(FormatEditor)?.Clipboard?.SetTextAsync(newJson);
    }
}