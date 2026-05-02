using CodeWF.Core.Helpers;
using System.Runtime.InteropServices;

namespace CodeWF.Toolbox.Tests.UnitTests;

public class StartWasabiOnSystemStartupTests
{
    [Fact]
    public async Task ModifyStartupOnDifferentSystemsTestAsync()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            await StartupHelper.ModifyStartupSettingAsync(true);

            Assert.True(WindowsStartupHelper.RegistryKeyExists());

            await StartupHelper.ModifyStartupSettingAsync(false);

            Assert.False(WindowsStartupHelper.RegistryKeyExists());
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            await StartupHelper.ModifyStartupSettingAsync(true);

            Assert.True(File.Exists(LinuxStartupHelper.FilePath));
            Assert.Equal(LinuxStartupHelper.ExpectedDesktopFileContent, LinuxStartupHelper.GetFileContent());

            await StartupHelper.ModifyStartupSettingAsync(false);

            Assert.False(File.Exists(LinuxStartupHelper.FilePath));
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // We don't read back the results, because on the CI pipeline, we cannot hit the "Allow" option of the pop-up window,
            // which comes up when a third-party app wants to modify the Login Items.

            await StartupHelper.ModifyStartupSettingAsync(true);

            await StartupHelper.ModifyStartupSettingAsync(false);
        }
    }
}