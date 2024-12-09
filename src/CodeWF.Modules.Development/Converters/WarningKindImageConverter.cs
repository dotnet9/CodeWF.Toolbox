using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CodeWF.Modules.Development.Models;
using System.Globalization;

namespace CodeWF.Modules.Development.Converters;

public class WarningKindImageConverter : IValueConverter
{
    private static readonly Dictionary<WarningKind, Bitmap> KindImages = new()
    {
        {
            WarningKind.All,
            new Bitmap(AssetLoader.Open(new Uri("avares://CodeWF.Modules.Development/Assets/OvertimeAll.png")))
        },
        {
            WarningKind.Warning,
            new Bitmap(AssetLoader.Open(new Uri("avares://CodeWF.Modules.Development/Assets/OvertimeAlarm.png")))
        },
        {
            WarningKind.Normal,
            new Bitmap(AssetLoader.Open(new Uri("avares://CodeWF.Modules.Development/Assets/OvertimeNormal.png")))
        }
    };

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is WarningKind kind)
        {
            return KindImages[kind];
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}