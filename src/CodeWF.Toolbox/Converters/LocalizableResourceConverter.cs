using Avalonia.Data.Converters;
using CodeWF.Toolbox.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CodeWF.Toolbox.Converters;

public class LocalizableResourceConverter(LocalizableResourceExtension owner) : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values[0] is not CultureInfo _)
        {
            return default;
        }

        var value = values[1];
        if (owner.KeyConverter.Convert(value, null, null, culture) is not string key)
        {
            return value;
        }

        value = LocalizationManager.GetObject(key) ?? key;
        if (value is string format)
        {
            value = string.Format(format, owner.Args.Indexes
                .Select(item => item.IsBinding ? values[item.Index] : owner.Args[item.Index])
                .ToArray());
        }

        return owner.ValueConverter.Convert(value, null, null, culture);
    }
}