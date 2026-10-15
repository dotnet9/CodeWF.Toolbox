﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using global::Avalonia.Controls;
using global::Avalonia.Styling;
using CodeWF.Core.Events;
using CodeWF.Core.Helpers;
using CodeWF.Core.IServices;
using CodeWF.Toolbox.Models;
using CodeWF.Toolkit.Files.Helpers;
using CodeWF.Toolkit.EventBus;
using Semi.Avalonia;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CodeWF.Toolkit.Lang;
using Ursa.Controls;
using CodeWF.Avalonia.Controls.Themes;

namespace CodeWF.Toolbox.Services;

internal class ApplicationService : IApplicationService
{
    private const string ThemeKey = "Theme";
    private const string PaletteKey = "StudioPalette";
    private const string ModeKey = "StudioMode";
    private StudioThemeController? _themeController;
    public StudioThemeController ThemeController => _themeController ??= new StudioThemeController(App.Instance);
    private const string LanguageKey = "Language";
    private const string AutoOpenToolboxAtStartupKey = "AutoOpenToolboxAtStartup";
    private const string HideTrayIconOnCloseKey = "HideTrayIconOnClose";
    private const string NeedExitDialogOnCloseKey = "NeedExitDialogOnClose";

    private const string DefaultTheme = "DotNet";
    private const string DefaultLanguage = "zh-CN";

    public bool AutoOpenToolboxAtStartup
    {
        get
        {
            return !AppConfigHelper.TryGet<bool>(AutoOpenToolboxAtStartupKey, out var autoOpenToolboxAtStartup) ||
                   autoOpenToolboxAtStartup;
        }
        set
        {
            AppConfigHelper.Set(AutoOpenToolboxAtStartupKey, value);
            _ = StartupHelper.ModifyStartupSettingAsync(value);
        }
    }

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
        new(Localization.Workspace.DotNet, "DotNet", ThemeVariant.Light),
        new(Localization.Workspace.Aurora, "Aurora", ThemeVariant.Light),
        new(Localization.Workspace.Ember, "Ember", ThemeVariant.Light),
        new(Localization.Workspace.Default, "Default", ThemeVariant.Light),
        new(Localization.Workspace.Aquatic, "Aquatic", ThemeVariant.Light),
        new(Localization.Workspace.Desert, "Desert", ThemeVariant.Light),
        new(Localization.Workspace.Dusk, "Dusk", ThemeVariant.Light),
        new(Localization.Workspace.NightSky, "NightSky", ThemeVariant.Light),
    };

    public void Load()
    {
        try
        {
            var theme = GetTheme();
            ChangeTheme(theme);
            ThemeController.Mode = Enum.TryParse<StudioThemeMode>(GetMode(), out var mode) ? mode : StudioThemeMode.System;
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
            if (AppConfigHelper.TryGet<string>(PaletteKey, out var theme) && Enum.TryParse<StudioPalette>(theme, out _))
            {
                return theme;
            }

            if (AppConfigHelper.TryGet<string>(ThemeKey, out var legacy) && legacy is "Aquatic" or "Desert" or "Dusk" or "NightSky") return legacy;

            return DefaultTheme;
        }
        catch (Exception ex)
        {
            _ = MessageBox.ShowAsync(ex.ToString());
            return DefaultTheme;
        }
    }

    public void SetTheme(string theme)
    {
        try
        {
            AppConfigHelper.Set(PaletteKey, theme);
            ChangeTheme(theme);
            EventBus.Default.Publish(new ThemeChangedCommand());
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
                I18nManager.Instance.GetLanguages()?.Exists(c => c.CultureName == currentCulture) == true)
            {
                return currentCulture;
            }

            return DefaultLanguage;
        }
        catch (Exception ex)
        {
            _ = MessageBox.ShowAsync(ex.ToString());
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
        ThemeController.Palette = Enum.TryParse<StudioPalette>(theme, out var palette) ? palette : StudioPalette.DotNet;
    }

    public string GetMode()
    {
        if (AppConfigHelper.TryGet<string>(ModeKey, out var mode) && Enum.TryParse<StudioThemeMode>(mode, out _)) return mode;
        return AppConfigHelper.TryGet<string>(ThemeKey, out var legacy) && legacy is "Light" or "Dark" ? legacy : "System";
    }

    public void SetMode(string mode)
    {
        if (!Enum.TryParse<StudioThemeMode>(mode, out var value)) return;
        ThemeController.Mode = value;
        AppConfigHelper.Set(ModeKey, mode);
        EventBus.Default.Publish(new ThemeChangedCommand());
    }

    private void ChangeCulture(string language)
    {
        I18nManager.Instance.Culture = new CultureInfo(language);
    }
}
