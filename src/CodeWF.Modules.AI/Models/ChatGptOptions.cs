using System.Diagnostics;

namespace CodeWF.Modules.AI.Models;

internal class ChatGptOptions
{
    private const string BaseUrl = "https://dotnet9.com/";

    public string AskBotHttpUrl { get; set; } = $"{BaseUrl}ai/askbot";
    public string PolyTranslateHttpUrl { get; set; } = $"{BaseUrl}ai/polytranslate";
    public string Title2SlugHttpUrl { get; set; } = $"{BaseUrl}ai/title2slug";
}