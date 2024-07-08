using CodeWF.Tools.Extensions;
using CodeWF.Tools.FileExtensions;
using System.Drawing;

namespace CodeWF.Tools.Core.Helpers;

public static class ImageHelper
{
    public static async Task ToIconAsync(string sourceImagePath,
        ExportIconKind exportKind = ExportIconKind.Merge, string? outputDirectoryOrIconPath = default,
        IconSizeKind sizeKind = IconSizeKind.Size16X16 | IconSizeKind.Size24X24 | IconSizeKind.Size32X32 |
                                IconSizeKind.Size48X48 |
                                IconSizeKind.Size64X64 | IconSizeKind.Size96X96 | IconSizeKind.Size128X128 |
                                IconSizeKind.Size256X256)
    {
        if (!File.Exists(sourceImagePath))
        {
            throw new FileNotFoundException("Source image file not found.", sourceImagePath);
        }

        string sourceFileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourceImagePath);

        var files = new Dictionary<Enum, string>();
        foreach (Enum enumValue in Enum.GetValues(typeof(IconSizeKind)))
        {
            if (!sizeKind.HasFlag(enumValue))
            {
                continue;
            }

            string sizeInfo = enumValue.GetDescription()!;
            int width = int.Parse(sizeInfo.Split('x')[0]);

            var tmp = FileHelper.GetTempFileName(AppInfo.AppInfo.AppName) + ".png";
            await ResizeImageSaveAsAsync(sourceImagePath, tmp, MagickFormat.Png32, width, width);
            files[enumValue] = tmp;
        }

        if (exportKind == ExportIconKind.Merge)
        {
            outputDirectoryOrIconPath ??= Path.Combine(Path.GetDirectoryName(sourceImagePath)!,
                $"{sourceFileNameWithoutExtension}.ico");
            var images = files.Select(file => Image.FromFile(file.Value)).ToList();
            await using (var fs = File.OpenWrite(outputDirectoryOrIconPath)) IconFactory.SavePngsAsIcon(images, fs);
            images.ForEach(i => i.Dispose());
            foreach (var file in files)
            {
                FileHelper.DeleteFileIfExist(file.Value);
            }
        }
        else
        {
            outputDirectoryOrIconPath ??= Path.GetDirectoryName(sourceImagePath)!;
            foreach (var file in files)
            {
                string sizeInfo = file.Key.GetDescription()!;

                string targetIconPath =
                    Path.Combine(outputDirectoryOrIconPath, $"{sourceFileNameWithoutExtension}_{sizeInfo}.ico");

                FileHelper.DeleteFileIfExist(targetIconPath);


                using MagickImage img = new MagickImage(file.Value);
                await img.WriteAsync(targetIconPath, MagickFormat.Ico);

                File.Delete(file.Value);
            }
        }
    }

    public static async Task ResizeImageSaveAsAsync(string sourceImagePath, string targetImagePath,
        MagickFormat imageFormat,
        int width, int height)
    {
        using MagickImage image = new MagickImage(sourceImagePath);
        image.Strip(); // 移除所有配置文件和注释信息，减小文件大小（可选）
        image.Resize(width, height);
        image.BackgroundColor = MagickColors.Transparent;
        await image.WriteAsync(targetImagePath, imageFormat);
    }
}

public enum ExportIconKind
{
    Merge,
    Separate
}

[Flags]
public enum IconSizeKind
{
    [Description("16x16")] Size16X16 = 1 << 0,
    [Description("24x24")] Size24X24 = 1 << 1,
    [Description("32x32")] Size32X32 = 1 << 2,
    [Description("48x48")] Size48X48 = 1 << 3,
    [Description("64x64")] Size64X64 = 1 << 4,
    [Description("96x96")] Size96X96 = 1 << 5,
    [Description("128x128")] Size128X128 = 1 << 6,
    [Description("256x256")] Size256X256 = 1 << 7
}