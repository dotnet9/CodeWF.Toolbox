using CodeWF.Modules.AI.Models;
using CodeWF.Modules.AI.Options;
using Microsoft.SemanticKernel;
using ReactiveUI;

namespace CodeWF.Modules.AI.ViewModels;

public class Title2SlugViewModel : ReactiveObject
{
    private readonly Kernel _kernel;
    private string? _askContent;

    public string? AskContent
    {
        get => _askContent;
        set => this.RaiseAndSetIfChanged(ref _askContent, value);
    }

    private string? _responseContent;

    public string? ResponseContent
    {
        get => _responseContent;
        set => this.RaiseAndSetIfChanged(ref _responseContent, value);
    }

    public Title2SlugViewModel()
    {
        var handler = new OpenAIHttpClientHandler();
        var builder = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: OpenAIOption.ChatModel,
                apiKey: OpenAIOption.Key,
                httpClient: new HttpClient(handler));
        _kernel = builder.Build();
    }

    public async void RaiseConvertCommandHandler()
    {
        if (!string.IsNullOrWhiteSpace(ResponseContent))
        {
            ResponseContent = "";
        }

        string skPrompt = """
                          {{$input}}

                          将上面的输入转成英文的URL Slug，无需任何其他内容
                          """;

        await foreach (var update in _kernel.InvokePromptStreamingAsync(skPrompt, new() { ["input"] = AskContent }))
        {
            ResponseContent += update.ToString();
        }
    }
}