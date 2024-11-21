using Avalonia;
using Avalonia.Styling;
using AvaloniaExtensions.Axaml.Markup;
using CodeWF.Core;
using CodeWF.Core.IServices;
using CodeWF.Toolbox.Models;
using CodeWF.Tools.Helpers;
using Semi.Avalonia;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ursa.Controls;

namespace CodeWF.Toolbox.Services;

internal class ApplicationService : IApplicationService
{
    private const string ThemeKey = "Theme";
    private const string LanguageKey = "Language";
    private const string HideTrayIconOnCloseKey = "HideTrayIconOnClose";

    private const string DefaultTheme = "Dark";
    private const string DefaultLanguage = "zh-CN";

    public bool HideTrayIconOnClose
    {
        get
        {
            return !AppConfigHelper.TryGet<bool>(HideTrayIconOnCloseKey, out var hideTrayIconOnClose) ||
                   hideTrayIconOnClose;
        }
        set
        {
            AppConfigHelper.Set(HideTrayIconOnCloseKey, value);
        }
    }

    public List<ThemeItem> Themes { get; } = new()
    {
        new ThemeItem("Light", ThemeVariant.Light),
        new ThemeItem("Dark", ThemeVariant.Dark),
        new ThemeItem("Aquatic", SemiTheme.Aquatic),
        new ThemeItem("Desert", SemiTheme.Desert),
        new ThemeItem("Dust", SemiTheme.Dust),
        new ThemeItem("NightSky", SemiTheme.NightSky),
    };

    public List<LanguageItem> Languages { get; } = new()
    {
        new LanguageItem("en", "English"),
        new LanguageItem("zh-CN", "中文简体"),
        new LanguageItem("zh-Hant", "中文繁體"),
        new LanguageItem("ja-JP", "日本語"),
    };

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

    public string GetTheme()
    {
        try
        {
            if (AppConfigHelper.TryGet<string>(ThemeKey, out var theme) && !string.IsNullOrWhiteSpace(theme))
            {
                return theme;
            }

            return DefaultTheme;
        }
        catch (Exception ex)
        {
            MessageBox.ShowAsync(ex.ToString());
            return DefaultTheme;
        }
    }

    public void SetTheme(string theme)
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
            if (AppConfigHelper.TryGet<string>(LanguageKey, out var language) && !string.IsNullOrWhiteSpace(language))
            {
                return language;
            }

            return DefaultLanguage;
        }
        catch (Exception ex)
        {
            MessageBox.ShowAsync(ex.ToString());
            return DefaultLanguage;
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

    private void ChangeTheme(string theme)
    {
        var app = Application.Current;
        if (app is null)
        {
            return;
        }

        var themeObj = Themes.FirstOrDefault(item =>
            string.Equals(theme, item.Name, StringComparison.InvariantCultureIgnoreCase));
        app.RequestedThemeVariant = themeObj?.Theme;
    }

    private void ChangeCulture(string language)
    {
        I18nManager.Instance.Culture = new CultureInfo(language);
    }
}