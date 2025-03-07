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
    private string _title;
    private string _adText;
    private string _phoneNumber;
    private Bitmap _qrCodeImage;

    public QrCodeGeneratorViewModel(INotificationService notificationService)
    {
        _notificationService = notificationService;
        Title = I18nManager.Instance.GetResource(Localization.QrCodeGeneratorView.DefaultTitle);
        AdText = "Âë·»£ºhttps://codewf.com";
        PhoneNumber = "16899999999";
    }

    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    public string AdText
    {
        get => _adText;
        set => this.RaiseAndSetIfChanged(ref _adText, value);
    }

    public string PhoneNumber
    {
        get => _phoneNumber;
        set => this.RaiseAndSetIfChanged(ref _phoneNumber, value);
    }

    public Bitmap QrCodeImage
    {
        get => _qrCodeImage;
        private set => this.RaiseAndSetIfChanged(ref _qrCodeImage, value);
    }

    public void GenerateQrCode()
    {
        if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(AdText) ||
            string.IsNullOrWhiteSpace(PhoneNumber))
        {
            _notificationService.Show(I18nManager.Instance.GetResource(Localization.QrCodeGeneratorView.Title), I18nManager.Instance.GetResource(Localization.QrCodeGeneratorView.NeedInputTip));
            return;
        }

        try
        {
            var content = $"Title: {Title}, Ad: {AdText}, Phone: {PhoneNumber}";
            var imagePath = Path.Combine(Path.GetTempPath(), "qrcode.png");

            QrCodeGenerator.GenerateQrCode(Title, AdText, content, imagePath);

            QrCodeImage = new Bitmap(imagePath);
        }
        catch (Exception ex)
        {
            Logger.Error(I18nManager.Instance.GetResource(Localization.QrCodeGeneratorView.CreateErrorMessage), ex);
            _notificationService.Show(I18nManager.Instance.GetResource(Localization.QrCodeGeneratorView.Title), $"{I18nManager.Instance.GetResource(Localization.QrCodeGeneratorView.CreateErrorMessage)}: {ex}");
        }
    }
}