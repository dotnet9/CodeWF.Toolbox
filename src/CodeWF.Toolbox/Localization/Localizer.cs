using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace CodeWF.Toolbox.Localization;

public class Localizer : INotifyPropertyChanged
{
    private const string IndexerName = "Item";
    private const string IndexerArrayName = "Item[]";
    private Dictionary<string, string>? _languageKeyAndValues;

    private readonly JsonSerializerOptions _jsonSerializerOptions =
        new() { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };

    public bool LoadLanguage(string language)
    {
        Language = language;

        Uri uri = new($"avares://CodeWF.Toolbox/Assets/i18n/{language}.json");
        if (!AssetLoader.Exists(uri))
        {
            return false;
        }

        using (var sr = new StreamReader(AssetLoader.Open(uri), Encoding.UTF8))
        {
            _languageKeyAndValues =
                JsonSerializer.Deserialize<Dictionary<string, string>>(sr.ReadToEnd(), _jsonSerializerOptions);
        }

        Invalidate();

        return true;
    }

    private string? _language;

    public string? Language
    {
        get => _language;
        private set
        {
            _language = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Language));
        }
    }

    public string this[string key]
    {
        get
        {
            if (_languageKeyAndValues != null && _languageKeyAndValues.TryGetValue(key, out var res))
                return res.Replace("\\n", "\n");

            return $"{Language}:{key}";
        }
    }

    public static Localizer Instance { get; set; } = new();
    public event PropertyChangedEventHandler? PropertyChanged;

    public void Invalidate()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerName));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerArrayName));
    }
}