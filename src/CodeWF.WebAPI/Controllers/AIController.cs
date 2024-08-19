using CodeWF.WebAPI.Models;
using CodeWF.WebAPI.Options;
using CodeWF.WebAPI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

namespace CodeWF.WebAPI.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class AIController
    : ControllerBase
{
    private readonly Kernel _kernel;

    public AIController(IOptions<OpenAIOption> openAIOption, OpenAIHttpClientHandler httpClientHandler,
        ILogger<AIController> logger)
    {
        var builder = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: openAIOption.Value.ChatModel!,
                apiKey: openAIOption.Value.Key!,
                httpClient: new HttpClient(httpClientHandler));
        _kernel = builder.Build();
    }

    [HttpPost]
    public async Task<IResult> AskBotAsync([FromBody] AskBotRequest request)
    {
        var content = _kernel.InvokePromptStreamingAsync(request.Content);
        await WriteResponseAsync(content);
        return Results.Empty;
    }

    [HttpPost]
    public async Task<IResult> PolyTranslateAsync([FromBody] PolyTranslateRequest request)
    {
        const string skPrompt = """  
                                {{$input}}  

                                将上面的输入翻译成{{$language}}，无需任何其他内容  
                                """;
        var content = _kernel.InvokePromptStreamingAsync(skPrompt,
            new KernelArguments { ["input"] = request.Content, ["language"] = string.Join(",", request.Languages) });
        await WriteResponseAsync(content);
        return Results.Empty;
    }

    [HttpPost]
    public async Task<IResult> Title2SlugAsync([FromBody] Title2SlugRequest request)
    {
        const string skPrompt = """  
                                {{$input}}  

                                将上面的输入转成英文的URL Slug，无需任何其他内容  
                                """;
        var content = _kernel.InvokePromptStreamingAsync(skPrompt,
            new KernelArguments { ["input"] = request.Content });
        await WriteResponseAsync(content);
        return Results.Empty;
    }

    private async Task WriteResponseAsync(IAsyncEnumerable<StreamingKernelContent> content)
    {
        Response.Headers.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        await Response.Body.FlushAsync();

        await foreach (var item in content)
        {
            await Response.WriteAsync(item.ToString());
            await Response.Body.FlushAsync();
        }

        await Response.Body.FlushAsync();
    }
}