namespace CodeWF.Modules.AI.Models;

internal class ChatGptOptions
{
    public string AskBotHttpUrl { get; set; } = "http://localhost:5242/ai/askbot";
    public string PolyTranslateHttpUrl { get; set; } = "http://localhost:5242/ai/polytranslate";
    public string Title2SlugHttpUrl { get; set; } = "http://localhost:5242/ai/title2slug";
}