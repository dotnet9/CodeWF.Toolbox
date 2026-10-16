Linux DEB 包包含应用本身的图标、桌面入口、中英文软件说明、MIT 许可与 Homepage，以及供软件管理器读取的 AppStream 元数据。PNG 按原始尺寸放入 hicolor，已有 SVG 同时放入 scalable；文件均随当前仓库独立打包。

`.NET` 品牌紫 `#512BD4` 通过 AppStream 的 `branding` 字段提供。软件管理器是否采用该颜色由发行版决定；DEB 不能统一替换系统安装器的窗口形状、动画或皮肤。包没有引用尚未发布的宣传截图。

维护 `scripts/installer/linux/product.json` 更新本仓库描述。公共生成脚本来源及 SHA256 记录在同目录 `source.json`，副本已包含在仓库中，打包不依赖其他仓库。

运行 `pwsh scripts/tests/linux_metadata_test.ps1` 检查元数据、图标、许可、桌面入口和体积。Windows 可验证暂存文件；真实 DEB 构建与软件管理器显示需在 Linux 使用 `dpkg-deb`、`appstreamcli validate` 和桌面环境验证。

运行 `pwsh scripts/tests/linux_packaging_test.ps1` 覆盖 x64/arm64 两种架构的暂存结构、语言资源、符号剔除、启动参数、校验和与失败传播，同时执行元数据检查。Windows 使用受控打包边界夹具，Linux 有 `dpkg-deb` 时调用真实打包工具；该测试不安装或启动产品。
