using Avalonia.Controls;
using AvaloniaEdit;
using ReactiveUI;
using System.Reactive;
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
        RaiseCopyCommand = ReactiveCommand.CreateFromTask(RaiseCopyHandlerAsync);

        this.WhenAnyValue(x => x.RawYaml)
            .Throttle(TimeSpan.FromMilliseconds(400))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(RawYamlChanged);
    }

    private string? _rawYaml;

    public string? RawYaml
    {
        get => _rawYaml;
        set => this.RaiseAndSetIfChanged(ref _rawYaml, value);
    }

    private string? _errorMessage;

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }
    public ReactiveCommand<Unit, Unit> RaiseCopyCommand { get; }

    private void RawYamlChanged(string? newRawYaml)
    {
        ErrorMessage = string.Empty;
        if (string.IsNullOrWhiteSpace(RawYaml))
        {
            YamlTextEditor.Text = default;
            return;
        }

        _deserializer ??= new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();

        _serializer ??= new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithIndentedSequences()
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

    private async Task RaiseCopyHandlerAsync()
    {
        TopLevel.GetTopLevel(YamlTextEditor)?.Clipboard?.SetTextAsync(YamlTextEditor.Text);
    }
}