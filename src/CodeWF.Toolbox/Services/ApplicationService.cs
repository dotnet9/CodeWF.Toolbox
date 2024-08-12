using Avalonia;
using Avalonia.Styling;
using CodeWF.Toolbox.Models;
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
                var theme = GetTheme();
                ChangeTheme(theme);
            }
            else
            {
                _applicationConfig = new ApplicationConfig() { Theme = ThemeVariant.Default.ToString() };
            }
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

            if (File.Exists(ConfigName))
            {
                File.Delete(ConfigName);
            }

            File.AppendAllText(ConfigName,
                JsonSerializer.Serialize(_applicationConfig, _jsonSerializerOptions));

            ChangeTheme(theme);
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
}