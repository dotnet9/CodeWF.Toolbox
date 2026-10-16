using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
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
VerifyTitleBar();
VerifyControlCulture(window, ["zh-CN", "zh-Hant", "en-US", "ja-JP"], culture =>
    window.FindControl<ComboBox>("TitleBarLanguageSelector")!.SelectedItem = settings.Languages.Single(item => item.CultureName == culture));
// About is a settings page; verify caption capabilities on its real application shell.
VerifyCaptionCapabilities(window);
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
foreach (var culture in new[] { "en-US", "zh-Hant", "ja-JP", "zh-CN" })
{
    SetBackgroundCulture(culture);
    Check(firstDocument.Title == ToolWorkspace.Translate(base64.Name) && secondDocument.Title == ToolWorkspace.Translate(secondItem.Name) &&
          firstView.FindControl<TextBlock>("ToolName")!.Text == ToolWorkspace.Translate(base64.Name),
        "Background culture updates active and inactive real document titles: " + culture);
    Check(first.Fields.Single(field => field.Id == "text").Text == "Independent 中文 😀" && second.Fields[0].Text == "case value",
        "Background culture preserves both document drafts: " + culture);
}
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
foreach (var (culture, expected) in new[] { ("zh-CN", "属性"), ("zh-Hant", "屬性"), ("en-US", "Properties"), ("ja-JP", "プロパティ") })
{
    settings.SelectedLanguage = settings.Languages.Single(language => language.CultureName == culture); Pump();
    Check(firstView.GetVisualDescendants().OfType<TextBlock>().Any(text => text.Text == expected),
        "Real document properties heading follows the selected language: " + culture);
}
Check(I18nManager.Instance.GetResource("Localization.NuoCheView.InputTitle") == "車移動の案内文（短めに）",
    "Japanese parking QR input label resolves from the real converter resource");
settings.SelectedLanguage = settings.Languages.Single(language => language.CultureName == "en-US"); Pump();
// 工具导航位于 Dock 外；检查用户已拖动的文档属性宽度，而非重置默认值。
var adjustedPropertyWidth = grid.ColumnDefinitions[2].Width;
var adjustedPropertyPixels = grid.ColumnDefinitions[2].ActualWidth;
var ribbon = window.GetVisualDescendants().OfType<MainMenuView>().Single();
var ribbonBounds = ribbon.Bounds;
workspace.SplitActive(); Pump();
Check(firstDocument.Owner != secondDocument.Owner, "Real Dock splits tools into independent panes");
Check(!((Dock.Model.Core.IDock)secondDocument.Owner!).VisibleDockables!.Contains(firstDocument),
    "Splitting removes the original tab instead of duplicating the document");
VerifyDocumentTransfer("split");
Capture(window, "workspace-split.png");
workspace.Factory.FloatDockable(firstDocument);
WaitForDock(() => TopLevel.GetTopLevel(firstView) is Window floating && !ReferenceEquals(floating, window),
    "Real Dock floats the same document into a headless host window");
VerifyDocumentTransfer("float");
Capture((Window)TopLevel.GetTopLevel(firstView)!, "workspace-floating.png");
workspace.Factory.MoveDockable((Dock.Model.Core.IDock)firstDocument.Owner!,
    (Dock.Model.Core.IDock)secondDocument.Owner!, firstDocument, null);
WaitForDock(() => ReferenceEquals(TopLevel.GetTopLevel(firstView), window),
    "Real Dock re-docks the same document into the application shell");
VerifyDocumentTransfer("re-dock");
Check(((Dock.Model.Core.IDock)firstDocument.Owner!).VisibleDockables!.Count(document => ReferenceEquals(document, firstDocument)) == 1 &&
      workspace.OpenDocuments.Count == 2, "Re-docking retains exactly one tab and workspace entry for each tool");
Capture(window, "workspace-redocked.png");
workspace.Open(base64); workspace.CloseActive(); Pump();
Check(workspace.OpenDocuments.Count == 1 && !workspace.OpenDocuments.Contains(firstDocument), "Close active works after splitting, floating and re-docking a document");
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
    SetBackgroundCulture("en-US"); SetBackgroundCulture("zh-CN");
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
VerifyPrimaryButtonStates();
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
foreach (var culture in new[] { "en-US", "zh-Hant", "ja-JP", "zh-CN" })
{
    SetBackgroundCulture(culture);
    CheckLocalized(about);
    Check(settings.SelectedLanguage?.CultureName == culture, "Background culture follows the live About and document views: " + culture);
}
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
VerifyCultureSubscriptionLifetime();
about.Close(); workspace.CloseAll(); window.Hide();
File.WriteAllLines(Path.Combine(output, "assertions.txt"), assertions);
Console.WriteLine($"Toolbox verification passed: {assertions.Count} assertions; NativeAOT={!RuntimeFeature.IsDynamicCodeSupported}; {output}");

void Check(bool condition, string description)
{
    if (!condition) throw new InvalidOperationException(description);
    assertions.Add(description); Console.WriteLine("PASS " + description);
}
void VerifyPrimaryButtonStates()
{
    // 读取真实模板和文字的最终颜色，不能只验证 Button 自身的属性值。
    var login = new LoginWindow(container.Resolve<IApplicationService>(), container.Resolve<ILoginService>());
    var exit = new ExitOptionWindow { DataContext = new ExitOptionViewModel() };
    login.Show(); exit.Show(); Pump();
    var currentToolView = (ToolDocumentView)workspace.OpenDocuments.Single().Context!;
    var candidates = new[]
    {
        (Host: (Window)window, Button: currentToolView.GetVisualDescendants().OfType<Button>().First(button => button.Classes.Contains("AppPrimary")), Name: "tool"),
        (Host: (Window)login, Button: login.GetVisualDescendants().OfType<Button>().Single(button => button.Classes.Contains("AppPrimary")), Name: "login"),
        (Host: (Window)exit, Button: exit.GetVisualDescendants().OfType<Button>().Single(button => button.Classes.Contains("ExitConfirm")), Name: "exit")
    };
    var originalCulture = I18nManager.Instance.Culture.Name;
    foreach (var culture in new[] { "zh-CN", "zh-Hant", "en-US", "ja-JP" })
    {
        SetBackgroundCulture(culture);
        exit.UpdateLayout(); Pump();
        var remember = exit.GetVisualDescendants().OfType<CheckBox>().Single(control => control.Classes.Contains("ExitRemember"));
        var text = remember.GetVisualDescendants().OfType<TextBlock>().Single(control => control.IsEffectivelyVisible);
        var confirm = candidates.Single(candidate => candidate.Name == "exit").Button;
        var rememberBottom = remember.TranslatePoint(new Point(0, remember.Bounds.Height), exit)!.Value.Y;
        var confirmTop = confirm.TranslatePoint(default, exit)!.Value.Y;
        Check(text.Text == I18nManager.Instance.GetResource(Localization.ExitOptionView.RememberMyChoice) && text.Bounds.Width + 1 >= text.DesiredSize.Width,
            "Exit remember label remains complete: " + culture);
        Check(rememberBottom + 4 <= confirmTop, "Exit actions keep a separate row below the remember choice: " + culture);
    }
    SetBackgroundCulture(originalCulture);
    var lines = new List<string> { "palette\tmode\tcontrol\tstate\tforeground\tbackground\tcontrast" };
    try
    {
        foreach (var palette in settings.Themes)
        foreach (var mode in settings.ThemeModes.Where(item => item.Key != "System"))
        {
            settings.SelectedTheme = palette; settings.SelectedMode = mode; Pump();
            foreach (var (host, button, name) in candidates)
            {
                button.IsEnabled = true;
                var outside = new Point(-100, -100);
                var point = button.TranslatePoint(new Point(button.Bounds.Width / 2, button.Bounds.Height / 2), host)!.Value;
                host.MouseMove(outside); Pump(); Measure("normal", "StudioPrimary");
                button.Focus(NavigationMethod.Tab); Pump(); Measure("focus", "StudioPrimary");
                host.MouseMove(point); Pump();
                Check(button.IsPointerOver, name + " receives real pointer hover: " + palette.Key + "/" + mode.Key);
                Measure("hover", "StudioPrimaryHover");
                host.MouseDown(point, MouseButton.Left); Pump();
                Check(button.IsPressed, name + " receives real pointer press: " + palette.Key + "/" + mode.Key);
                Measure("pressed", "StudioPrimaryPressed");
                host.MouseMove(outside); host.MouseUp(outside, MouseButton.Left); Pump();
                button.IsEnabled = false; Pump(); Measure("disabled", null);
                button.IsEnabled = true; Pump();

                void Measure(string state, string? expectedBackground)
                {
                    var presenter = button.GetVisualDescendants().OfType<Avalonia.Controls.Presenters.ContentPresenter>()
                        .Single(control => control.Name == "PART_ContentPresenter");
                    var text = presenter.GetVisualDescendants().OfType<TextBlock>().First(control => control.IsEffectivelyVisible);
                    var background = ((ISolidColorBrush)presenter.Background!).Color;
                    var foreground = ((ISolidColorBrush)text.Foreground!).Color;
                    var ratio = Contrast(foreground, background);
                    lines.Add($"{palette.Key}\t{mode.Key}\t{name}\t{state}\t{foreground}\t{background}\t{ratio.ToString("F4", CultureInfo.InvariantCulture)}");
                    if (expectedBackground is not null)
                    {
                        Check(host.TryFindResource(expectedBackground, host.ActualThemeVariant, out var expected) && expected is ISolidColorBrush brush && brush.Color == background,
                            name + " template follows " + expectedBackground + ": " + palette.Key + "/" + mode.Key + "/" + state + ", actual=" + background + ", expected=" + expected);
                        Check(ratio >= 4.5, name + " actual text contrast >= 4.5: " + palette.Key + "/" + mode.Key + "/" + state + " = " + ratio.ToString("F4", CultureInfo.InvariantCulture));
                    }
                    else Check(!button.IsEnabled && !button.IsEffectivelyEnabled, name + " disabled state blocks interaction: " + palette.Key + "/" + mode.Key);
                }
            }
            Capture(login, "login-" + palette.Key + "-" + mode.Key + ".png");
            Capture(exit, "exit-" + palette.Key + "-" + mode.Key + ".png");
        }
    }
    finally
    {
        // 登录窗口的关闭属于业务流程，验证只隐藏窗口，不执行退出或登录。
        login.Hide(); exit.Close();
        File.WriteAllLines(Path.Combine(output, "primary-button-contrast.tsv"), lines);
    }

    static double Contrast(Color foreground, Color background)
    {
        static double Luminance(Color color)
        {
            static double Channel(byte value) { var normalized = value / 255d; return normalized <= 0.04045 ? normalized / 12.92 : Math.Pow((normalized + 0.055) / 1.055, 2.4); }
            return 0.2126 * Channel(color.R) + 0.7152 * Channel(color.G) + 0.0722 * Channel(color.B);
        }
        var first = Luminance(foreground); var second = Luminance(background);
        return (Math.Max(first, second) + 0.05) / (Math.Min(first, second) + 0.05);
    }
}
void VerifyTitleBar()
{
    var toggle = window.FindControl<Ursa.Controls.ThemeToggleButton>("TitleBarThemeToggle")!;
    var palettes = window.FindControl<ListBox>("TitleBarPalettes")!;
    var languages = window.FindControl<ComboBox>("TitleBarLanguageSelector")!;
    var button = toggle.GetVisualDescendants().OfType<Button>().Single(item => item.Name == "PART_ThemeButton");
    var application = container.Resolve<IApplicationService>();
    Check(toggle.IsThreeState && palettes.Items.Count == 8, "Title bar has one Ursa three-state control and eight flat palette choices");
    foreach (var palette in settings.Themes)
    {
        palettes.SelectedItem = palette; settings.ThemeMode = StudioThemeMode.System;
        foreach (var expected in new[] { StudioThemeMode.Light, StudioThemeMode.Dark, StudioThemeMode.System })
        {
            Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
            button.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
            Check(settings.ThemeMode == expected && CodeWF.Toolkit.Files.Helpers.AppConfigHelper.TryGet<string>("StudioMode", out var persistedMode) && persistedMode == expected.ToString() && application.GetTheme() == palette.Key,
                "Actual Ursa click persists mode and preserves palette: " + palette.Key + " " + expected);
            var dark = expected == StudioThemeMode.Dark || expected == StudioThemeMode.System && Application.Current!.PlatformSettings?.GetColorValues().ThemeVariant == Avalonia.Platform.PlatformThemeVariant.Dark;
            Check(Application.Current!.RequestedThemeVariant == StudioTheme.GetVariant(Enum.Parse<StudioPalette>(palette.Key), dark),
                "Ursa scope preserves custom application variant: " + palette.Key + " " + expected);
        }
    }
    foreach (var width in new[] { 1120, 1280 })
    {
        window.Width = width; Pump(); window.UpdateLayout();
        languages.IsDropDownOpen = true; Pump();
        Check(languages.IsDropDownOpen && languages.Items.Count == settings.Languages.Count, "Language icon opens the complete real list at " + width);
        foreach (var language in settings.Languages)
        {
            languages.SelectedItem = language; Pump();
            Check(I18nManager.Instance.Culture.Name == language.CultureName, "Title language selection activates global culture: " + language.CultureName);
            var scope = window.FindControl<ThemeVariantScope>("TitleBarThemeScope")!;
            Check(new[] { "SYSTEM", "LIGHT", "DARK" }.All(key => ((TextBlock)scope.Resources["STRING_THEME_TOGGLE_" + key]!).Text == I18nManager.Instance.GetResource("Localization.Workspace." + char.ToUpperInvariant(key[0]) + key[1..].ToLowerInvariant())), "Ursa internal mode tooltips follow selected language: " + language.CultureName);
        }
        languages.IsDropDownOpen = false;
        Check(toggle.Bounds.Width == 30 && languages.Bounds.Height == 30 && palettes.GetVisualDescendants().OfType<ListBoxItem>().Count() == 8,
            "All eight color dots keep compact title dimensions at " + width);
        var left = (Control)window.LeftContent!;
        var right = (Control)window.RightContent!;
        var caption = window.GetVisualDescendants().OfType<Button>().Single(item => item.Name == "PART_MinimizeButton");
        Check(right.TranslatePoint(default, window)!.Value.X >= left.TranslatePoint(new Point(left.Bounds.Width, 0), window)!.Value.X &&
              right.TranslatePoint(new Point(right.Bounds.Width, 0), window)!.Value.X <= caption.TranslatePoint(default, window)!.Value.X,
            "Title controls fit between title and caption buttons at " + width);
        Capture(window, "titlebar-" + width + ".png");
    }
    Check(!window.GetVisualDescendants().OfType<ComboBox>().Any(item => ReferenceEquals(item.ItemsSource, settings.Themes)), "Palette dots have no duplicate dropdown");
    settings.SelectedLanguage = settings.Languages.Single(item => item.CultureName == "zh-CN"); settings.SelectedTheme = settings.Themes[0]; settings.ThemeMode = StudioThemeMode.Light;
}
void VerifyCaptionCapabilities(Window target)
{
    Pump();
    var minimize = target.GetVisualDescendants().OfType<Button>().Single(button => button.Name == "PART_MinimizeButton");
    var maximize = target.GetVisualDescendants().OfType<Button>().Single(button => button.Name == "PART_MaximizeRestoreButton");
    var canMinimize = target.CanMinimize;
    var canMaximize = target.CanMaximize;
    var originalState = target.WindowState;
    target.WindowState = WindowState.Normal;
    target.CanMinimize = false; Pump();
    Check(!minimize.IsEnabled, "Actual caption disables minimization when the host forbids it");
    minimize.RaiseEvent(new global::Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
    Check(target.WindowState == WindowState.Normal, "Disabled caption also guards programmatic minimization");
    target.CanMinimize = true; Pump();
    Check(minimize.IsEnabled, "Runtime minimization capability updates the same caption button");
    minimize.RaiseEvent(new global::Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
    Check(target.WindowState == WindowState.Minimized, "Enabled caption minimizes the actual host window");
    target.WindowState = WindowState.Normal;
    target.CanMaximize = false; Pump();
    Check(!maximize.IsEnabled, "Actual caption disables forbidden maximization");
    maximize.RaiseEvent(new global::Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
    Check(target.WindowState == WindowState.Normal, "Disabled caption guards programmatic maximization");
    target.CanMinimize = canMinimize; target.CanMaximize = canMaximize; target.WindowState = originalState; Pump();
    Check(minimize.IsEnabled == canMinimize && maximize.IsEnabled == (target.CanResize && canMaximize), "Caption capabilities restore without recreating the host window");
}
void VerifyDocumentTransfer(string stage)
{
    Pump();
    Check(ReferenceEquals(firstDocument.Context, firstView) && ReferenceEquals(firstView.ToolModel, first) &&
          first.Fields.Single(field => field.Id == "text").Text == "Independent 中文 😀" &&
          first.Outputs[0].Text == Convert.ToBase64String(Encoding.UTF8.GetBytes("Independent 中文 😀")),
        "Document " + stage + " preserves the actual view, model, Unicode input and result");
    Check(grid.ColumnDefinitions[2].Width == adjustedPropertyWidth &&
          Math.Abs(grid.ColumnDefinitions[2].ActualWidth - adjustedPropertyPixels) <= 2,
        "Document " + stage + " preserves the user-adjusted property width");
    Check(ribbon.Bounds == ribbonBounds, "Document " + stage + " preserves the external tool ribbon geometry");
}
void WaitForDock(Func<bool> condition, string message)
{
    var timer = Stopwatch.StartNew();
    while (!condition() && timer.ElapsedMilliseconds < 4000) Pump();
    Check(condition(), message);
}
void VerifyControlCulture(Window owner, string[] cultures, Action<string> selectTitleBar)
{
    var originalCulture = I18nManager.Instance.Culture;
    var editor = new TextBox { Text = "Locale 中文 😀" };
    var host = new Window { Content = editor, Width = 340, Height = 110 };
    host.Show(owner); Pump();
    var menu = editor.ContextFlyout as MenuFlyout ?? throw new InvalidOperationException("Semi editor menu is missing.");
    var semiTheme = Application.Current!.Styles.OfType<Semi.Avalonia.SemiTheme>().Single();
    var ursaTheme = Application.Current.Styles.OfType<Ursa.Themes.Semi.UrsaSemiTheme>().Single();
    foreach (var source in new[] { "title selector", "direct manager", "background manager" })
    foreach (var culture in cultures)
    {
        var previousLanguage = settings.SelectedLanguage;
        if (source == "background manager") SetBackgroundCulture(culture);
        else if (source == "direct manager") I18nManager.Instance.Culture = System.Globalization.CultureInfo.GetCultureInfo(culture);
        else selectTitleBar(culture);
        Pump();
        Check(settings.SelectedLanguage?.CultureName == culture &&
              (window.FindControl<ComboBox>("TitleBarLanguageSelector")!.SelectedItem as LocalizationLanguage)?.CultureName == culture,
            "Actual title language selection follows " + source + ": " + culture);
        if (source != "title selector" && previousLanguage is not null && previousLanguage.CultureName != culture)
        {
            selectTitleBar(previousLanguage.CultureName); Pump();
            Check(I18nManager.Instance.Culture.Name == previousLanguage.CultureName,
                "Previous title language can be selected after a direct manager change: " + previousLanguage.CultureName);
            I18nManager.Instance.Culture = System.Globalization.CultureInfo.GetCultureInfo(culture); Pump();
        }
        ResourceDictionary semiText = culture switch
        {
            "en-US" => new Semi.Avalonia.Locale.en_us(),
            "zh-Hant" => new Semi.Avalonia.Locale.zh_tw(),
            "ja-JP" => new Semi.Avalonia.Locale.ja_jp(),
            _ => new Semi.Avalonia.Locale.zh_cn()
        };
        ResourceDictionary ursaText = culture switch
        {
            "en-US" => new Ursa.Themes.Semi.Locale.en_us(),
            "ja-JP" => new Ursa.Themes.Semi.Locale.en_us(),
            _ => new Ursa.Themes.Semi.Locale.zh_cn()
        };
        menu.ShowAt(editor); Pump();
        Check(((MenuItem)menu.Items[1]!).Header?.ToString() == semiText["STRING_MENU_COPY"]?.ToString() &&
              ((MenuItem)menu.Items[2]!).Header?.ToString() == semiText["STRING_MENU_PASTE"]?.ToString() &&
              editor.Text == "Locale 中文 😀" && semiTheme.Locale?.Name == (culture == "zh-Hant" ? "zh-TW" : culture),
            "Existing Semi editor copy/paste follows " + source + ": " + culture);
        menu.Hide();
        Check(ursaTheme.Resources["STRING_MENU_DIALOG_CANCEL"]?.ToString() == ursaText["STRING_MENU_DIALOG_CANCEL"]?.ToString() &&
              ursaTheme.Locale?.Name == (culture == "zh-Hant" ? "zh-CN" : culture == "ja-JP" ? "en-US" : culture),
            "Actual Ursa locale follows " + source + ": " + culture);
    }
    Application.Current.Resources["STRING_MENU_COPY"] = "宿主复制";
    Application.Current.Resources["STRING_MENU_DIALOG_CANCEL"] = "宿主取消";
    I18nManager.Instance.Culture = System.Globalization.CultureInfo.GetCultureInfo("zh-CN"); Pump();
    menu.ShowAt(editor); Pump();
    Check(((MenuItem)menu.Items[1]!).Header?.ToString() == "宿主复制" &&
          Application.Current.Resources["STRING_MENU_DIALOG_CANCEL"]?.ToString() == "宿主取消",
        "Third-party locale changes preserve host resource overrides");
    menu.Hide();
    Application.Current.Resources.Remove("STRING_MENU_COPY");
    Application.Current.Resources.Remove("STRING_MENU_DIALOG_CANCEL");
    I18nManager.Instance.Culture = originalCulture; Pump();
    host.Close();
}
void VerifyCultureSubscriptionLifetime()
{
    var originalCulture = I18nManager.Instance.Culture;
    var scopedSettings = new TitleBarSettingsViewModel(container.Resolve<IApplicationService>(), container.Resolve<ILoginService>(), container.Resolve<IUserProfileService>());
    var scopedWindow = new VerificationMainWindow(container.Resolve<IApplicationService>(), container.Resolve<IFileChooserService>(), container.Resolve<INotificationService>(), scopedSettings);
    scopedWindow.Show(); Pump();
    I18nManager.Instance.Culture = CultureInfo.GetCultureInfo("zh-CN"); Pump();
    Check(scopedSettings.SelectedLanguage?.CultureName == "zh-CN", "A second real host observes global culture changes");
    scopedWindow.Close(); Pump();
    var changes = 0;
    scopedSettings.PropertyChanged += (_, args) => { if (args.PropertyName == nameof(TitleBarSettingsViewModel.SelectedLanguage)) changes++; };
    I18nManager.Instance.Culture = CultureInfo.GetCultureInfo("en-US"); Pump();
    Check(!scopedWindow.IsVisible && changes == 0 && scopedSettings.SelectedLanguage?.CultureName == "zh-CN",
        "Closed real host unsubscribes from global culture changes");
    I18nManager.Instance.Culture = originalCulture; Pump();
}
void SetBackgroundCulture(string culture)
{
    Task.Run(() => I18nManager.Instance.Culture = CultureInfo.GetCultureInfo(culture)).GetAwaiter().GetResult();
    Pump();
    Check(I18nManager.Instance.Culture.Name == culture, "Background language change completes with live views: " + culture);
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
// 仅跳过产品的托盘/退出交互，仍使用真实 MainWindow 的窗口关闭与 Closed 生命周期。
internal sealed class VerificationMainWindow(IApplicationService application, IFileChooserService files, INotificationService notifications, TitleBarSettingsViewModel settings)
    : MainWindow(application, files, notifications, settings)
{
    protected override void OnClosing(WindowClosingEventArgs args) { }
}
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
