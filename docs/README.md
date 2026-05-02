# CodeWF Toolbox Developer Guide

English | [简体中文](README.zh-CN.md)

This guide explains the project boundaries, module onboarding flow, localization resources, and release checks for developers learning Avalonia + Prism modular application design.

## Architecture

![Architecture](assets/architecture.svg)

The solution is organized into five areas:

- `CodeWF.Toolbox.Desktop`: desktop entry point, AppBuilder setup, icon, manifest, and publish profiles.
- `CodeWF.Toolbox`: application shell, main window, menu, settings, themes, login window, and Prism module catalog.
- `CodeWF.Core`: shared abstractions, file chooser, notifications, tool menu service, region names, and TabControl region adapter.
- `CodeWF.Modules.*`: feature modules. Each module owns its menu entries, views, view models, and resources. Current modules include AI helpers, converters, log viewing, development tools, and XML localization management.
- `docs`/`tests`/`publish`: documentation, unit tests, and publishing scripts.

## Module Lifecycle

![Module lifecycle](assets/module-lifecycle.svg)

At startup, `App.ConfigureModuleCatalog` adds modules to the Prism catalog. A module constructor registers tool entries through `IToolMenuService`; `OnInitialized` registers views with `RegionNames.ContentRegion`. When the user chooses a menu item, the shell asks Prism Region navigation to display the target view.

## Conventions

- Menu names and descriptions should use generated `Localization.*` resource keys.
- View names should use `nameof(MyToolView)` to avoid string drift.
- Modules should depend on `CodeWF.Core` abstractions, not on shell internals.
- Tool status should be expressed with `ToolStatus`: `Planned`, `Developing`, or `Complete`.
- Complex control initialization may live in the View code-behind, while business behavior should stay in the ViewModel.
- Shared capabilities should be introduced through `CodeWF.Core` interfaces and service implementations.

## Adding a Tool Module

1. Create `src/CodeWF.Modules.YourModule`.
2. Reference `CodeWF.Core` and the Prism/Avalonia packages used by the solution.
3. Create `YourModule : IModule`.
4. Register groups and tool items through `IToolMenuService`.
5. Register views with `RegisterViewWithRegion<TView>(RegionNames.ContentRegion)`.
6. Register special view models, dialogs, or services in `RegisterTypes`.
7. Add `I18n/*.xml` and generate or update `I18n/Language.cs`.
8. Add the module to `CodeWF.Toolbox/App.axaml.cs`.

Example:

```csharp
public class SampleModule : IModule
{
    public SampleModule(IToolMenuService toolMenuService)
    {
        var groupName = Localization.SampleModule.Title;
        toolMenuService.AddSeparator();
        toolMenuService.AddGroup(groupName, Icons.Sample);
        toolMenuService.AddItem(
            Localization.SampleView.Title,
            groupName,
            Localization.SampleView.Description,
            nameof(SampleView),
            Icons.Sample,
            ToolStatus.Developing);
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
        var regionManager = containerProvider.Resolve<IRegionManager>();
        regionManager.RegisterViewWithRegion<SampleView>(RegionNames.ContentRegion);
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
    }
}
```

## Localization

Each module owns its resource files, for example:

- `I18n/YourModule.zh-CN.xml`
- `I18n/YourModule.en-US.xml`
- `I18n/YourModule.ja-JP.xml`
- `I18n/YourModule.zh-Hant.xml`

Keep all language files structurally aligned. Menus, buttons, prompts, and error messages should use resource keys instead of fixed text in XAML or ViewModels. Short fallback text is acceptable for development diagnostics.

## Quality Checks

Before submitting changes, run:

```powershell
dotnet restore CodeWF.Toolbox.slnx
dotnet build CodeWF.Toolbox.slnx
dotnet test src/CodeWF.Toolbox.Tests/CodeWF.Toolbox.Tests.csproj
```

Check that:

- New tools can be found through menu search and navigate correctly.
- Groups and separators never trigger empty navigation.
- File chooser, clipboard, and drag/drop flows handle cancellation and null values.
- ViewModel async methods either await work or explicitly return `Task.CompletedTask`.
- Localization displays correctly in at least Chinese and English.
- Large text viewers should keep business logic in ViewModels and use shared editor helpers for selection, font, and scroll behavior.

## Publishing

The repository keeps `publish.bat`, `publish_win-x64.bat`, `publish_all.bat`, and several publish profiles. Before publishing, verify:

- `Directory.Build.props` contains the intended Avalonia/DataGrid versions and platform constant.
- `GlobalAssemblies/GlobalAssembly.cs` has the expected version and product description.
- The output directory will not overwrite a release that should be preserved.

## Follow-Up Work

- Reduce the remaining nullable warnings, especially in the XML translation manager module.
- Add stricter input validation and failure messages for each tool.
- Expand unit tests for core services and conversion logic.
- Consolidate publishing scripts into cross-platform PowerShell or CI workflows.
