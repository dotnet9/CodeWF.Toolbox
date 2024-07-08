using Avalonia.Platform.Storage;
using CodeWF.Tools.Extensions;
using CodeWF.Tools.Module.Image.Models;
using CodeWF.Utils;
using System.Reactive.Linq;

namespace CodeWF.Tools.Module.Image.ViewModels;

public class GifToImagesViewModel : ViewModelBase
{
    private readonly IFileChooserService _fileChooserService;
    private readonly INotificationService _notificationService;
    private string? _isCalcTimestamp;

    public string? SourceImagePath
    {
        get => _isCalcTimestamp;
        set => this.RaiseAndSetIfChanged(ref _isCalcTimestamp, value);
    }

    public List<EnumValue> Sizes { get; } = Enum.GetValues(typeof(IconSizeKind))
        .Cast<IconSizeKind>()
        .Select(value =>
            new EnumValue { Name = ((int)value).ToString(), Description = value.GetDescription(), IsSelected = true })
        .ToList();

    private bool _canExport;

    public bool CanExport
    {
        get => _canExport;
        set => this.RaiseAndSetIfChanged(ref _canExport, value);
    }

    public GifToImagesViewModel(IFileChooserService fileChooserService, INotificationService notificationService)
    {
        _fileChooserService = fileChooserService;
        _notificationService = notificationService;

        this.WhenAnyValue(x => x.SourceImagePath)
            .Subscribe(sourceImagePath =>
                CanExport = sourceImagePath.IsNullOrEmpty() == false && File.Exists(sourceImagePath));
    }

    public async Task ExecuteOpenSourceImageHandle()
    {
        var openFiles = await _fileChooserService.OpenFileAsync("选择图片",
            new List<FilePickerFileType>() { FilePickerFileTypes.ImageAll }, false);
        if (openFiles?.Any() != true)
        {
            return;
        }

        SourceImagePath = openFiles[0];
    }

    public async Task ExecuteExportMergeIconHandle()
    {
        if (GetSelectedSize(out var size))
        {
            await ImageHelper.ToIconAsync(SourceImagePath, ExportIconKind.Merge, null, size);
        }
    }

    public async Task ExecuteExportMultipleIconHandle()
    {
        if (GetSelectedSize(out var size))
        {
            await ImageHelper.ToIconAsync(SourceImagePath, ExportIconKind.Separate, null, size);
        }
    }

    private bool GetSelectedSize(out IconSizeKind size)
    {
        size = 0;
        var selectedSize = Sizes.Where(s => s.IsSelected).ToList();
        if (selectedSize.Any() != true)
        {
            _notificationService.Show("未选择转换大小", "请选择转换后的图标大小");
            return false;
        }

        for (var i = 0; i < selectedSize.Count(); i++)
        {
            size |= (IconSizeKind)Enum.Parse(typeof(IconSizeKind), selectedSize[i].Name!);
        }

        return true;
    }
}