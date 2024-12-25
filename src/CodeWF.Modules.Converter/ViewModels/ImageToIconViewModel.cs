using Avalonia.Dialogs.Internal;
using Avalonia.Platform.Storage;
using AvaloniaXmlTranslator;
using CodeWF.Core.IServices;
using CodeWF.Modules.Converter.Models;
using CodeWF.Tools.Extensions;
using CodeWF.Tools.FileExtensions;
using CodeWF.Tools.Helpers;
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
        (bool isSuccess, uint[]? sizes, string? iconPath) = await GetGenerateInfo();
        if (!isSuccess)
        {
            return;
        }

        try
        {
            await ImageHelper.MergeGenerateIcon(NeedConvertImagePath, iconPath, sizes);
        }
        catch (Exception ex)
        {
            await MessageBox.ShowOverlayAsync(ex.Message);
        }

        FileHelper.OpenFolderAndSelectFile(iconPath);
    }

    public async Task RaiseSeparateGenerateIconHandler()
    {
        (bool isSuccess, uint[]? sizes, string? iconPath) = await GetGenerateInfo();
        if (!isSuccess)
        {
            return;
        }

        var folder = Path.GetDirectoryName(iconPath);
        try
        {
            await ImageHelper.SeparateGenerateIcon(NeedConvertImagePath, iconPath, sizes);
        }
        catch (Exception ex)
        {
            await MessageBox.ShowOverlayAsync(ex.Message);
        }

        FileHelper.OpenFolder(folder);
    }

    private async Task<(bool IsSuccess, uint[]? Sizes, string? DestIconPath)> GetGenerateInfo()
    {
        if (string.IsNullOrWhiteSpace(NeedConvertImagePath)
            || !File.Exists(NeedConvertImagePath))
        {
            await MessageBox.ShowOverlayAsync(
                I18nManager.Instance.GetResource(Localization.ImageToIconView.ChoiceSourceImageDialogTitle)!);
            return (false, null, null);
        }

        var selectedSize = IconSizes.Where(item => item.IsSelected).ToList();
        if (selectedSize.Count <= 0)
        {
            await MessageBox.ShowOverlayAsync(
                I18nManager.Instance.GetResource(Localization.ImageToIconView.DestImageSize)!);
            return (false, null, null);
        }

        var folder = Path.GetDirectoryName(NeedConvertImagePath);
        var fileName = Path.GetFileNameWithoutExtension(NeedConvertImagePath);
        var savePath = Path.Combine(folder, $"{fileName}.ico");
        var destSizes = selectedSize.Select(size => (uint)(size.Size)).ToArray();

        return (true, destSizes, savePath);
    }

    #endregion
}