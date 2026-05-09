# 码匠工具箱

[English](README.md) | 简体中文

码匠工具箱是一个基于 Avalonia + Prism 的模块化桌面工具箱，面向开发者日常效率场景。项目将主程序、公共服务与功能模块解耦，方便后续继续扩展新的工具页，而不会把应用演化成一个难维护的大窗体工程。

![应用截图](screen.png)

## 特性

- 基于 Avalonia UI 与 Semi/Ursa 控件构建跨平台桌面界面。
- 使用 Prism 模块目录、依赖注入与 Region 导航组织工具页面。
- 采用 XML 资源做国际化，已包含简体中文、繁体中文、英文和日文。
- 内置 AI、格式转换、日志查看、开发辅助、XML 翻译管理等模块。
- 保留面向 Native AOT 发布的脚本与平台常量配置。
- 已完善菜单注册、工具搜索和区域导航边界处理。

## 文档

- [中文开发文档](docs/README.zh-CN.md)
- [Developer guide](docs/README.md)
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
  CodeWF.Modules.AI/                 AI 工具模块
  CodeWF.Modules.Converter/          转换工具模块
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

详细约定请参考开发文档。

## 内置工具

- 日志查看：快速打开大日志文件，只渲染当前可见区域，并支持文件持续追加时的 tail 跟随。
- 格式转换：提供 JSON/YAML、Base64、GUID、日期时间与图片转图标等工具。
- 开发辅助：提供 JSON/YAML 格式化等日常开发小工具。
- XML 翻译管理：用于比对、合并和维护 XML 国际化资源。
