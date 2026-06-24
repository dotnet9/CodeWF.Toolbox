# 码匠工具箱

码匠工具箱是一个基于 Avalonia + Prism 的模块化桌面工具箱，面向开发者日常效率场景。项目将主程序、公共服务与功能模块解耦，方便后续继续扩展新的工具页，而不会把应用演化成一个难维护的大窗体工程。

![应用截图](screen.png)

## 仓库规范

- 当前版本：`12.0.3.3`，版本号统一维护在根目录 `Directory.Build.props` 的 `<Version>` 节点。
- NuGet 包项目统一支持 `net8.0;net10.0`；Demo、App、测试与内部应用项目统一使用 `net11.0` / `net11.0-windows`。
- 根目录 `logo.svg`、`logo.png`、`logo.ico` 是唯一图标源，子工程只通过 MSBuild `Link` 引用，不维护图标副本。
- 运行时帮助、Markdown 示例、内置备忘录、设计说明等业务文档按功能保留；仓库级入口文档使用根目录 `README.md` 和 `UpdateLog.md`。

## 特性

- 基于 Avalonia UI 与 Semi/Ursa 控件构建跨平台桌面界面。
- 使用 Prism 模块目录、依赖注入与 Region 导航组织工具页面。
- 采用 XML 资源做国际化，已包含简体中文、繁体中文、英文和日文。
- 内置格式转换、日志查看、开发辅助、Web 辅助、安全工具、XML 翻译管理等模块。
- 保留面向 Native AOT 发布的脚本与平台常量配置。
- 已完善菜单注册、工具搜索和区域导航边界处理。

## 文档

- [开发文档](docs/Development.md)
- [架构 SVG](docs/assets/architecture.svg)
- [模块生命周期 SVG](docs/assets/module-lifecycle.svg)

![架构图](docs/assets/architecture.svg)

## 快速开始

环境要求：

- .NET 11 SDK
- Avalonia 支持的 Windows、macOS 或 Linux 桌面环境

```powershell
dotnet restore CodeWF.Toolbox.slnx
dotnet build CodeWF.Toolbox.slnx
dotnet run --project src/CodeWF.Toolbox/CodeWF.Toolbox.csproj
```

## 目录结构

```text
src/
  CodeWF.Toolbox/                    桌面应用、主界面、设置与资源
  CodeWF.Core/                       公共抽象、服务与区域定义
  CodeWF.Controls/                   公共控件
  CodeWF.Modules.Converter/          转换工具模块
  CodeWF.Modules.ToolFramework/      本地工具运行框架与工具目录
  CodeWF.Modules.Development/        开发辅助模块
  CodeWF.Modules.LogViewer/          大文件日志查看模块
  CodeWF.Modules.XmlTranslatorManager/ XML 国际化管理模块
  CodeWF.Toolbox.Tests/              单元测试工程
docs/
  assets/                            独立 SVG 图示
```

## 新增模块流程

1. 在 `src/CodeWF.Modules.*` 下创建模块工程。
2. 实现 `IModule`。
3. 通过 `IToolMenuService` 注册分组与工具菜单。
4. 将页面注册到 `RegionNames.ContentRegion`。
5. 在 `App.ConfigureModuleCatalog` 中加入模块。
6. 补齐本模块的 XML 多语言资源和生成的语言键。

详细约定请参考 [开发文档](docs/Development.md)。

## 内置工具

- 日志查看：快速打开大日志文件，只渲染当前可见区域，并支持文件持续追加时的 tail 跟随。
- 格式转换：提供 JSON/YAML、Base64、GUID、日期时间与图片转图标等工具。
- 开发辅助：提供 JSON/YAML 格式化、Shell 与数据处理等日常开发小工具。
- XML 翻译管理：用于比对、合并和维护 XML 国际化资源。

## 第三方开源组件审计（2026-05-20）

检查方式：`dotnet restore CodeWF.Toolbox.slnx --configfile <local-nuget-config>`、`dotnet list package --include-transitive`、NuGet `.nuspec`、NuGet.org 与源码仓库信息。优先接受 MIT / Apache-2.0 / BSD；其它开源协议在源码与传递依赖均可追溯时单独标注。

整改：

- 移除了 `AvaloniaUI.DiagnosticsSupport` 和 `Semi.Avalonia.AvaloniaEdit`；AvaloniaEdit 改为 `Avalonia.AvaloniaEdit` + 官方 Fluent 样式。
- `CodeWF.AvaloniaControls.ProDataGrid` / `CodeWF.AvaloniaControls.ProDataGrid.Themes` 升到 `12.0.3.2`，避开 `Semi.Avalonia.ProDataGrid` 黑盒链。
- 自研 NuGet 依赖更新到本次修复包：`CodeWF.AvaloniaControls.Themes 12.0.3.3`、`CodeWF.EventBus 3.4.5.5`、`CodeWF.Log.Core 12.0.3.1`、`CodeWF.Tools* 1.3.13.2`、`Lang.Avalonia.Json 12.0.3.1`。
- 稳定开源包更新：`Dapper 2.1.79`、`Dapper.AOT 1.0.52`、`Xaml.Behaviors 12.0.0.1`、`coverlet.collector 10.0.1`。
- `BouncyCastle.Cryptography` 从 `2.7.0-beta.98` 改为稳定 `2.6.2`。
- `Microsoft.Data.Sqlite` 从 `11.0.0-preview.4` 改为稳定 `10.0.8`。
- `xunit.runner.visualstudio` 从 `4.0.0-pre.4` 改为稳定 `3.1.5`。
- 按要求保留 `Prism.Avalonia` / `Prism.DryIoc.Avalonia` `8.1.97.11073` 和 `Irihi.Ursa.PrismExtension` `2.0.0`；Prism 9 是收费版本，不升级。

| 包 | 使用范围 | 协议 | 源码/项目地址 | 结论 |
| --- | --- | --- | --- | --- |
| `Avalonia` / `Avalonia.Desktop` / `Avalonia.AvaloniaEdit` / `AvaloniaEdit.TextMate` | UI 与编辑器 | MIT | https://github.com/AvaloniaUI/Avalonia / https://github.com/AvaloniaUI/AvaloniaEdit | 通过 |
| `Semi.Avalonia` | UI 主题 | MIT | https://github.com/irihitech/Semi.Avalonia | 通过，仅保留开源主体包 |
| `Irihi.Ursa.PrismExtension` / `Irihi.Ursa.Themes.Semi` | 对话框、主题与 Prism 扩展 | MIT | https://github.com/irihitech/Ursa.Avalonia | 通过，保留兼容 Prism 8 的版本线 |
| `Prism.Avalonia` / `Prism.DryIoc.Avalonia` `8.1.97.11073` | 模块化、DI、Region | MIT | https://github.com/AvaloniaCommunity/Prism.Avalonia | 通过，未使用 Prism 9 |
| `ReactiveUI.Avalonia` / `Xaml.Behaviors` | MVVM / 行为 | MIT | https://github.com/reactiveui/reactiveui / https://github.com/wieslawsoltes/Xaml.Behaviors | 通过 |
| `CodeWF.*` / `Lang.Avalonia.Json` | 自研组件 | MIT | https://github.com/dotnet9 | 自研开源包，通过；ProDataGrid 主题改用 `CodeWF.AvaloniaControls.ProDataGrid.Themes` |
| `BCrypt.Net-Next` / `BouncyCastle.Cryptography` / `NBitcoin` / `Otp.NET` | 安全与编码工具 | MIT | https://github.com/BcryptNet/bcrypt.net / https://github.com/bcgit/bc-csharp / https://github.com/MetacoSA/NBitcoin / https://github.com/kspearrin/Otp.NET | 通过 |
| `Dapper` / `Dapper.AOT` / `Microsoft.Data.Sqlite` | 数据访问 | Apache-2.0 / MIT | https://github.com/DapperLib/Dapper / https://github.com/dotnet/efcore | 通过 |
| `CronExpressionDescriptor` / `DiffPlex` / `Figgle` / `Figgle.Fonts` / `Hashids.net` / `IbanNet` / `libphonenumber-csharp` / `Markdig` / `NUlid` / `QRCoder` / `TextMateSharp.Grammars` / `Tomlyn` / `UAParser` / `YamlDotNet` | 工具模块 | MIT / BSD / Apache-2.0 | 各 NuGet 包均提供公开 repository/projectUrl | 通过 |
| `VC-LTL` | Windows 兼容 | EPL-2.0 | https://github.com/Chuyu-Team/VC-LTL5 | 源码开放，按“非优先但可追溯”通过 |
| `YY-Thunks` | Windows 兼容 | MIT | https://github.com/Chuyu-Team/YY-Thunks | 源码开放，通过 |
| `Microsoft.NET.Test.Sdk` / `coverlet.collector` / `xunit` / `xunit.runner.visualstudio` | 测试 | MIT / Apache-2.0 | https://github.com/microsoft/vstest / https://github.com/coverlet-coverage/coverlet / https://github.com/xunit/xunit | 通过 |

传递依赖检查结论：有效依赖链未发现 `Semi.Avalonia.Dock`、`Semi.Avalonia.ProDataGrid`、`Semi.Avalonia.AvaloniaEdit`、`AvaloniaUI.DiagnosticsSupport`、Prism 9 包、`System.Drawing.Common 4.7.0` 或其它黑盒组件。`Magick.NET-Q16-AnyCPU` 通过 `CodeWF.Tools.Image 1.3.13.2` 使用，源码与许可证可追溯。
## Package Versioning Convention

Keep NuGet package versions and Central Package Management settings in `Directory.Packages.props`, including shared version properties such as `AvaloniaVersion`. Keep `Directory.Build.props` focused on build, compiler, and NuGet package metadata. When referenced, `VC-LTL` and `YY-Thunks` should use their latest prerelease versions for OS platform compatibility.
