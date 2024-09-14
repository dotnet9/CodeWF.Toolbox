using AvaloniaEdit;
using ReactiveUI;
using System.Reactive.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CodeWF.Modules.Development.ViewModels;

public class YamlPrettifyViewModel : ReactiveObject
{
    private IDeserializer? _deserializer;
    private ISerializer? _serializer;
    public TextEditor YamlTextEditor { get; set; }

    public YamlPrettifyViewModel()
    {
        this.WhenAnyValue(x => x.RawYaml)
            .Throttle(TimeSpan.FromMilliseconds(400))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(RawYamlChanged);
    }

    private bool _isSortKey = false;

    public bool IsSortKey
    {
        get => _isSortKey;
        set => this.RaiseAndSetIfChanged(ref _isSortKey, value);
    }

    public List<int> IndentSizes { get; } = Enumerable.Range(1, 10).ToList();

    private int _indentSize = 2;

    public int IndentSize
    {
        get => _indentSize;
        set => this.RaiseAndSetIfChanged(ref _indentSize, value);
    }

    private string? _rawYaml;

    public string? RawYaml
    {
        get => _rawYaml;
        set => this.RaiseAndSetIfChanged(ref _rawYaml, value);
    }

    private string? _prettifiedYaml;

    public string? PrettifiedYaml
    {
        get => _prettifiedYaml;
        set => this.RaiseAndSetIfChanged(ref _prettifiedYaml, value);
    }

    private string? _errorMessage;

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    private void RawYamlChanged(string? newRawYaml)
    {
        if (string.IsNullOrWhiteSpace(newRawYaml))
        {
            YamlTextEditor.Text = default;
            return;
        }

        _deserializer ??= new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();

        _serializer ??= new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        try
        {
            var obj = _deserializer.Deserialize(RawYaml);
            YamlTextEditor.Text = _serializer.Serialize(obj);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }
}