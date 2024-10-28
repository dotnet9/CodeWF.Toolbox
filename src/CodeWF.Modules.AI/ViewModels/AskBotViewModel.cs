using CodeWF.Modules.AI.Helpers;
using CodeWF.Modules.AI.Models;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;

namespace CodeWF.Modules.AI.ViewModels;

public class AskBotViewModel : ReactiveObject
{
    private readonly ApiClient _apiClient;
    private readonly ChatGptOptions _chatGptOptions = new();

    public AskBotViewModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
        this.WhenAnyValue(x => x.AskContent)
            .Throttle(TimeSpan.FromSeconds(1))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => RaiseAskAICommandHandlerAsync());
        RaiseAskAICommand = ReactiveCommand.CreateFromTask(RaiseAskAICommandHandlerAsync);
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

    public ReactiveCommand<Unit, Unit> RaiseAskAICommand { get; }

    private async Task RaiseAskAICommandHandlerAsync()
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
            await _apiClient.CreateChatGptClient(_chatGptOptions.AskBotHttpUrl, new AskBotRequest(AskContent), result =>
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