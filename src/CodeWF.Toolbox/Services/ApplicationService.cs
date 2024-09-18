using Avalonia;
using Avalonia.Styling;
using AvaloniaExtensions.Axaml.Markup;
using CodeWF.Core;
using CodeWF.Core.IServices;
using CodeWF.Tools.Helpers;
using System;
using System.Globalization;
using Ursa.Controls;

namespace CodeWF.Toolbox.Services;

internal class ApplicationService : IApplicationService
{
    private const string ThemeKey = "Theme";
    private const string LanguageKey = "Language";

    public void Load()
    {
        try
        {
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
        try
        {
            if (!AppConfigHelper.TryGet<string>(ThemeKey, out var theme))
            {
                return ThemeVariant.Default;
            }

            return theme switch
            {
                nameof(ThemeVariant.Dark) => ThemeVariant.Dark,
                nameof(ThemeVariant.Light) => ThemeVariant.Light,
                _ => ThemeVariant.Default
            };
        }
        catch (Exception ex)
        {
            MessageBox.ShowAsync(ex.ToString());
            return ThemeVariant.Dark;
        }
    }

    public void SetTheme(ThemeVariant theme)
    {
        try
        {
            AppConfigHelper.Set(ThemeKey, theme);
            ChangeTheme(theme);
        }
        catch
        {
            // ignored
        }
    }

    public string GetCulture()
    {
        try
        {
            if (!AppConfigHelper.TryGet<string>(LanguageKey, out var language))
            {
                return CultureNames.ChineseSimple;
            }

            return language!;
        }
        catch (Exception ex)
        {
            MessageBox.ShowAsync(ex.ToString());
            return CultureNames.ChineseSimple;
        }
    }

    public void SetCulture(string culture)
    {
        try
        {
            AppConfigHelper.Set(LanguageKey, culture);
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
}