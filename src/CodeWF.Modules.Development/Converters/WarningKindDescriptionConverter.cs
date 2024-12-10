using Avalonia.Data.Converters;
using AvaloniaXmlTranslator;
using CodeWF.Modules.Development.Models;
using CodeWF.Tools.Extensions;
using System.Globalization;

namespace CodeWF.Modules.Development.Converters;

public class WarningKindDescriptionConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is WarningKind kind)
        {
            return kind.GetDescription();
        }

        return "Nothing";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}