using Avalonia;
using Avalonia.Styling;
using AvaloniaExtensions.Axaml.Markup;
using CodeWF.Core;
using CodeWF.Core.IServiceInterfaces;
using CodeWF.Toolbox.Models;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace CodeWF.Toolbox.Services;

internal class ApplicationService : IApplicationService
{
    private const string ConfigName = "config.json";
    private ApplicationConfig? _applicationConfig;

    private readonly JsonSerializerOptions _jsonSerializerOptions =
        new() { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };

    public void Load()
    {
        try
        {
            if (_applicationConfig != null)
            {
                return;
            }

            if (File.Exists(ConfigName))
            {
                _applicationConfig =
                    JsonSerializer.Deserialize<ApplicationConfig>(File.ReadAllText(ConfigName), _jsonSerializerOptions);
            }
            else
            {
                _applicationConfig = new ApplicationConfig() { Theme = ThemeVariant.Default.ToString() };
            }

            var theme = GetTheme();
            ChangeTheme(theme);
            var culture = GetCulture();
            ChangeCulture(culture);
        }
        catch
        {
            // ignored
        }
    }

    public ThemeVariant GetTheme()
    {
        if (string.IsNullOrWhiteSpace(_applicationConfig?.Theme))
        {
            return ThemeVariant.Default;
        }

        return _applicationConfig.Theme switch
        {
            nameof(ThemeVariant.Dark) => ThemeVariant.Dark,
            nameof(ThemeVariant.Light) => ThemeVariant.Light,
            _ => ThemeVariant.Default
        };
    }

    public void SetTheme(ThemeVariant theme)
    {
        try
        {
            if (_applicationConfig == null)
            {
                _applicationConfig = new ApplicationConfig() { Theme = theme.ToString() };
            }
            else
            {
                _applicationConfig.Theme = theme.ToString();
            }

            Save();

            ChangeTheme(theme);
        }
        catch
        {
            // ignored
        }
    }

    public string GetCulture()
    {
        if (string.IsNullOrWhiteSpace(_applicationConfig?.Language))
        {
            return CultureNames.ChineseSimple;
        }

        return _applicationConfig.Language;
    }

    public void SetCulture(string culture)
    {
        try
        {
            if (_applicationConfig == null)
            {
                _applicationConfig = new ApplicationConfig() { Language = culture };
            }
            else
            {
                _applicationConfig.Language = culture;
            }

            Save();

            ChangeCulture(culture);
        }
        catch
        {
            // ignored
        }
    }

    private static void ChangeTheme(ThemeVariant theme)
    {
        var app = Application.Current;
        if (app is null)
        {
            return;
        }

        app.RequestedThemeVariant = theme;
    }

    private void ChangeCulture(string language)
    {
        I18nManager.Instance.Culture = new CultureInfo(language);
    }

    private void Save()
    {
        if (File.Exists(ConfigName))
        {
            File.Delete(ConfigName);
        }

        File.AppendAllText(ConfigName,
            JsonSerializer.Serialize(_applicationConfig, _jsonSerializerOptions));
    }
}