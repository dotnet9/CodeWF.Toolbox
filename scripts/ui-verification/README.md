# 无桌面 UI 与原生产物验证

验证使用真实 App/Prism 容器、Avalonia Headless 和 Skia，不打开桌面窗口。TOOLBOX_DATA_DIRECTORY 与 APP_CONFIG_FILE 指向输出目录中的独立账户、偏好和日志，不接触日常配置。

```powershell
dotnet build scripts/ui-verification/UiVerification.csproj -c Release --artifacts-path <temporary-build>
dotnet <temporary-build>/bin/UiVerification/release/UiVerification.dll <temporary-output>
dotnet test src/CodeWF.Toolbox.Tests/CodeWF.Toolbox.Tests.csproj -c Release --filter 'Category!=SystemIntegration'
```

覆盖 272 项断言：117 入口元数据、13 自定义页面、真实 Unicode Base64、独立输入/输出、GridSplitter 指针拖动、Dock 分屏和关闭/重开、关闭区域清理、八配色×浅/深、系统模式、偏好重载、明确请求英文不回滚、关于元数据，以及更新检查/下载/校验/安装失败、重试、缺少资产回退、取消与释放。标题栏新增真实 Ursa 三态点击、Studio 配色保留、八色直接选择、完整四语言列表、内部模式提示动态切语及 1120/1280 布局边界。更新通过 IUpdateService 假对象验证，不运行安装器。

原有自启动测试标记为 SystemIntegration：它会修改操作系统自启动设置，只能在可丢弃测试机显式运行。

NativeAOT 发布验证器时使用与正式应用相同的完整反射元数据和 TrimmerRoots.xml；运行发布后的 UiVerification.exe，检查输出 NativeAOT=True。正式应用另有 --verify-runtime <temporary-output>，仅验证 AOT、发布资源与 Unicode 算法，执行后退出，不创建窗口。

发布脚本支持 `-OutputDirectory`、`-ArtifactsPath`、`-NuGetConfig`，可将正式产品构建、发布资源和安装器输入全部保存在仓库外，仍保留相同的 NativeAOT 参数与默认包源。

文件/网络工具的外部副作用和各操作系统安装器交互不由此验证；发布流水线分别在五个原生 runner 构建、运行自检并验证安装包 SHA-256。
