using Avalonia.Dialogs.Internal;
using Avalonia.Platform.Storage;
using AvaloniaXmlTranslator;
using CodeWF.Core.IServices;
using CodeWF.Modules.Converter.Models;
using CodeWF.Tools.Extensions;
using CodeWF.Tools.FileExtensions;
using ImageMagick;
using ReactiveUI;
using System.Collections.ObjectModel;
using Ursa.Controls;

namespace CodeWF.Modules.Converter.ViewModels;

public class ImageToIconViewModel : ReactiveObject
{
    private readonly IFileChooserService _fileChooserService;
    private readonly INotificationService _notificationService;

    private readonly FilePickerFileType _icoFilePickerFileType =
        new("Icon file") { Patterns = ["*.ico"] };

    public ImageToIconViewModel(IFileChooserService fileChooserService, INotificationService notificationService)
    {
        _fileChooserService = fileChooserService;
        _notificationService = notificationService;
        IconSizes.AddRange(Enum.GetValues<IconSize>()
            .Select(size => new IconSizeItem(size)));
    }

    #region Properties

    public ObservableCollection<IconSizeItem> IconSizes { get; } = new();

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
            I18nManager.Instance.GetResource(Localization.ImageToIconView.ChoiceSourceImageDescription)!,
            true,
            [FilePickerFileTypes.All]);
        if (!(files?.Count > 0))
        {
            return;
        }

        NeedConvertImagePath = files[0];
    }

    public async Task RaiseMergeGenerateIconHandler()
    {
        var (isSuccess, sizes) = await GetGenerateInfo();
        if (!isSuccess)
        {
            return;
        }

        var folder = Path.GetDirectoryName(NeedConvertImagePath);
        var fileName = Path.GetFileNameWithoutExtension(NeedConvertImagePath);
        var savePath = Path.Combine(folder, $"{fileName}.ico");
        try
        {
            var baseImage = new MagickImage(NeedConvertImagePath);
            var collection = new MagickImageCollection();

            foreach (var size in sizes)
            {
                var resizedImage = baseImage.Clone();
                resizedImage.Resize((uint)size.Size, (uint)size.Size);
                collection.Add(resizedImage);
            }

            await collection.WriteAsync(savePath);
        }
        catch (Exception ex)
        {
            await MessageBox.ShowOverlayAsync(ex.Message);
        }

        FileHelper.OpenFolderAndSelectFile(savePath);
    }

    public async Task RaiseSeparateGenerateIconHandler()
    {
        var (isSuccess, sizes) = await GetGenerateInfo();
        if (!isSuccess)
        {
            return;
        }

        var folder = Path.GetDirectoryName(NeedConvertImagePath);
        var fileName = Path.GetFileNameWithoutExtension(NeedConvertImagePath);
        try
        {
            var baseImage = new MagickImage(NeedConvertImagePath);

            foreach (var size in sizes)
            {
                var resizedImage = baseImage.Clone();
                resizedImage.Resize((uint)size.Size, (uint)size.Size);

                var savePath = Path.Combine(folder, $"{fileName}.{size.Size.GetDescription()}.ico");
                resizedImage.WriteAsync(savePath);
            }
        }
        catch (Exception ex)
        {
            await MessageBox.ShowOverlayAsync(ex.Message);
        }

        FileHelper.OpenFolder(folder);
    }

    private async Task<(bool IsSuccess, List<IconSizeItem>? Sizes)> GetGenerateInfo()
    {
        if (string.IsNullOrWhiteSpace(NeedConvertImagePath)
            || !File.Exists(NeedConvertImagePath))
        {
            await MessageBox.ShowOverlayAsync(
                I18nManager.Instance.GetResource(Localization.ImageToIconView.ChoiceSourceImageDialogTitle)!);
            return (default, default);
        }

        var selectedSize = IconSizes.Where(item => item.IsSelected).ToList();
        if (selectedSize.Count <= 0)
        {
            await MessageBox.ShowOverlayAsync(
                I18nManager.Instance.GetResource(Localization.ImageToIconView.DestImageSize)!);
            return (default, default);
        }

        return (true, selectedSize);
    }

    #endregion
}