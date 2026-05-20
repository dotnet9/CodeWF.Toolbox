# CodeWF Toolbox

English | [简体中文](README.zh-CN.md)

CodeWF Toolbox is an Avalonia + Prism modular desktop toolbox for developer productivity scenarios. It keeps the shell, shared services, and feature modules separated so new tools can be added without turning the main application into one large window class.

![Application screenshot](screen.png)

## Highlights

- Cross-platform desktop UI based on Avalonia UI and Semi/Ursa controls.
- Prism module catalog, dependency injection, and region navigation.
- XML-based internationalization with Simplified Chinese, Traditional Chinese, English, and Japanese resources.
- Tool modules for format converters, log viewing, development utilities, web helpers, security tools, and XML translation management.
- Native AOT-oriented publishing scripts and platform constants.
- Improved menu registration, searchable tool navigation, and safer region navigation.

## Documentation

- [Developer guide](docs/README.md)
- [中文开发文档](docs/README.zh-CN.md)
- [Architecture SVG](docs/assets/architecture.svg)
- [Module lifecycle SVG](docs/assets/module-lifecycle.svg)

![Architecture](docs/assets/architecture.svg)

## Quick Start

Requirements:

- .NET 11 SDK
- Windows, macOS, or Linux desktop runtime supported by Avalonia

```powershell
dotnet restore CodeWF.Toolbox.slnx
dotnet build CodeWF.Toolbox.slnx
dotnet run --project src/CodeWF.Toolbox/CodeWF.Toolbox.csproj
```

## Solution Layout

```text
src/
  CodeWF.Toolbox/                    Desktop app, shell, main views, settings, resources
  CodeWF.Core/                       Shared abstractions, services, regions
  CodeWF.Controls/                   Shared controls
  CodeWF.Modules.Converter/          Converter tools
  CodeWF.Modules.ToolFramework/      Shared local toolbox runtime and tool catalog
  CodeWF.Modules.Development/        Development tools
  CodeWF.Modules.LogViewer/          Large-file log viewer with tail monitoring
  CodeWF.Modules.XmlTranslatorManager/ XML i18n management tools
  CodeWF.Toolbox.Tests/              Unit test project
docs/
  assets/                            Standalone SVG diagrams
```

## Adding a Module

1. Create a module project under `src/CodeWF.Modules.*`.
2. Implement `IModule`.
3. Register menu entries through `IToolMenuService`.
4. Register views with `RegionNames.ContentRegion`.
5. Add the module to `App.ConfigureModuleCatalog`.
6. Add localization XML files and generated language keys.

See the developer guide for the detailed conventions.

## Included Tools

- Log Viewer: opens large log files quickly, renders only the visible range, and follows appended content when tail mode is enabled.
- Format converters: JSON/YAML, Base64, GUID, date-time, and image-to-icon utilities.
- Development helpers: JSON/YAML formatting, shell and data utilities, and small productivity tools.
- XML translation manager: compares, merges, and maintains XML localization resources.

## Third-Party Open Source Audit (2026-05-20)

Checked with `dotnet restore CodeWF.Toolbox.slnx`, `dotnet list package --include-transitive`, NuGet `.nuspec` metadata, NuGet.org, and upstream source repositories. MIT / Apache-2.0 / BSD are preferred; other source-open licenses are marked when source and transitive dependencies are traceable.

Remediation:

- Removed `AvaloniaUI.DiagnosticsSupport` and `Semi.Avalonia.AvaloniaEdit`; AvaloniaEdit now uses `Avalonia.AvaloniaEdit` with the official Fluent styling path.
- Updated `CodeWF.AvaloniaControls.ProDataGrid` / `CodeWF.AvaloniaControls.ProDataGrid.Themes` to `12.0.3.2`, avoiding the `Semi.Avalonia.ProDataGrid` black-box chain.
- Updated internal packages to `CodeWF.AvaloniaControls.Themes 12.0.3.3`, `CodeWF.EventBus 3.4.5.5`, `CodeWF.Log.Core 12.0.3.1`, `CodeWF.Tools* 1.3.13.2`, and `Lang.Avalonia.Json 12.0.3.1`.
- Updated stable open-source packages: `Dapper 2.1.79`, `Dapper.AOT 1.0.52`, `Xaml.Behaviors 12.0.0.1`, and `coverlet.collector 10.0.1`.
- Changed `BouncyCastle.Cryptography` from `2.7.0-beta.98` to stable `2.6.2`.
- Changed `Microsoft.Data.Sqlite` from `11.0.0-preview.4` to stable `10.0.8`.
- Changed `xunit.runner.visualstudio` from `4.0.0-pre.4` to stable `3.1.5`.
- Kept `Prism.Avalonia` / `Prism.DryIoc.Avalonia` on `8.1.97.11073` and `Irihi.Ursa.PrismExtension` on `2.0.0`; Prism 9 is commercial and intentionally not upgraded.

| Package | Usage | License | Source | Status |
| --- | --- | --- | --- | --- |
| `Avalonia` / `Avalonia.Desktop` / `Avalonia.AvaloniaEdit` / `AvaloniaEdit.TextMate` | UI and editor | MIT | https://github.com/AvaloniaUI/Avalonia / https://github.com/AvaloniaUI/AvaloniaEdit | Approved |
| `Semi.Avalonia` | UI theme | MIT | https://github.com/irihitech/Semi.Avalonia | Approved; only the source-open core package is used |
| `Irihi.Ursa.PrismExtension` / `Irihi.Ursa.Themes.Semi` | Dialogs, theme, Prism extension | MIT | https://github.com/irihitech/Ursa.Avalonia | Approved; kept on the Prism 8-compatible line |
| `Prism.Avalonia` / `Prism.DryIoc.Avalonia` `8.1.97.11073` | Modular app, DI, regions | MIT | https://github.com/AvaloniaCommunity/Prism.Avalonia | Approved; Prism 9 is not used |
| `ReactiveUI.Avalonia` / `Xaml.Behaviors` | MVVM / behaviors | MIT | https://github.com/reactiveui/reactiveui / https://github.com/wieslawsoltes/Xaml.Behaviors | Approved |
| `CodeWF.*` / `Lang.Avalonia.Json` | Internal components | MIT | https://github.com/dotnet9 | Approved; ProDataGrid styling uses `CodeWF.AvaloniaControls.ProDataGrid.Themes` |
| `BCrypt.Net-Next` / `BouncyCastle.Cryptography` / `NBitcoin` / `Otp.NET` | Security and encoding tools | MIT | https://github.com/BcryptNet/bcrypt.net / https://github.com/bcgit/bc-csharp / https://github.com/MetacoSA/NBitcoin / https://github.com/kspearrin/Otp.NET | Approved |
| `Dapper` / `Dapper.AOT` / `Microsoft.Data.Sqlite` | Data access | Apache-2.0 / MIT | https://github.com/DapperLib/Dapper / https://github.com/dotnet/efcore | Approved |
| `CronExpressionDescriptor` / `DiffPlex` / `Figgle` / `Figgle.Fonts` / `Hashids.net` / `IbanNet` / `libphonenumber-csharp` / `Markdig` / `NUlid` / `QRCoder` / `TextMateSharp.Grammars` / `Tomlyn` / `UAParser` / `YamlDotNet` | Tool modules | MIT / BSD / Apache-2.0 | Public repository/project URLs are available from NuGet metadata | Approved |
| `VC-LTL` | Windows compatibility | EPL-2.0 | https://github.com/Chuyu-Team/VC-LTL5 | Source-open; approved under the traceable non-preferred license rule |
| `YY-Thunks` | Windows compatibility | MIT | https://github.com/Chuyu-Team/YY-Thunks | Approved |
| `Microsoft.NET.Test.Sdk` / `coverlet.collector` / `xunit` / `xunit.runner.visualstudio` | Tests | MIT / Apache-2.0 | https://github.com/microsoft/vstest / https://github.com/coverlet-coverage/coverlet / https://github.com/xunit/xunit | Approved |

Transitive dependency check: active restore assets do not contain `Semi.Avalonia.Dock`, `Semi.Avalonia.ProDataGrid`, `Semi.Avalonia.AvaloniaEdit`, `AvaloniaUI.DiagnosticsSupport`, Prism 9 packages, `System.Drawing.Common 4.7.0`, or other black-box components. The `Magick.NET-Q16-AnyCPU` chain used through `CodeWF.Tools.Image 1.3.13.2` is source and license traceable.
