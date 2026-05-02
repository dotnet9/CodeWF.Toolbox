using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CodeWF.Core.IServices;
using CodeWF.Log.Core;
using CodeWF.Tools.Image;
using HashidsNet;
using Lang.Avalonia;
using ReactiveUI;

namespace CodeWF.Modules.Converter.ViewModels;

public class NuoCheViewModel : ReactiveObject
{
    private readonly INotificationService _notificationService;
    private readonly IFileChooserService _fileChooserService;

    public NuoCheViewModel(INotificationService notificationService, IFileChooserService fileChooserService)
    {
        _notificationService = notificationService;
        _fileChooserService = fileChooserService;

        InputTitle = I18nManager.Instance.GetResource(Localization.NuoCheView.DefaultInputTitle);
        PhoneNumber = 16899999999;
        SubTitle = $"{I18nManager.Instance.GetResource(Localization.NuoCheView.DefaultSubTitlePrefix)}: {PhoneNumber}";
        EnableSubTitle = false;
    }

    public string InputTitle
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public long PhoneNumber
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public Bitmap? QrCodeImage
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string QrCodeImagePath
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string GeneratedUrl
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string SubTitle
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool EnableSubTitle
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public async Task EnableSubTitleHandlerAsync()
    {
        SubTitle = $"{I18nManager.Instance.GetResource(Localization.NuoCheView.DefaultSubTitlePrefix)}: {PhoneNumber}";
    }

    public async Task GenerateQrCodeAsync()
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

            QrCodeGenerator.GenerateQrCode(InputTitle, GeneratedUrl, QrCodeImagePath,
                EnableSubTitle ? SubTitle : null);

            QrCodeImage = new Bitmap(QrCodeImagePath);
        }
        catch (Exception ex)
        {
            Logger.Error(I18nManager.Instance.GetResource(Localization.NuoCheView.CreateErrorMessage), ex);
            _notificationService.Show(I18nManager.Instance.GetResource(Localization.NuoCheView.Title),
                $"{I18nManager.Instance.GetResource(Localization.NuoCheView.CreateErrorMessage)}: {ex}");
        }
    }

    public async Task SaveQrCodeAsync()
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
                [
                    new FilePickerFileType(
                        I18nManager.Instance.GetResource(Localization.NuoCheView.SaveQrCodeFileFormat))
                    {
                        Patterns = ["*.png"]
                    }
                ]);

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

    public async Task RaisePointerPressedAsync(PointerPressedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(QrCodeImagePath) || !File.Exists(QrCodeImagePath))
        {
            return;
        }

        var dragData = new DataTransfer();
        if (await _fileChooserService.StorageProvider.TryGetFileFromPathAsync(QrCodeImagePath) is { } storageFile)
        {
            dragData.Add(DataTransferItem.CreateFile(storageFile));
        }

        await DragDrop.DoDragDropAsync(e, dragData, DragDropEffects.Copy);
    }
}
