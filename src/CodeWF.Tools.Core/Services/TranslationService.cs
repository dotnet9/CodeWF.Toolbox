namespace CodeWF.Tools.Core.Services;

/// <summary>
///     中英互译使用Yandex Translation，Yandex使用了神经网络机器翻译技术（NMT），
///     以提供更高质量的翻译结果。Yandex Translation 支持多种语言对，包括一些
///     较少见的语言，并且特别擅长处理欧洲语言之间的翻译。
/// </summary>
public class TranslationService : ITranslationService
{
    private readonly SlugHelper _slugHelper = new();
    private readonly YandexTranslator _translator = new();

    /// <summary>
    ///     中英文翻译
    /// </summary>
    /// <param name="chineseText"></param>
    /// <returns></returns>
    public async Task<string> ChineseToEnglishAsync(string? chineseText)
    {
        return string.IsNullOrWhiteSpace(chineseText)
            ? string.Empty
            : (await _translator.TranslateAsync(chineseText, "en")).Translation;
    }

    /// <summary>
    ///     英中文翻译
    /// </summary>
    /// <param name="englishText"></param>
    /// <returns></returns>
    public async Task<string> EnglishToChineseAsync(string? englishText)
    {
        return string.IsNullOrWhiteSpace(englishText)
            ? string.Empty
            : (await _translator.TranslateAsync(englishText, "zh-CN")).Translation;
    }

    /// <summary>
    ///     英文与URL别名转换
    /// </summary>
    /// <param name="englishText"></param>
    /// <returns></returns>
    public string EnglishToUrlSlug(string? englishText)
    {
        return string.IsNullOrWhiteSpace(englishText) ? string.Empty : _slugHelper.GenerateSlug(englishText);
    }
}