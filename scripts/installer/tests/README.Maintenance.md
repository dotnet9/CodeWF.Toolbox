# 安装器维护逻辑回归

在 Windows 使用 Python 3.9 以上版本与 Inno Setup 6 执行，不依赖第三方 Python 包。`ISCC.exe` 可通过 `--compiler` 参数、`INNO_ISCC` 环境变量或 `PATH` 指定；输出目录可通过 `--output` 指定，默认创建系统临时目录（支持 `CODEWF_TEST_TMP`）。所有仓库资源由脚本自身位置解析，允许项目克隆到任意目录。

```powershell
python scripts/installer/tests/verify_maintenance_logic.py --compiler "C:/Program Files (x86)/Inno Setup 6/ISCC.exe" --output "$env:TEMP/codewf-maintenance-check"
```

测试编译并运行三个独立的纯逻辑夹具，每个 57 项、总计 171 项断言：普通 AppId、标准 GUID 转义、已有双右花括号 AppId。检查精确卸载键、注册卸载路径的引用/尾参/目录边界、卸载成功条件及卸载后/执行中阻止安装和关闭的状态。

运行前检查 ISPP 保存的实际预处理结果，确认注册读取、`ShellExec`、`Exec`、外部函数和安装声明已移除；测试入口 `InitializeSetup` 始终返回 `False`，不会创建安装向导。夹具只将断言结果写入输出目录；不访问真实产品注册，不启动卸载器，不安装文件或快捷方式。

生产维护分支另外编译一次，确认真实注册读取和卸载启动代码仍存在；主视觉引擎与维护模块再集成编译一次，检查品牌条件定义、独立维护模板和完整模板可共同编译。这两个产物均不执行。本回归不代替维护页面的原生控件视觉验收或真实产品安装/卸载测试。

输出包括生成的 `.iss`、预处理 `.iss`、编译日志、三个断言报告和汇总 `receipt.json`。汇总记录编译器、模板、测试源和可执行文件 SHA256；输入在验证中改变会失败。重复使用输出目录时仅移除本轮对应的旧报告与产物，不递归清理其他文件。
