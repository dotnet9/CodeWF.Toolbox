using Unit = System.Reactive.Unit;

namespace CodeWF.Tools.Modules.Web.ViewModels;

public class SlugifyViewModel : ViewModelBase
{
    private readonly IClipboardService? _clipboardService;
    private readonly IEventAggregator _eventAggregator;
    private readonly INotificationService? _notificationService;
    private readonly ITranslationService? _translationService;

    private string? _from;

    private bool _isAutoTranslation = true;
    private TranslationKind _kind = TranslationKind.ChineseToSlug;

    private string? _to;

    public SlugifyViewModel(INotificationService notificationService, IClipboardService clipboardService,
        ITranslationService translationService,
        IEventAggregator eventAggregator)
    {
        _notificationService = notificationService;
        _clipboardService = clipboardService;
        _translationService = translationService;
        _eventAggregator = eventAggregator;
        KindChanged = ReactiveCommand.Create<TranslationKind>(OnKindChanged);

        RegisterPrismEvent();
    }

    /// <summary>
    ///     中文标题
    /// </summary>
    public TranslationKind Kind
    {
        get => _kind;
        set => this.RaiseAndSetIfChanged(ref _kind, value);
    }

    /// <summary>
    ///     待翻译字符串
    /// </summary>
    public string? From
    {
        get => _from;
        set
        {
            this.RaiseAndSetIfChanged(ref _from, value);
            if (_isAutoTranslation)
            {
                HandleTranslationAsync().WaitAsync(TimeSpan.FromSeconds(3));
            }
        }
    }

    /// <summary>
    ///     目标翻译字符串
    /// </summary>
    public string? To
    {
        get => _to;
        set => this.RaiseAndSetIfChanged(ref _to, value);
    }

    /// <summary>
    ///     自动翻译
    /// </summary>
    public bool IsAutoTranslation
    {
        get => _isAutoTranslation;
        set => this.RaiseAndSetIfChanged(ref _isAutoTranslation, value);
    }

    public ReactiveCommand<TranslationKind, Unit> KindChanged { get; }

    private void RegisterPrismEvent()
    {
        _eventAggregator.GetEvent<TestEvent>().Subscribe(args =>
        {
            _notificationService?.Show("Prism Event",
                $"模块【SlugifyString】Prism事件处理程序：Args = {args.Args}, Now = {DateTime.Now}");
        });
    }

    public async Task HandleTranslationAsync()
    {
        if (string.IsNullOrWhiteSpace(From))
        {
            To = string.Empty;
            return;
        }

        try
        {
            switch (Kind)
            {
                case TranslationKind.ChineseToEnglish:
                    To = await _translationService!.ChineseToEnglishAsync(From);
                    break;
                case TranslationKind.EnglishToChinese:
                    To = await _translationService!.EnglishToChineseAsync(From);
                    break;
                case TranslationKind.ChineseToSlug:
                    string english = await _translationService!.ChineseToEnglishAsync(From);
                    To = _translationService!.EnglishToUrlSlug(english);
                    break;
                default:
                    To = _translationService!.EnglishToUrlSlug(From);
                    break;
            }
        }
        catch (Exception ex)
        {
            To = ex.Message;
        }
    }

    public Task ExecuteCopyAsync()
    {
        if (!string.IsNullOrWhiteSpace(To))
        {
            _clipboardService?.CopyToAsync(To);
            _notificationService?.Show("成功", "已复制");
        }
        else
        {
            _notificationService?.Show("没有可以复制内容", "请先生成后再复制");
        }

        return Task.CompletedTask;
    }


    private void OnKindChanged(TranslationKind newKind)
    {
        Kind = newKind;
    }
}