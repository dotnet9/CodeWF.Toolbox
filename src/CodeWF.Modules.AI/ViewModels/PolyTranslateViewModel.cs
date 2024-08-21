using AvaloniaExtensions.Axaml.Markup;
using CodeWF.AvaloniaControls.Extensions;
using CodeWF.Core;
using CodeWF.Modules.AI.Helpers;
using CodeWF.Modules.AI.I18n;
using CodeWF.Modules.AI.Models;
using CodeWF.Modules.AI.Views;
using CodeWF.WebAPI.ViewModels;
using Prism.Ioc;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using Ursa.Controls;

namespace CodeWF.Modules.AI.ViewModels;

public class PolyTranslateViewModel : ReactiveObject
{
    private readonly IContainerExtension _container;
    private readonly ApiClient _apiClient;
    private List<string> _choiceLanguages;
    private readonly ChatGptOptions _chatGptOptions = new();

    public PolyTranslateViewModel(IContainerExtension container, ApiClient apiClient)
    {
        _container = container;
        _apiClient = apiClient;

        _choiceLanguages = ["Chinese Simplified", "Chinese Traditional", "English (United States)", "Japanese (Japan)"];
        Languages = string.Join(",", _choiceLanguages);

        this.WhenAnyValue(x => x.AskContent)
            .Throttle(TimeSpan.FromSeconds(1))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => RaiseTranslateCommandHandlerAsync());
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

    private async Task RaiseChoiceLanguagesCommandHandlerAsync()
    {
        var option =
            new OverlayDialogOptions()
            {
                Title = I18nManager.GetString(Language.LanguageKey), Buttons = DialogButton.OK
            };

        // 这种方式第一次可以，再一次运行异常
        //await _overlayDialogService.ShowModal(DialogNames.Setting, null, HostIds.Main, option);
        var vm = _container.Resolve<ChoiceLanguagesViewModel>();
        vm.SelectedLanguages = new RangeObservableCollection<string> { _choiceLanguages };
        vm.AllLanguages = new RangeObservableCollection<string> { LanguageList.Languages.Except(_choiceLanguages) };

        // 这种方式是可以的，手工获取视图实例
        await OverlayDialog.ShowModal(_container.Resolve<ChoiceLanguagesView>(), vm, HostIds.Main, option);
        _choiceLanguages = vm.SelectedLanguages.ToList();
        Languages = string.Join(",", vm.SelectedLanguages);
    }
}