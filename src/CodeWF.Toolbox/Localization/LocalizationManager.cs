using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Threading;

namespace CodeWF.Toolbox.Localization;
public class LocalizationManager : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private CultureInfo culture;

    private LocalizationManager()
    {;
        this.Resources = new Dictionary<string, object>();
        this.culture = CultureInfo.InvariantCulture;
    }

    public static LocalizationManager Instance { get; } = new LocalizationManager();

    public CultureInfo Culture
    {
        get => this.culture;
        set
        {
            if (Equals(this.culture, value))
            {
                return;
            }
            this.culture = value;
            Thread.CurrentThread.CurrentCulture = value;
            Thread.CurrentThread.CurrentUICulture = value;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Culture)));
            this.CultureChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public Dictionary<string, object> Resources { get; }

    public event EventHandler<EventArgs>? CultureChanged;


    public static T? GetResource<T>(string key)
    {
        if (Localizer.Instance[key] is T result)
        {
            return result;
        }

        return default;
    }

    public static object? GetObject(string key)
    {
        return GetResource<object>(key);
    }

    public static string? GetString(string key)
    {
        return GetResource<string>(key);
    }
}