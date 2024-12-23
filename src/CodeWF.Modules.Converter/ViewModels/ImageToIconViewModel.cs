using Avalonia.Platform.Storage;
using AvaloniaXmlTranslator;
using CodeWF.Core.IServices;
using CodeWF.Tools.FileExtensions;
using ImageMagick;
using ReactiveUI;
using Ursa.Controls;

namespace CodeWF.Modules.Converter.ViewModels;

public class ImageToIconViewModel : ReactiveObject
{
    private readonly IFileChooserService _fileChooserService;
    private readonly INotificationService _notificationService;
    private List<uint> _iconSizes = [24];

    private readonly FilePickerFileType _icoFilePickerFileType =
        new("Icon file") { Patterns = ["*.ico"] };

    public ImageToIconViewModel(IFileChooserService fileChooserService, INotificationService notificationService)
    {
        _fileChooserService = fileChooserService;
        _notificationService = notificationService;
    }

    #region Properties

    private string? _needConvertImagePath;

    public string? NeedConvertImagePath
    {
        get => _needConvertImagePath;
        set => this.RaiseAndSetIfChanged(ref _needConvertImagePath, value);
    }

    #endregion

    #region Command's handler

    public async Task RaiseChoiceNeedConvertImageHandler()
    {
        var files = await _fileChooserService.OpenFileAsync(
            I18nManager.Instance.GetResource(Localization.ImageToIconView.ChoiceOtherImageDialogTitle)!,
            true,
            [FilePickerFileTypes.All]);
        if (!(files?.Count > 0))
        {
            return;
        }

        NeedConvertImagePath = files[0];
    }

    public async Task RaiseChoiceIconSizeHandler(uint size)
    {
        if (_iconSizes.Contains(size))
        {
            _iconSizes.Remove(size);
        }
        else
        {
            _iconSizes.Add(size);
        }
    }

    public async Task RaiseCreateIconHandler()
    {
        if (string.IsNullOrWhiteSpace(NeedConvertImagePath)
            || !File.Exists(NeedConvertImagePath))
        {
            await MessageBox.ShowOverlayAsync(
                I18nManager.Instance.GetResource(Localization.ImageToIconView.ChoiceOtherImageDialogTitle)!);
            return;
        }

        var saveFile = await _fileChooserService.SaveFileAsync(
            I18nManager.Instance.GetResource(Localization.ImageToIconView.ChoiceSaveImageDialogTitle)!,
            [_icoFilePickerFileType]);
        if (string.IsNullOrWhiteSpace(saveFile))
        {
            return;
        }

        try
        {
            var baseImage = new MagickImage(NeedConvertImagePath);
            var collection = new MagickImageCollection();
            _iconSizes.Sort();
            foreach (var size in _iconSizes)
            {
                var resizedImage = baseImage.Clone();
                resizedImage.Resize(size, size);
                collection.Add(resizedImage);
            }

            await collection.WriteAsync(saveFile);
        }
        catch (Exception ex)
        {
            await MessageBox.ShowOverlayAsync(ex.Message);
        }

        FileHelper.OpenFolderAndSelectFile(saveFile);
    }

    #endregion
}