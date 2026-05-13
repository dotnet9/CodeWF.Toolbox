using Avalonia;
using Avalonia.Media;

namespace CodeWF.Toolbox;

public static class AppBuilderExtensions
{
    private const string BundledAppFontFamily =
        "avares://CodeWF.Toolbox/Assets/Fonts#Noto Sans SC";

    public const string AppFontFamily =
        "Microsoft YaHei UI, Segoe UI, avares://CodeWF.Toolbox/Assets/Fonts#Noto Sans SC";

    public static AppBuilder WithBundledAppFont(this AppBuilder appBuilder)
    {
        // AvaloniaEdit can query the default typeface during construction, before XAML styles are applied.
        // Keep the app default on a bundled font so Linux builds do not depend on system font discovery.
        return appBuilder.With(new FontManagerOptions
        {
            DefaultFamilyName = BundledAppFontFamily,
            FontFallbacks =
            [
                new FontFallback
                {
                    FontFamily = new FontFamily(BundledAppFontFamily)
                }
            ]
        });
    }
}
