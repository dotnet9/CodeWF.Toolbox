using Lang.Avalonia;

namespace CodeWF.Core.Helpers;

public static class LangHelper
{
    private static List<LocalizationLanguage>? _languages;

    public static List<LocalizationLanguage>? GetLanguages()
    {
        _languages ??=
        [
            new LocalizationLanguage { CultureName = "en-US", Description = "English", Language = "English" },
            new LocalizationLanguage
            {
                CultureName = "zh-CN", Description = "中文简体", Language = "Chinese (Simplified)"
            },

            new LocalizationLanguage
            {
                CultureName = "zh-Hant",
                Description = "中文繁体",
                Language = "Chinese (Traditional)"
            },

            new LocalizationLanguage { CultureName = "ja-JP", Description = "Japanese", Language = "Japanese" }
        ];

        return _languages;
    }
}