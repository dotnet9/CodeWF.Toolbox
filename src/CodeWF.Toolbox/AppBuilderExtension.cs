using Avalonia.Media;

namespace Avalonia;

public static class AppBuilderExtension
{
    public static AppBuilder WithFont_SourceHanSansCN(this AppBuilder appBuilder)
    {
        const string uri = "avares://CodeWF.Toolbox/Assets/Fonts#Source Han Sans CN";
        return appBuilder.With(new FontManagerOptions()
        {
            DefaultFamilyName = uri,
            FontFallbacks = [new FontFallback { FontFamily = new FontFamily(uri) }]
        });
    }
}