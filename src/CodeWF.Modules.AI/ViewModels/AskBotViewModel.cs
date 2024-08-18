using CodeWF.Modules.AI.Models;
using CodeWF.Modules.AI.Options;
using Microsoft.SemanticKernel;
using ReactiveUI;

namespace CodeWF.Modules.AI.ViewModels;

public class AskBotViewModel : ReactiveObject
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

    public AskBotViewModel()
    {
        var handler = new OpenAIHttpClientHandler();
        var builder = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: OpenAIOption.ChatModel,
                apiKey: OpenAIOption.Key,
                httpClient: new HttpClient(handler));
        _kernel = builder.Build();
    }

    public async void RaiseAskAICommandHandler()
    {
        if (!string.IsNullOrWhiteSpace(ResponseContent))
        {
            ResponseContent = "";
        }

        await foreach (var update in _kernel.InvokePromptStreamingAsync(AskContent))
        {
            ResponseContent += update.ToString();
        }
    }
}