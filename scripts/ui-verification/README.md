# 无桌面 UI 与原生产物验证

验证使用真实 App/Prism 容器、Avalonia Headless 和 Skia，不打开桌面窗口。TOOLBOX_DATA_DIRECTORY 与 APP_CONFIG_FILE 指向输出目录中的独立账户、偏好和日志，不接触日常配置。

```powershell
dotnet build scripts/ui-verification/UiVerification.csproj -c Release --artifacts-path <temporary-build>
dotnet <temporary-build>/bin/UiVerification/release/UiVerification.dll <temporary-output>
dotnet test src/CodeWF.Toolbox.Tests/CodeWF.Toolbox.Tests.csproj -c Release --filter 'Category!=SystemIntegration'
```

覆盖 939 项断言：117 入口元数据、13 自定义页面、真实 Unicode Base64、独立输入/输出、GridSplitter 指针拖动、Dock 分屏和关闭/重开、关闭区域清理、八配色×浅/深、系统模式、偏好重载、明确请求英文不回滚、关于元数据，以及更新检查/下载/校验/安装失败、重试、缺少资产回退、取消与释放。标题栏验证真实 Ursa 三态点击、Studio 配色保留、八色直接选择、完整四语言列表、内部模式提示动态切语及 1120/1280 布局边界。全局管理器直接切语时，验证真实 ComboBox 反向同步与原选项重选；通过真实 MainWindow 的关闭生命周期验证全局事件解绑，仅跳过托盘/退出交互。文档迁移检查拆分后移除原标签、真实浮动与重新停靠时的视觉宿主、同一视图及输入结果、用户拖动后的属性宽度和外部 Ribbon 几何；重新停靠直接使用标准 Dock API，无额外激活操作。更新通过 IUpdateService 假对象验证，不运行安装器。

工具运行、登录及退出确认三个真实按钮覆盖 16 套配色的默认、键盘焦点、悬停、按下和禁用共 240 个状态。测试读取模板 `PART_ContentPresenter` 和实际文字的最终颜色，核对主色角色并要求可交互状态的文字对比度不低于 4.5；使用真实指针事件切换状态，释放到按钮外，不触发登录、工具运行或退出。退出确认窗另检查四种语言的记住选择标签完整、复选框与操作按钮分行。明细输出为 `primary-button-contrast.tsv`。

原有自启动测试标记为 SystemIntegration：它会修改操作系统自启动设置，只能在可丢弃测试机显式运行。

NativeAOT 发布验证器时使用与正式应用相同的完整反射元数据和 TrimmerRoots.xml；运行发布后的 UiVerification.exe，检查输出 NativeAOT=True。正式应用另有 --verify-runtime <temporary-output>，验证 AOT、发布资源、Unicode 算法及独立嵌套目录中的真实 SQLite 配置读写，确认当前目录不落库，执行后退出，不创建窗口。

12.0.15 将账号、偏好与 JSON 格式化数据库统一到 Core 数据根。6 项纯测试包括两个独立目录、含分号路径、真实 JSON 配置保存/重读及当前目录不落库；已有相对路径数据库保留原位，不自动迁移或删除。

发布脚本支持 `-OutputDirectory`、`-ArtifactsPath`、`-NuGetConfig`，可将正式产品构建、发布资源和安装器输入全部保存在仓库外，仍保留相同的 NativeAOT 参数与默认包源。

文件/网络工具的外部副作用和各操作系统安装器交互不由此验证；发布流水线分别在五个原生 runner 构建、运行自检并验证安装包 SHA-256。

当前依赖仍有既存的 `SQLitePCLRaw.lib.e_sqlite3 2.1.11` 高严重性告警（[GHSA-2m69-gcr7-jv3q](https://github.com/advisories/GHSA-2m69-gcr7-jv3q)）。本轮未更改数据库依赖版本，界面和 NativeAOT 通过不代表该告警已解决。

真实宿主窗口增加动态 CanMinimize / CanMaximize 能力、按钮禁用、程序化点击守卫与原状态恢复断言；Toolbox 关于是设置页，能力检查在真实主窗口执行。

2026-10-15 历史 12.0.12 验证使用 CodeWF.Avalonia 12.1.4.22、CodeWF.Toolkit 1.0.0.19：托管与 NativeAOT UI 验证均通过 292 项断言；正式产品 12.0.12.0 的 `--verify-runtime` 实际运行通过，验证发布 I18n 与 Unicode Base64 往返。

最终构建将 Prism 传递引入的 XAML Loader 固定为 12.1.4，与 Avalonia 一致；正式产品与验证器均检查此解析版本，避免原生发布混入 11.0.7。

同一正式产品目录通过 Inno Setup 编译生成 `CodeWF.Toolbox-v12.0.12-win-x64-setup.exe`，60,759,456 字节，SHA256 为 `e42bfb7dd91777466e2917e21e2b1c26c49ee8d634e7cc234ac6dd698ce0615b`。仅编译并检查安装器，未执行安装；其他系统发布结果以对应 CI 为准。
