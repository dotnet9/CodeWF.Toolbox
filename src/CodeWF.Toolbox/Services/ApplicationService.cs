using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using AvaloniaXmlTranslator;
using CodeWF.Core.IServices;
using CodeWF.Toolbox.Models;
using CodeWF.Tools.Helpers;
using DynamicData;
using Semi.Avalonia;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Ursa.Controls;

namespace CodeWF.Toolbox.Services;

internal class ApplicationService : IApplicationService
{
    private const string ThemeKey = "Theme";
    private const string LanguageKey = "Language";
    private const string HideTrayIconOnCloseKey = "HideTrayIconOnClose";
    private const string NeedExitDialogOnCloseKey = "NeedExitDialogOnClose";

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

    public bool NeedExitDialogOnClose
    {
        get
        {
            return !AppConfigHelper.TryGet<bool>(NeedExitDialogOnCloseKey, out var needExitDialogOnClose) ||
                   needExitDialogOnClose;
        }
        set
        {
            AppConfigHelper.Set(NeedExitDialogOnCloseKey, value);
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

            var currentCulture = Thread.CurrentThread.CurrentCulture.Name;
            if (!string.IsNullOrWhiteSpace(currentCulture) &&
                I18nManager.Instance.Resources.ContainsKey(currentCulture))
            {
                return currentCulture;
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
        var themeObj = Themes.FirstOrDefault(item =>
            string.Equals(theme, item.Name, StringComparison.InvariantCultureIgnoreCase));
        App.Instance.RequestedThemeVariant = themeObj?.Theme;
    }

    private readonly Dictionary<string, ResourceDictionary> _semiCultures = new()
    {
        { "en-US", new Semi.Avalonia.Locale.en_us() },
        { "ja-JP", new Semi.Avalonia.Locale.ja_jp() },
        { "ru-RU", new Semi.Avalonia.Locale.ru_ru() },
        { "uk-UA", new Semi.Avalonia.Locale.uk_ua() },
        { "zh-CN", new Semi.Avalonia.Locale.zh_cn() }
    };

    private readonly Dictionary<string, ResourceDictionary> _ursaCultures = new()
    {
        { "en-US", new Ursa.Themes.Semi.Locale.en_us() }, { "zh-CN", new Ursa.Themes.Semi.Locale.zh_cn() }
    };

    private void ChangeCulture(string language)
    {
        void ChangeThirdCulture(ResourceDictionary res)
        {
            foreach (var kv in res)
            {
                if (App.Instance.Resources.ContainsKey(kv.Key))
                {
                    App.Instance.Resources.Remove(kv.Key);
                }

                App.Instance.Resources.Add(kv);
            }
        }

        // Change owner culture
        var culture = new CultureInfo(language);
        I18nManager.Instance.Culture = culture;

        // Change Semi.Avalonia culture

        if (_semiCultures.ContainsKey(language))
        {
            ChangeThirdCulture(_semiCultures[language]);
        }

        // Change Ursa.Avalonia culture

        if (_ursaCultures.ContainsKey(language))
        {
            ChangeThirdCulture(_ursaCultures[language]);
        }
    }
}