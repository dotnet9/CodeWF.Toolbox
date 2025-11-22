using CodeWF.Modules.AI.Helpers;
using CodeWF.Modules.AI.Models;
using ReactiveUI;
using System.Reactive;

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

    public string? AskContent
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ResponseContent
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
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