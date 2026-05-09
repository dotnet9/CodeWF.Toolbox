using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Collections.Concurrent;
using System.Globalization;

namespace CodeWF.Toolbox.Converters;

public class IconConverter : IValueConverter
{
    private static readonly ConcurrentDictionary<string, StreamGeometry> IconCache = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string icon)
        {
            return IconCache.GetOrAdd(icon, StreamGeometry.Parse);
        }

        return AvaloniaProperty.UnsetValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return AvaloniaProperty.UnsetValue;
    }
}