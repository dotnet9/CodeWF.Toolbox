using Avalonia;
using Avalonia.Media;

namespace CodeWF.Toolbox;

public static class AppBuilderExtensions
{
    private const string BundledAppFontFamily =
        "avares://codewf/Assets/Fonts#Noto Sans SC";

    public const string AppFontFamily =
        "Microsoft YaHei UI, Segoe UI, avares://codewf/Assets/Fonts#Noto Sans SC";

    public static AppBuilder WithBundledAppFont(this AppBuilder appBuilder)
    {
        // Windows 优先使用系统 UI 字体保证清晰度，其他平台回退到随包字体。
        return appBuilder.With(new FontManagerOptions
        {
            DefaultFamilyName = AppFontFamily,
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