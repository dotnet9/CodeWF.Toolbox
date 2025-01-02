using Avalonia.Platform.Storage;
using AvaloniaXmlTranslator;
using CodeWF.Core.IServices;
using CodeWF.Modules.Converter.Models;
using CodeWF.Tools;
using CodeWF.Tools.FileExtensions;
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
        (bool isSuccess, uint[]? sizes) = await GetGenerateInfo();
        if (!isSuccess)
        {
            return;
        }

        var folder = Path.GetDirectoryName(NeedConvertImagePath);
        var fileName = Path.GetFileNameWithoutExtension(NeedConvertImagePath);
        var saveIconPath = Path.Combine(folder, $"{fileName}.ico");
        try
        {
            await ImageHelper.MergeGenerateIcon(NeedConvertImagePath, saveIconPath, sizes);
        }
        catch (Exception ex)
        {
            await MessageBox.ShowOverlayAsync(ex.Message);
        }

        FileHelper.OpenFolderAndSelectFile(saveIconPath);
    }

    public async Task RaiseSeparateGenerateIconHandler()
    {
        (bool isSuccess, uint[]? sizes) = await GetGenerateInfo();
        if (!isSuccess)
        {
            return;
        }

        var saveIconFolder = Path.GetDirectoryName(NeedConvertImagePath);
        try
        {
            await ImageHelper.SeparateGenerateIcon(NeedConvertImagePath, saveIconFolder, sizes);
        }
        catch (Exception ex)
        {
            await MessageBox.ShowOverlayAsync(ex.Message);
        }

        FileHelper.OpenFolder(saveIconFolder);
    }

    private async Task<(bool IsSuccess, uint[]? Sizes)> GetGenerateInfo()
    {
        if (string.IsNullOrWhiteSpace(NeedConvertImagePath)
            || !File.Exists(NeedConvertImagePath))
        {
            await MessageBox.ShowOverlayAsync(
                I18nManager.Instance.GetResource(Localization.ImageToIconView.ChoiceSourceImageDialogTitle)!);
            return (false, null);
        }

        var selectedSize = IconSizes.Where(item => item.IsSelected).ToList();
        if (selectedSize.Count <= 0)
        {
            await MessageBox.ShowOverlayAsync(
                I18nManager.Instance.GetResource(Localization.ImageToIconView.DestImageSize)!);
            return (false, null);
        }

        var destSizes = selectedSize.Select(size => (uint)(size.Size)).ToArray();

        return (true, destSizes);
    }

    #endregion
}