using CodeWF.Modules.AI.Helpers;
using CodeWF.Modules.AI.Models;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;

namespace CodeWF.Modules.AI.ViewModels;

public class Title2SlugViewModel : ReactiveObject
{
    private readonly ApiClient _apiClient;
    private readonly ChatGptOptions _chatGptOptions = new();

    public Title2SlugViewModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
        RaiseConvertCommand = ReactiveCommand.CreateFromTask(RaiseConvertCommandHandlerAsync);
    }

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

    public ReactiveCommand<Unit, Unit> RaiseConvertCommand { get; }

    private async Task RaiseConvertCommandHandlerAsync()
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
            await _apiClient.CreateChatGptClient(_chatGptOptions.Title2SlugHttpUrl, new Title2SlugRequest(AskContent),
                result =>
                {
                    ResponseContent += result;
                }, status =>
                {
                    //AskContent = string.Empty;
                });
        }
        catch (Exception ex)
        {
            ResponseContent = ex.Message;
        }
    }
}