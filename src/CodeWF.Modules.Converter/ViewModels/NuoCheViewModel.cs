using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using AvaloniaXmlTranslator;
using CodeWF.Core.IServices;
using CodeWF.LogViewer.Avalonia;
using CodeWF.Tools.Image;
using HashidsNet;
using ReactiveUI;

namespace CodeWF.Modules.Converter.ViewModels;

public class NuoCheViewModel : ReactiveObject
{
    private readonly INotificationService _notificationService;
    private readonly IFileChooserService _fileChooserService;
    private string _inputTitle;
    private long _phoneNumber;
    private Bitmap? _qrCodeImage;
    private string _qrCodeImagePath;
    private string _generatedUrl;

    public NuoCheViewModel(INotificationService notificationService, IFileChooserService fileChooserService)
    {
        _notificationService = notificationService;
        _fileChooserService = fileChooserService;

        InputTitle = I18nManager.Instance.GetResource(Localization.NuoCheView.DefaultInputTitle);
        PhoneNumber = 16899999999;
    }

    public string InputTitle
    {
        get => _inputTitle;
        set => this.RaiseAndSetIfChanged(ref _inputTitle, value);
    }

    public long PhoneNumber
    {
        get => _phoneNumber;
        set => this.RaiseAndSetIfChanged(ref _phoneNumber, value);
    }

    public Bitmap? QrCodeImage
    {
        get => _qrCodeImage;
        private set => this.RaiseAndSetIfChanged(ref _qrCodeImage, value);
    }

    public string QrCodeImagePath
    {
        get => _qrCodeImagePath;
        private set => this.RaiseAndSetIfChanged(ref _qrCodeImagePath, value);
    }

    public string GeneratedUrl
    {
        get => _generatedUrl;
        private set => this.RaiseAndSetIfChanged(ref _generatedUrl, value);
    }

    public void GenerateQrCode()
    {
        if (string.IsNullOrWhiteSpace(InputTitle))
        {
            _notificationService.Show(I18nManager.Instance.GetResource(Localization.NuoCheView.Title),
                I18nManager.Instance.GetResource(Localization.NuoCheView.NeedInputTip));
            return;
        }

        try
        {
            var encodedPhone = new Hashids("codewf").EncodeLong(PhoneNumber);

            GeneratedUrl =
                $"https://codewf.com/nuoche?p={encodedPhone}";
            QrCodeImagePath = Path.Combine(Path.GetTempPath(), "nuoche.png");

            QrCodeGenerator.GenerateQrCode(InputTitle, GeneratedUrl, QrCodeImagePath);

            QrCodeImage = new Bitmap(QrCodeImagePath);
        }
        catch (Exception ex)
        {
            Logger.Error(I18nManager.Instance.GetResource(Localization.NuoCheView.CreateErrorMessage), ex);
            _notificationService.Show(I18nManager.Instance.GetResource(Localization.NuoCheView.Title),
                $"{I18nManager.Instance.GetResource(Localization.NuoCheView.CreateErrorMessage)}: {ex}");
        }
    }


    public async Task SaveQrCode()
    {
        if (QrCodeImage == null || string.IsNullOrEmpty(QrCodeImagePath))
        {
            _notificationService.Show(I18nManager.Instance.GetResource(Localization.NuoCheView.SaveNotificationTitle),
                I18nManager.Instance.GetResource(Localization.NuoCheView.SaveNoQrCodeMessage));
            return;
        }

        try
        {
            var file = await _fileChooserService.SaveFileAsync(
                I18nManager.Instance.GetResource(Localization.NuoCheView.SaveQrCodeFileTitle),
                new[]
                {
                    new FilePickerFileType(
                        I18nManager.Instance.GetResource(Localization.NuoCheView.SaveQrCodeFileFormat))
                    {
                        Patterns = new[] { "*.png" }
                    }
                }
            );

            if (file != null)
            {
                File.Copy(QrCodeImagePath, file, true);
                _notificationService.Show(
                    I18nManager.Instance.GetResource(Localization.NuoCheView.SaveQrCodeSuccessTitle),
                    I18nManager.Instance.GetResource(Localization.NuoCheView.SaveQrCodeSuccessMessage));
            }
        }
        catch (Exception ex)
        {
            _notificationService.Show(I18nManager.Instance.GetResource(Localization.NuoCheView.SaveNotificationTitle),
                $"{I18nManager.Instance.GetResource(Localization.NuoCheView.SaveQrCodeErrorMessage)}: {ex.Message}");
        }
    }

    public async Task RaisePointerPressed(PointerPressedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(QrCodeImagePath) || !File.Exists(QrCodeImagePath))
        {
            return;
        }

        var dragData = new DataObject();
        if (await _fileChooserService.StorageProvider.TryGetFileFromPathAsync(QrCodeImagePath) is { } storageFile)
        {
            dragData.Set(DataFormats.Files, new[] { storageFile });
        }

        await DragDrop.DoDragDrop(e, dragData, DragDropEffects.Copy);
    }
}