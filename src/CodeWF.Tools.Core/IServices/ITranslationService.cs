namespace CodeWF.Tools.Core.IServices;

/// <summary>
///     文章标题翻译
/// </summary>
public interface ITranslationService
{
    /// <summary>
    ///     中英文翻译
    /// </summary>
    /// <param name="chineseText"></param>
    /// <returns></returns>
    public Task<string> ChineseToEnglishAsync(string? chineseText);

    /// <summary>
    ///     英中文翻译
    /// </summary>
    /// <param name="englishText"></param>
    /// <returns></returns>
    public Task<string> EnglishToChineseAsync(string? englishText);

    /// <summary>
    ///     英文与URL别名转换
    /// </summary>
    /// <param name="englishText"></param>
    /// <returns></returns>
    public string EnglishToUrlSlug(string? englishText);
}