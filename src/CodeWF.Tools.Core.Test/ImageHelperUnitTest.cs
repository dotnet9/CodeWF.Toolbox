using CodeWF.Tools.Extensions;

namespace CodeWF.Tools.Core.Test;

[TestClass]
public class ImageHelperUnitTest
{
    [TestMethod]
    public async Task Test_ConvertImageToIconMergeAsync_SUCCESS()
    {
        string sourceImg = "D://logo.png";
        string iconPath = "D://logo.ico";
        Assert.IsTrue(File.Exists(sourceImg), "Source image file does not exist.");
        Assert.IsFalse(File.Exists(iconPath), "Target icon file exist.");

        await ImageHelper.ToIconAsync(sourceImg, ExportIconKind.Merge, iconPath);

        Assert.IsTrue(File.Exists(iconPath), $"Expected icon file exist: {iconPath}");
    }

    [TestMethod]
    public async Task Test_ConvertImageToIconSeparateAsync_SUCCESS()
    {
        string sourceImg = "D://logo.png";
        string iconDirectory = "D://";
        Assert.IsTrue(File.Exists(sourceImg), "Source image file does not exist.");
        await ImageHelper.ToIconAsync(sourceImg, ExportIconKind.Separate, iconDirectory);

        string sourceFileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourceImg);
        IEnumerable<string> expectedIconFiles = Enum.GetValues<IconSizeKind>().Select(size =>
            Path.Combine(iconDirectory,
                $"{sourceFileNameWithoutExtension}_{size.GetDescription()}.ico"));
        foreach (string iconFile in expectedIconFiles)
        {
            Assert.IsTrue(File.Exists(iconFile),
                $"Expected icon file does not exist: {iconFile}");
        }
    }
}