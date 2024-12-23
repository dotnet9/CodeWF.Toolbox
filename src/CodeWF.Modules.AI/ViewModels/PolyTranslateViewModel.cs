using AvaloniaXmlTranslator;
using CodeWF.AvaloniaControls.Extensions;
using CodeWF.Core;
using CodeWF.Modules.AI.Helpers;
using CodeWF.Modules.AI.Models;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using Ursa.Controls;
using Ursa.PrismExtension;

namespace CodeWF.Modules.AI.ViewModels;

public class PolyTranslateViewModel : ReactiveObject
{
    private readonly IContainerExtension _container;
    private readonly ApiClient _apiClient;
    private readonly IUrsaOverlayDialogService _overlayDialogService;
    private List<string> _choiceLanguages;
    private readonly ChatGptOptions _chatGptOptions = new();

    public PolyTranslateViewModel(IContainerExtension container, ApiClient apiClient,
        IUrsaOverlayDialogService overlayDialogService)
    {
        _container = container;
        _apiClient = apiClient;
        _overlayDialogService = overlayDialogService;

        _choiceLanguages = ConfigHelper.GetTranslateLanguages() ??
        [
            "Chinese Simplified", "Chinese Traditional", "English (United States)", "Japanese (Japan)"
        ];
        Languages = string.Join(",", _choiceLanguages);
        RaiseTranslateCommand = ReactiveCommand.CreateFromTask(RaiseTranslateCommandHandlerAsync);
        RaiseChoiceLanguagesCommand = ReactiveCommand.CreateFromTask(RaiseChoiceLanguagesCommandHandlerAsync);
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

    private string? _languages;

    public string? Languages
    {
        get => _languages;
        set => this.RaiseAndSetIfChanged(ref _languages, value);
    }

    public ReactiveCommand<Unit, Unit> RaiseTranslateCommand { get; }
    public ReactiveCommand<Unit, Unit> RaiseChoiceLanguagesCommand { get; }

    private async Task RaiseTranslateCommandHandlerAsync()
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
            await _apiClient.CreateChatGptClient(_chatGptOptions.PolyTranslateHttpUrl,
                new PolyTranslateRequest(AskContent, _choiceLanguages), result =>
                {
                    ResponseContent += result + "\r\n";
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

    private async Task RaiseChoiceLanguagesCommandHandlerAsync()
    {
        var option =
            new OverlayDialogOptions()
            {
                Title = I18nManager.Instance.GetResource(Localization.ChoiceLanguagesView.LanguageKey), Buttons = DialogButton.OK
            };

        var vm = _container.Resolve<ChoiceLanguagesViewModel>();
        vm.SelectedLanguages = new RangeObservableCollection<string> { _choiceLanguages };
        vm.AllLanguages = new RangeObservableCollection<string> { LanguageList.Languages.Except(_choiceLanguages) };
        await _overlayDialogService.ShowModal(DialogNames.ChoiceLanguages, vm, HostIds.Main, option);
        _choiceLanguages = vm.SelectedLanguages.ToList();
        Languages = string.Join(",", vm.SelectedLanguages);
        ConfigHelper.UpdateTranslateLanguages(_choiceLanguages);
    }
}