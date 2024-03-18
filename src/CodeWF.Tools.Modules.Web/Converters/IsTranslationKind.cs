namespace CodeWF.Tools.Modules.Web.Converters;

internal class IsTranslationKind : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
        {
            return false;
        }

        string? checkValue = parameter.ToString();
        return value.ToString() == checkValue;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return (TranslationKind)Enum.Parse(typeof(TranslationKind), parameter?.ToString()!);
    }
}