# macOS 磁盘镜像品牌层

主源：CodeWF.Avalonia 的 `scripts/installer/macos`。各产品仓内保存自包含副本，`source-sha256.json` 记录源文件哈希。目录内文本固定 LF、PNG 标记为二进制，避免跨平台检出改变源哈希。打包时不访问其他仓库或下载依赖。

现有打包脚本继续负责 `.app`、产品图标、资源、签名和校验文件。本层使用系统 `hdiutil` 创建可写 HFS+ 临时镜像，用 `osascript` 配置该镜像 Finder 窗口为 720×480、100 点图标、固定应用与 Applications 位置，再转换为 UDZO 并校验。任何布局、挂载、压缩或校验失败都会终止打包；不会静默发布没有品牌层的镜像。需要可用的 macOS Finder 会话及相应自动化权限。

Finder 或 Spotlight 短暂占用镜像时，正常卸载及 EXIT 清理各最多尝试五次，失败间隔为 1、2、4、8 秒。只调用正常 `hdiutil detach`，不强制卸载、不结束 Finder。正常卸载耗尽次数后，本次打包仍失败且不转换镜像；清理会再次尝试卸载。若仍被占用，保留临时工作目录、挂载内容及其路径，避免误删挂载中的文件。

背景沿用已验收原型的 `#512BD4`、八角核心、椭圆轨道和粒子，核心缩小字号显示 CodeWF，副标题为 `NATIVE APPLICATION · CodeWF`，页脚标注官网 `codewf.com`。官网仅为静态文字，不是可点击链接；不添加额外图标，应用与 Applications 继续保持原有位置。箭头指向系统 Applications 目标，复制、覆盖和系统权限对话框仍由 Finder 处理，不模拟复制进度或接管权限。

默认背景为简洁中英双语。可在打包时设 `CODEWF_DMG_LANGUAGE=en` 或 `zh` 选择单语言背景；合法值为 `bilingual`、`en`、`zh`。DMG 背景是静态资源，无法在用户挂载时随系统语言切换；Finder 自己的菜单、文件名显示和权限交互遵循系统语言。

维护背景时运行 `node scripts/installer/macos/render-backgrounds.cjs`，使用已配置的 Playwright 与 Chromium 生成三组 SVG / PNG；随后重新计算本目录来源清单并同步各仓副本。正式打包只读取已提交 PNG，不需要 Node、Playwright 或在线下载。

`tests/branded-dmg-test.sh` 用受控工具检查三语言 CodeWF 品牌及官网文字、成功流程、参数、语言资源、短暂占用恢复、永久占用保留、EXIT 清理及挂载/布局/转换/验证失败传播。它不调用本机 macOS 工具、不挂载镜像、不操作桌面。Windows 的这些测试与语法检查不证明真实 Finder 布局、Retina、多显示器、Gatekeeper、公证或用户安装已验收；正式发布还需在 macOS 交互会话生成并挂载检查。
