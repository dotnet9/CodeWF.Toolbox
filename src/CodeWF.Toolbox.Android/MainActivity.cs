using Android.App;
using Android.Content.PM;
using Avalonia.Android;

namespace CodeWF.Toolbox.Android;
[Activity(
    Label = "CodeWF.Toolbox.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
}
