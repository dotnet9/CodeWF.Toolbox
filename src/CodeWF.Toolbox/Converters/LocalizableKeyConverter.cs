using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace CodeWF.Toolbox.Converters;
public class LocalizableKeyConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Get(value);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    public static string Get(object value)
    {
        return value switch
        {
            Enum v => $"{v.GetType().Name}_{v}",
            string key => key,
            _ => string.Empty
        };
    }
}