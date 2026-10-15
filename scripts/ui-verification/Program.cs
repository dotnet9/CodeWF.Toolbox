using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CodeWF.Avalonia.Controls.Themes;
using CodeWF.Core;
using CodeWF.Core.IServices;
using CodeWF.Core.Models;
using CodeWF.Modules.ToolFramework.Services;
using CodeWF.Modules.ToolFramework.ViewModels;
using CodeWF.Toolkit.Core.UpdateChecking;
using CodeWF.Toolkit.Lang;
using CodeWF.Toolbox;
using CodeWF.Toolbox.Services;
using CodeWF.Toolbox.ViewModels;
using CodeWF.Toolbox.Views;
using Prism.Ioc;
using Prism.Regions;
using ReactiveUI.Avalonia;
using System.Diagnostics;
using System.Globalization;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Text;
using UpdateCheckResult = CodeWF.Toolkit.Core.UpdateChecking.UpdateCheckResult;
using UpdateInfo = CodeWF.Toolkit.Core.UpdateChecking.UpdateInfo;

var output = Path.GetFullPath(args.Last());
Directory.CreateDirectory(output);
var isolated = Path.Combine(output, "isolated-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(isolated);
Environment.SetEnvironmentVariable("TOOLBOX_DATA_DIRECTORY", isolated);
AppContext.SetData("APP_CONFIG_FILE", Path.Combine(isolated, "verification.config"));
var assertions = new List<string>();
AppBuilder.Configure<App>().UseSkia().UseReactiveUI(_ => { })
    .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false }).SetupWithoutStarting();
var container = ContainerLocator.Container;
var workspace = container.Resolve<ToolWorkspace>();
var regions = container.Resolve<IRegionManager>();
var menu = container.Resolve<IToolMenuService>();
var window = App.Instance.MainWindow as MainWindow ?? container.Resolve<MainWindow>();
window.Width = 1280; window.Height = 840; window.Show(); Pump();
var settings = (TitleBarSettingsViewModel)window.DataContext!;
Check(settings.Themes.Count == 8 && settings.ThemeModes.Count == 3, "Eight shared palettes and three independent modes");
var tools = Flatten(menu.MenuItems).Where(item => !string.IsNullOrWhiteSpace(item.ViewName)).ToArray();
Check(tools.Length == 117, "All 117 original navigation entries retained");
var registry = container.Resolve<ToolRegistry>();
var manifest = tools.Select(item =>
{
    var uri = item.ViewName!;
    var id = uri.Contains("?tool=") ? Uri.UnescapeDataString(uri.Split("?tool=")[1]) : uri;
    var spec = uri.Contains("?tool=") ? registry.Create(id) : null;
    if (uri.Contains("?tool=")) Check(spec is not null && spec.RunAsync is not null && spec.Outputs.Count > 0, "Tool remains registered: " + id);
    var group = spec?.Category ?? (id.Contains("Xml") ? "XML" : id.Contains("LogViewer") ? "Logs" : id.Contains("Dashboard") ? "Home" : id.Contains("Prettify") ? "Development" : "Converter");
    var fields = spec?.Fields.Select(field => new PrototypeField(field.Id,
        field.IsMultiLine ? "Multi" : field.IsNumber ? "Number" : field.IsBoolean ? "Bool" : field.IsSelect ? "Select" : field.IsPath ? "File" : "Text", field.Label,
        field.IsNumber ? field.Number.ToString(CultureInfo.InvariantCulture) : field.IsBoolean ? field.Boolean.ToString() : field.IsSelect ? field.SelectedOption?.Value ?? "" : field.Text,
        field.Options.Select(option => new PrototypeOption(option.Label, option.Value)).ToArray())).ToArray()
        ?? [new PrototypeField("input", "Multi", "Localization.Workspace.Properties", "", [])];
    return new PrototypeTool(id, group, item.Name ?? id, item.Description ?? "", fields);
}).ToArray();
File.WriteAllText(Path.Combine(output, "tool-manifest.json"), System.Text.Json.JsonSerializer.Serialize(manifest, PrototypeJsonContext.Default.PrototypeToolArray));
var base64 = tools.First(item => item.ViewName!.Contains("tool=base64-string-converter"));
workspace.CloseAll(); Pump();
workspace.Open(base64); Pump();
var firstDocument = workspace.OpenDocuments.Single();
var recycling = (Avalonia.Controls.Recycling.ControlRecycling)App.Instance.Resources["ToolDocumentRecycling"]!;
Check(recycling.TryGetValue(firstDocument, out _), "Open document uses the shared Dock recycling cache");
var firstView = (ToolDocumentView)firstDocument.Context!;
var first = firstView.ToolModel as ToolViewModel ?? throw new Exception("Missing tool view model.");
window.GetVisualDescendants().OfType<MainMenuView>().Single().GetVisualDescendants().OfType<TabStrip>().Single().SelectedIndex = 1; Pump();
first.Fields.Single(field => field.Id == "text").Text = "Independent 中文 😀";
Complete(first.RunCommand.Execute().ToTask());
Check(first.Outputs[0].Text == Convert.ToBase64String(Encoding.UTF8.GetBytes("Independent 中文 😀")), "Real Base64 operation preserves Unicode");
Check(first.ContentFields.Count == 1 && first.ParameterFields.Count == 2, "Content and editable properties share the actual tool fields");
var secondItem = tools.First(item => item.ViewName!.Contains("tool=case-converter"));
workspace.Open(secondItem); Pump();
var secondDocument = workspace.OpenDocuments.Single(document => document != firstDocument);
var second = (ToolViewModel)((ToolDocumentView)secondDocument.Context!).ToolModel!;
second.Fields.Single(field => field.Id == "text").Text = "case value";
workspace.Open(base64); Pump();
Check(workspace.OpenDocuments.Count == 2 && (firstDocument.Owner as Dock.Model.Core.IDock)?.ActiveDockable == firstDocument, "Reopening activates the existing document");
Check(!ReferenceEquals(first, second) && first.Fields.Single(field => field.Id == "text").Text == "Independent 中文 😀" && second.Fields[0].Text == "case value", "Independent documents preserve input and output when switching");
var splitter = firstView.GetVisualDescendants().OfType<GridSplitter>().First(control => control.IsVisible && control.Bounds.Height > 0);
var location = splitter.TranslatePoint(new Point(splitter.Bounds.Width / 2, splitter.Bounds.Height / 2), window)!.Value;
var grid = (Grid)splitter.Parent!;
var previousWidth = grid.ColumnDefinitions[2].ActualWidth;
window.MouseDown(location, MouseButton.Left); window.MouseMove(location + new Vector(-45, 0)); window.MouseUp(location + new Vector(-45, 0), MouseButton.Left); Pump();
Check(Math.Abs(grid.ColumnDefinitions[2].ActualWidth - previousWidth) > 20, "Real GridSplitter resizes the properties panel");
Capture(window, "workspace-zh-CN.png");
settings.SelectedLanguage = settings.Languages.Single(language => language.CultureName == "en-US"); Pump();
Check(I18nManager.Instance.Culture.Name == "en-US" && firstDocument.Title == "Base64 string encoder/decoder", "Requested English culture remains active and real document title is English");
Check(settings.SelectedLanguage?.CultureName == "en-US", "Visible language selection follows the requested culture");
Check(firstDocument.Title == I18nManager.Instance.GetResource(base64.Name!), "Open document titles follow singleton language changes");
CheckLocalized(window); Capture(window, "workspace-en-US.png");
workspace.SplitActive(); Pump();
Check(firstDocument.Owner != secondDocument.Owner, "Real Dock splits tools into independent panes");
Capture(window, "workspace-split.png");
workspace.Open(base64); workspace.CloseActive(); Pump();
Check(workspace.OpenDocuments.Count == 1 && !workspace.OpenDocuments.Contains(firstDocument), "Close active works after moving a document to a split pane");
Check(!recycling.TryGetValue(firstDocument, out _), "Closed document is removed from the shared recycling cache");
workspace.Open(base64); Pump();
var reopened = workspace.OpenDocuments.Single(document => document.Id == base64.ViewName);
Check(!ReferenceEquals(reopened.Context, firstView), "Reopening a closed tool creates a fresh view and model");
workspace.CloseAll(); Pump();
Check(workspace.OpenDocuments.Count == 0 && !regions.Regions.Any(region => region.Name.StartsWith("Document_")), "Close all releases document regions and subscriptions");
foreach (var item in tools.Where(item => !item.ViewName!.Contains("?tool=")))
{
    workspace.Open(item); Pump();
    var document = workspace.OpenDocuments.Single();
    Check(((ToolDocumentView)document.Context!).ToolModel is not null, "Original custom module navigates: " + item.ViewName);
    workspace.CloseAll(); Pump();
}
workspace.Open(base64); Pump();
foreach (var palette in settings.Themes)
foreach (var mode in settings.ThemeModes.Where(item => item.Key != "System"))
{
    settings.SelectedTheme = palette; settings.SelectedMode = mode; Pump();
    Check(App.Instance.RequestedThemeVariant == StudioTheme.GetVariant(Enum.Parse<StudioPalette>(palette.Key), mode.Key == "Dark"), palette.Key + " applies " + mode.Key);
    CheckLocalized(window); Capture(window, palette.Key + "-" + mode.Key + ".png");
}
settings.SelectedMode = settings.ThemeModes.Single(item => item.Key == "System");
Check(App.Instance.RequestedThemeVariant == StudioTheme.GetVariant(Enum.Parse<StudioPalette>(settings.SelectedTheme!.Key), App.Instance.PlatformSettings?.GetColorValues().ThemeVariant == Avalonia.Platform.PlatformThemeVariant.Dark), "System mode follows operating system");
var reloaded = new TitleBarSettingsViewModel(container.Resolve<IApplicationService>(), container.Resolve<ILoginService>(), container.Resolve<IUserProfileService>());
Check(reloaded.SelectedMode?.Key == "System" && reloaded.SelectedTheme?.Key == settings.SelectedTheme?.Key, "Palette and mode preferences reload independently");
settings.SelectedTheme = settings.Themes.Single(item => item.Key == "DotNet"); settings.SelectedMode = settings.ThemeModes.Single(item => item.Key == "Light");
var fake = new FakeUpdates();
var opened = new List<string>(); var exits = 0;
using var updates = new UpdateViewModel(fake, opened.Add, () => exits++);
using var metadata = new AboutViewModel(fake);
var aboutView = new AboutView { DataContext = metadata };
var about = new Window { Width = 760, Height = 680, Content = aboutView };
about.Show(); Pump(); Capture(about, "about-en-US.png"); CheckLocalized(about);
Check(metadata.Author == "沙漠尽头的狼" && metadata.Version == typeof(App).Assembly.GetName().Version!.ToString(), "About reads author and actual application version");
Check(!string.IsNullOrWhiteSpace(metadata.CompileTime) && metadata.Platform.Contains("Avalonia"), "Published About build and platform metadata resolve");
Complete(updates.CheckAsync()); Check(updates.State == UpdateState.Current, "Current version state");
fake.Result = UpdateCheckResult.Failed("offline"); Complete(updates.CheckAsync()); Check(updates.State == UpdateState.Failed && updates.StatusText.Contains("offline"), "Check failure remains visible");
fake.Result = new(NewUpdate(), true, null); Complete(updates.AdvanceAsync()); Check(updates.State == UpdateState.Available && updates.ReleaseNotes == "Release notes", "Retry checks release and exposes notes");
fake.DownloadFailure = true; Complete(updates.AdvanceAsync()); Check(updates.State == UpdateState.Failed, "Checksum/download failure remains visible");
fake.DownloadFailure = false; Complete(updates.AdvanceAsync()); Check(updates.State == UpdateState.Downloaded, "Download retry preserves release selection");
fake.InstallFailure = true; Complete(updates.AdvanceAsync()); Check(updates.State == UpdateState.Failed && exits == 0, "Install failure keeps the app usable");
fake.InstallFailure = false; Complete(updates.AdvanceAsync()); Check(updates.State == UpdateState.Installed && exits == 1, "Successful install requests shutdown after installer launch");
foreach (var missing in new[] { "asset", "checksum", "both" })
{
    fake.Result = new(NewUpdate() with { AssetUrl = missing is "asset" or "both" ? null : "https://example.test/setup.exe", ChecksumUrl = missing is "checksum" or "both" ? null : "https://example.test/setup.exe.sha256" }, true, null);
    Complete(updates.CheckAsync()); var before = fake.DownloadCount; var links = opened.Count;
    Complete(updates.AdvanceAsync()); Check(fake.DownloadCount == before && opened.Count == links + 1, "Missing " + missing + " opens release page");
}
fake.Delay = true; var pending = updates.CheckAsync();
Check(updates.IsBusy && !updates.UpdateCommand.CanExecute(null), "Busy commands reject duplicate operations");
updates.Cancel(); Complete(pending); Check(updates.State == UpdateState.Canceled, "Cancel returns to usable state");
using var closing = new UpdateViewModel(fake, _ => { }, () => { }); pending = closing.CheckAsync(); closing.Dispose(); Complete(pending);
Check(!closing.UpdateCommand.CanExecute(null), "Disposed update model cancels operation and disables commands");
about.Close(); workspace.CloseAll(); window.Hide();
File.WriteAllLines(Path.Combine(output, "assertions.txt"), assertions);
Console.WriteLine($"Toolbox verification passed: {assertions.Count} assertions; NativeAOT={!RuntimeFeature.IsDynamicCodeSupported}; {output}");

void Check(bool condition, string description)
{
    if (!condition) throw new InvalidOperationException(description);
    assertions.Add(description); Console.WriteLine("PASS " + description);
}
void Pump()
{
    var timer = Stopwatch.StartNew();
    do { Dispatcher.UIThread.RunJobs(); AvaloniaHeadlessPlatform.ForceRenderTimerTick(); Thread.Sleep(8); } while (timer.ElapsedMilliseconds < 125);
}
void Complete(Task task)
{
    var timer = Stopwatch.StartNew();
    while (!task.IsCompleted && timer.ElapsedMilliseconds < 10000) Pump();
    Check(task.IsCompleted, "Async operation completes"); task.GetAwaiter().GetResult(); Pump();
}
void Capture(Window target, string file)
{
    target.UpdateLayout(); Pump();
    using var image = target.CaptureRenderedFrame() ?? throw new InvalidOperationException("No rendered frame: " + file);
    image.Save(Path.Combine(output, file));
}
void CheckLocalized(Window target) => Check(!target.GetVisualDescendants().OfType<TextBlock>().Any(text => text.IsVisible && text.Text?.StartsWith("Localization.", StringComparison.Ordinal) == true), "Visible controls resolve language resources");
IEnumerable<ToolMenuItem> Flatten(IEnumerable<ToolMenuItem> items)
{
    foreach (var item in items) { yield return item; foreach (var child in Flatten(item.Children)) yield return child; }
}
UpdateInfo NewUpdate() => new(new Version(99, 0), "v99.0", "CodeWF.Toolbox", "Release notes", "https://example.test/release", "https://example.test/setup.exe", "setup.exe", "https://example.test/setup.exe.sha256");

internal sealed record PrototypeOption(string Label, string Value);
internal sealed record PrototypeField(string Id, string Kind, string Label, string Default, PrototypeOption[] Options);
internal sealed record PrototypeTool(string Id, string Group, string Title, string Description, PrototypeField[] Fields);
[System.Text.Json.Serialization.JsonSerializable(typeof(PrototypeTool[]))]
[System.Text.Json.Serialization.JsonSourceGenerationOptions(PropertyNamingPolicy = System.Text.Json.Serialization.JsonKnownNamingPolicy.CamelCase, WriteIndented = true)]
internal partial class PrototypeJsonContext : System.Text.Json.Serialization.JsonSerializerContext;

sealed class FakeUpdates : IUpdateService
{
    public UpdateServiceOptions Options { get; } = new() { Repository = "CodeWF.Toolbox", ProductName = "CodeWF.Toolbox" };
    public Version CurrentVersion => new(1, 0);
    public string CurrentVersionText => "1.0";
    public UpdateCheckResult Result { get; set; } = UpdateCheckResult.Latest();
    public bool Delay { get; set; }
    public bool DownloadFailure { get; set; }
    public bool InstallFailure { get; set; }
    public int DownloadCount { get; private set; }
    public async Task<UpdateCheckResult> CheckAsync(Version? current = null, CancellationToken cancellationToken = default)
    {
        if (Delay) await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        return Result;
    }
    public Task<UpdateDownloadResult> DownloadAsync(UpdateInfo update, IProgress<UpdateDownloadProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        DownloadCount++;
        if (DownloadFailure) throw new IOException("Checksum mismatch");
        progress?.Report(new(100, 100));
        return Task.FromResult(new UpdateDownloadResult("verified-package", update.AssetName!, 100));
    }
    public Task<UpdateInstallResult> InstallAsync(UpdateDownloadResult download, CancellationToken cancellationToken = default)
    {
        if (InstallFailure) throw new IOException("Installer failed");
        return Task.FromResult(new UpdateInstallResult(UpdateInstallKind.Launched, "Installer launched") { ShouldExitApplication = true });
    }
}
