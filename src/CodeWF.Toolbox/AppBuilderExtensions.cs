using Avalonia;
using Avalonia.Media;

namespace CodeWF.Toolbox;

public static class AppBuilderExtensions
{
    public const string SourceHanSansCnFontFamily =
        "avares://CodeWF.Toolbox/Assets/Fonts#Source Han Sans CN";

    public static AppBuilder WithBundledSourceHanSansCnFont(this AppBuilder appBuilder)
    {
        // 字体打包在应用资源中，避免不同系统默认中文字体导致界面字重和行高不一致。
        return appBuilder.With(new FontManagerOptions
        {
            DefaultFamilyName = SourceHanSansCnFontFamily,
            FontFallbacks =
            [
                new FontFallback
                {
                    FontFamily = new FontFamily(SourceHanSansCnFontFamily)
                }
            ]
        });
    }
}
