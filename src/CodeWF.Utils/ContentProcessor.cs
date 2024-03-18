namespace CodeWF.Utils;

public static class ContentProcessor
{
    public enum MarkdownConvertType
    {
        None = 0,
        Html = 1,
        Text = 2
    }

    public static string ReplaceCDNEndpointToImgTags(this string html, string endpoint)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return html;
        }

        endpoint = endpoint.TrimEnd('/');
        Regex imgSrcRegex = new("<img.+?(src)=[\"'](.+?)[\"'].+?>");
        string newStr = imgSrcRegex.Replace(html,
            match => match.Value.Contains("src=\"/image/")
                ? match.Value.Replace("/image/", $"{endpoint}/")
                : match.Value);

        return newStr;
    }

    public static string GetPostAbstract(string content, int wordCount, bool useMarkdown = false)
    {
        string plainText = useMarkdown ? MarkdownToContent(content, MarkdownConvertType.Text) : RemoveTags(content);

        string result = plainText.Ellipsize(wordCount);
        return result;
    }

    public static string RemoveTags(string html)
    {
        if (string.IsNullOrEmpty(html))
        {
            return string.Empty;
        }

        UglifyResult result = Uglify.HtmlToText(html);

        return !result.HasErrors && !string.IsNullOrWhiteSpace(result.Code)
            ? result.Code.Trim()
            : RemoveTagsBackup(html);
    }

    public static string Ellipsize(this string text, int characterCount)
    {
        return text.Ellipsize(characterCount, "\u00A0\u2026");
    }

    public static bool IsLetter(this char c)
    {
        return c is >= 'a' and <= 'z' or >= 'A' and <= 'Z';
    }

    public static bool IsSpace(this char c)
    {
        return c is '\r' or '\n' or '\t' or '\f' or ' ';
    }

    public static string MarkdownToContent(string markdown, MarkdownConvertType type, bool disableHtml = true)
    {
        MarkdownPipelineBuilder pipeline = new MarkdownPipelineBuilder()
            .UsePipeTables()
            .UseBootstrap();

        if (disableHtml)
        {
            pipeline.DisableHtml();
        }

        string result = type switch
        {
            MarkdownConvertType.None => markdown,
            MarkdownConvertType.Html => Markdown.ToHtml(markdown, pipeline.Build()),
            MarkdownConvertType.Text => Markdown.ToPlainText(markdown, pipeline.Build()),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        return result;
    }

    #region Private Methods

    private static string RemoveTagsBackup(string html)
    {
        char[] result = new char[html.Length];

        int cursor = 0;
        bool inside = false;
        foreach (char current in html)
        {
            switch (current)
            {
                case '<':
                    inside = true;
                    continue;
                case '>':
                    inside = false;
                    continue;
            }

            if (!inside)
            {
                result[cursor++] = current;
            }
        }

        string stringResult = new(result, 0, cursor);

        return stringResult.Replace("&nbsp;", " ");
    }

    private static string Ellipsize(this string text, int characterCount, string ellipsis, bool wordBoundary = false)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return "";
        }

        if (characterCount < 0 || text.Length <= characterCount)
        {
            return text + ellipsis;
        }

        // search beginning of word
        int backup = characterCount;
        while (characterCount > 0 && text[characterCount - 1].IsLetter())
        {
            characterCount--;
        }

        // search previous word
        while (characterCount > 0 && text[characterCount - 1].IsSpace())
        {
            characterCount--;
        }

        // if it was the last word, recover it, unless boundary is requested
        if (characterCount == 0 && !wordBoundary)
        {
            characterCount = backup;
        }

        string trimmed = text[..characterCount];
        return trimmed + ellipsis;
    }

    #endregion
}