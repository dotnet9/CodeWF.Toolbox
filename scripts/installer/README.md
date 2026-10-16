# .NET 紫安装器

源目录为 CodeWF.Avalonia 的 `scripts/installer`。所有 App 与两个 Demo 在各自仓库保留完整快照，打包不读取相邻仓库。`source-sha256.json` 记录共享源摘要；文本统一 LF，位图保持二进制。

## Windows

产品 `.iss` 引入 `installer\CodeWF.Setup.iss`，继续定义自身 AppId、版本、权限、目录、任务、文件、图标和启动项。品牌层使用 Inno Setup 6 的原生页面与 Windows GDI，主色为 `#512BD4`，无需 WebView、联网下载或第三方皮肤 DLL。

- 实际窗口区域为凹缺、突出左舱、右切角和阶梯底边。标题栏支持拖动与最小化，按钮保留原生键盘和焦点行为。
- 深色、浅色、跟随系统循环切换；默认深色。系统外观只读，不修改用户个性化设置。
- 八角 .NET 核心、轨道与粒子响应真实安装进度。减少动态效果时停止粒子及呼吸动画；隐藏和最小化时停止绘制。生产流程不注入测试等待时间。
- MIT 许可保留各仓库原文，界面支持简体中文和英文。许可、目录、安装任务和完成启动项由 Inno 正常处理。
- 维护入口只匹配自身精确 AppId。修复复用正常安装，卸载打开已验证的注册卸载器；成功后关闭安装向导，禁止继续安装。
- 独立卸载窗口使用相同品牌资源，百分比读取原生进度条。资源放在 `{app}\.codewf-installer` 并由 Inno 卸载日志精确清理。保留未登记文件，不递归清空安装目录。

使用 32 位 Inno Setup 引擎编译；产品载荷仍为 Windows x64 NativeAOT。当前 GDI 回调按该引擎 ABI 定义，升级编译器时必须保留引擎架构并运行原生夹具。

```powershell
ISCC.exe scripts/<产品名>.iss /DAppVersion=<版本> /DSourceDir=<NativeAOT发布目录> /DOutputDir=<安装包目录>
```

测试目录中的安装夹具不创建卸载注册、快捷方式或文件关联，也不启动产品。原生 GUI 验证器在独立桌面运行，不切换用户桌面；正式产品安装器只编译，不在开发机替用户执行安装。维护逻辑检查预处理结果，确认夹具排除了真实注册探测和卸载启动。

## macOS 与 Linux

`macos/` 提供统一 DMG 背景与 Finder 拖放布局，生成 x64 / arm64 DMG；系统复制、覆盖、Gatekeeper 和授权仍由 macOS 处理。默认使用中英双语静态背景。可选 PKG 仍是原型提案，不新增第二种正式交付格式。

`linux/` 提供统一品牌与 AppStream 元数据，生成 x64 / arm64 DEB。软件管理器外观及授权受发行版控制，不能由包文件统一为 Windows 异形窗口。

所有平台交付真实 EXE / DEB / DMG 与 SHA-256，不用 ZIP 代替安装包。Windows 夹具或受控 shell 测试不等同于真实 Linux / macOS 安装验收；签名、公证、多 DPI、多显示器及读屏仍需要相应环境检查。
