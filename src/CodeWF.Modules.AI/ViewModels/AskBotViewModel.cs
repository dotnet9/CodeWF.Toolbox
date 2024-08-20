using CodeWF.Modules.AI.Helpers;
using CodeWF.Modules.AI.Models;
using CodeWF.WebAPI.ViewModels;
using ReactiveUI;

namespace CodeWF.Modules.AI.ViewModels;

public class AskBotViewModel(ApiClient apiClient) : ReactiveObject
{
    private readonly ChatGptOptions _chatGptOptions = new();

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

    public async Task RaiseAskAICommandHandler()
    {
        if (string.IsNullOrWhiteSpace(AskContent))
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(ResponseContent))
        {
            ResponseContent = "";
        }

        try
        {
            await apiClient.CreateChatGptClient(_chatGptOptions.AskBotHttpUrl, new AskBotRequest(AskContent), result =>
            {
                ResponseContent += result;
            }, status =>
            {
                AskContent = string.Empty;
            });
        }
        catch (Exception ex)
        {
            ResponseContent = ex.Message;
        }
    }
}