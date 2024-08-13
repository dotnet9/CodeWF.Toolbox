using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace CodeWF.Toolbox.Converters;

public class LocalizeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string key)
        {
            return Localizer.Localizer.Instance[key];
        }

        return AvaloniaProperty.UnsetValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return AvaloniaProperty.UnsetValue;
    }
}