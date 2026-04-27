# CodeWF Toolbox

English | [简体中文](README-zh_CN.md)

CodeWF Toolbox is an Avalonia + Prism desktop demo for building a modular tool client. It keeps the shell, common services, and feature modules separated so new tools can be added without turning the main application into one large window class.

![Application screenshot](screen.png)

## Highlights

- Cross-platform desktop UI based on Avalonia UI and Semi/Ursa controls.
- Prism module catalog, dependency injection, and region navigation.
- XML-based internationalization with Simplified Chinese, Traditional Chinese, English, and Japanese resources.
- Tool modules for AI helpers, format converters, development utilities, and XML translation management.
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

- .NET SDK that can build `net10.0` projects
- Windows, macOS, or Linux desktop runtime supported by Avalonia

```powershell
dotnet restore CodeWF.Toolbox.slnx
dotnet build CodeWF.Toolbox.slnx
dotnet run --project src/CodeWF.Toolbox.Desktop/CodeWF.Toolbox.Desktop.csproj
```

## Solution Layout

```text
src/
  CodeWF.Toolbox.Desktop/            Desktop entry point
  CodeWF.Toolbox/                    Shell, main views, settings, resources
  CodeWF.Core/                       Shared abstractions, services, regions
  CodeWF.Controls/                   Shared controls
  CodeWF.Modules.AI/                 AI utility module
  CodeWF.Modules.Converter/          Converter tools
  CodeWF.Modules.Development/        Development tools
  CodeWF.Modules.XmlTranslatorManager/ XML i18n management tools
docs/
  assets/                            Standalone SVG diagrams
tests/                               Demo and unit test projects
```

## Adding a Module

1. Create a module project under `src/CodeWF.Modules.*`.
2. Implement `IModule`.
3. Register menu entries through `IToolMenuService`.
4. Register views with `RegionNames.ContentRegion`.
5. Add the module to `App.ConfigureModuleCatalog`.
6. Add localization XML files and generated language keys.

See the developer guide for the detailed conventions.
