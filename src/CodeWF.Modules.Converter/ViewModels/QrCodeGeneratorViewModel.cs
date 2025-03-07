using Avalonia.Media.Imaging;
using AvaloniaXmlTranslator;
using CodeWF.Core.IServices;
using CodeWF.LogViewer.Avalonia;
using CodeWF.Tools.Image;
using ReactiveUI;

namespace CodeWF.Modules.Converter.ViewModels;

public class QrCodeGeneratorViewModel : ReactiveObject
{
    private readonly INotificationService _notificationService;
    private string _inputTitle;
    private string _inputAd;
    private string _phoneNumber;
    private string _webTitle;
    private string _webDescription;
    private string _webButtonContent;
    private Bitmap _qrCodeImage;

    public QrCodeGeneratorViewModel(INotificationService notificationService)
    {
        _notificationService = notificationService;
        InputTitle = I18nManager.Instance.GetResource(Localization.QrCodeGeneratorView.DefaultInputTitle);
        InputAd = I18nManager.Instance.GetResource(Localization.QrCodeGeneratorView.DefaultInputAd);
        PhoneNumber = "16899999999";
        WebTitle = I18nManager.Instance.GetResource(Localization.QrCodeGeneratorView.DefaultWebTitle);
        WebDescription = I18nManager.Instance.GetResource(Localization.QrCodeGeneratorView.DefaultWebDescription);
        WebButtonContent = I18nManager.Instance.GetResource(Localization.QrCodeGeneratorView.DefaultWebButtonContent);
    }

    public string InputTitle
    {
        get => _inputTitle;
        set => this.RaiseAndSetIfChanged(ref _inputTitle, value);
    }

    public string InputAd
    {
        get => _inputAd;
        set => this.RaiseAndSetIfChanged(ref _inputAd, value);
    }

    public string PhoneNumber
    {
        get => _phoneNumber;
        set => this.RaiseAndSetIfChanged(ref _phoneNumber, value);
    }

    public string WebTitle
    {
        get => _webTitle;
        set => this.RaiseAndSetIfChanged(ref _webTitle, value);
    }

    public string WebDescription
    {
        get => _webDescription;
        set => this.RaiseAndSetIfChanged(ref _webDescription, value);
    }

    public string WebButtonContent
    {
        get => _webButtonContent;
        set => this.RaiseAndSetIfChanged(ref _webButtonContent, value);
    }

    public Bitmap QrCodeImage
    {
        get => _qrCodeImage;
        private set => this.RaiseAndSetIfChanged(ref _qrCodeImage, value);
    }

    public void GenerateQrCode()
    {
        if (string.IsNullOrWhiteSpace(InputTitle) 
            || string.IsNullOrWhiteSpace(InputAd) 
            || string.IsNullOrWhiteSpace(PhoneNumber)
            || string.IsNullOrWhiteSpace(WebTitle)
            || string.IsNullOrWhiteSpace(WebDescription)
            || string.IsNullOrWhiteSpace(PhoneNumber))
        {
            _notificationService.Show(I18nManager.Instance.GetResource(Localization.QrCodeGeneratorView.Title), I18nManager.Instance.GetResource(Localization.QrCodeGeneratorView.NeedInputTip));
            return;
        }

        try
        {
            var content = $"https://codewf.com?title={WebTitle}&description={WebDescription}&button={WebButtonContent}&phone={PhoneNumber}";
            var imagePath = Path.Combine(Path.GetTempPath(), "qrcode.png");

            QrCodeGenerator.GenerateQrCode(InputTitle, InputAd, content, imagePath);

            QrCodeImage = new Bitmap(imagePath);
        }
        catch (Exception ex)
        {
            Logger.Error(I18nManager.Instance.GetResource(Localization.QrCodeGeneratorView.CreateErrorMessage), ex);
            _notificationService.Show(I18nManager.Instance.GetResource(Localization.QrCodeGeneratorView.Title), $"{I18nManager.Instance.GetResource(Localization.QrCodeGeneratorView.CreateErrorMessage)}: {ex}");
        }
    }
}