# 码坊工具箱

[English](README.md) | 简体中文

码坊工具箱是一个基于 Avalonia UI + Prism 的模块化桌面工具客户端，面向开发者日常效率场景。项目重点展示如何把主壳、公共服务、工具菜单、区域导航和业务模块拆开，让后续新增工具时不必把逻辑堆进主窗口。

![应用截图](screen.png)

## 亮点

- 基于 Avalonia UI、Semi.Avalonia、Ursa 控件构建跨平台桌面界面。
- 使用 Prism 模块目录、依赖注入与 Region 导航组织工具页面。
- 使用 XML 资源做国际化，已包含简体中文、繁体中文、英文、日文。
- 内置 AI、格式转换、日志阅读、开发辅助、XML 翻译管理等模块。
- 保留面向 Native AOT 发布的脚本和平台常量。
- 已完善菜单注册、顶部工具搜索和区域导航边界处理。

## 文档

- [中文开发文档](docs/README.zh-CN.md)
- [Developer guide](docs/README.md)
- [架构 SVG](docs/assets/architecture.svg)
- [模块生命周期 SVG](docs/assets/module-lifecycle.svg)

![架构设计](docs/assets/architecture.svg)

## 快速开始

环境要求：

- 能构建 `net10.0` 项目的 .NET SDK
- Avalonia 支持的 Windows、macOS 或 Linux 桌面环境

```powershell
dotnet restore CodeWF.Toolbox.slnx
dotnet build CodeWF.Toolbox.slnx
dotnet run --project src/CodeWF.Toolbox.Desktop/CodeWF.Toolbox.Desktop.csproj
```

## 目录结构

```text
src/
  CodeWF.Toolbox.Desktop/            桌面入口项目
  CodeWF.Toolbox/                    主壳、主界面、设置、资源
  CodeWF.Core/                       公共抽象、服务、区域适配器
  CodeWF.Controls/                   公共控件
  CodeWF.Modules.AI/                 AI 工具模块
  CodeWF.Modules.Converter/          转换工具模块
  CodeWF.Modules.LogViewer/          支持大文件与实时 tail 的日志阅读模块
  CodeWF.Modules.Development/        开发辅助模块
  CodeWF.Modules.XmlTranslatorManager/ XML 国际化管理模块
docs/
  assets/                            独立 SVG 图
tests/                               单元测试项目
```

## 新增模块流程

1. 在 `src/CodeWF.Modules.*` 下创建模块项目。
2. 实现 `IModule`。
3. 通过 `IToolMenuService` 注册分组和工具菜单。
4. 将工具页面注册到 `RegionNames.ContentRegion`。
5. 在 `App.ConfigureModuleCatalog` 中加入模块。
6. 补齐本模块的 XML 多语言资源和生成的语言键。

详细约定见中文开发文档。

## 内置工具

- 日志阅读器：快速打开大日志文件，只渲染当前可见内容，并支持文件追加时实时 tail 追踪。
- 格式转换：JSON/YAML、Base64、GUID、日期时间和图片转图标等工具。
- 开发辅助：JSON/YAML 格式化和日常开发小工具。
- XML 翻译管理：对比、合并和维护 XML 国际化资源。
