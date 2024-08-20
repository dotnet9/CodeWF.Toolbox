using System.Diagnostics;

namespace CodeWF.Modules.AI.Models;

internal class ChatGptOptions
{
    private static string _baseUrl;
    static ChatGptOptions()
    {
        if (Debugger.IsAttached)
        {
            _baseUrl = "http://localhost:5242/";
        }
        else
        {
            _baseUrl = "https://api.dotnet9.com/";
        }
    }
    public string AskBotHttpUrl { get; set; } = $"{_baseUrl}ai/askbot";
    public string PolyTranslateHttpUrl { get; set; } = $"{_baseUrl}ai/polytranslate";
    public string Title2SlugHttpUrl { get; set; } = $"{_baseUrl}ai/title2slug";
}