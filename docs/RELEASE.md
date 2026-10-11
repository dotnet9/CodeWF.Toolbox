# 发布流程规范（RELEASE）

> 本文档是各仓库统一的标准发布流程。**每次发版都按本文档执行**，避免出现
> 「Release 页只有 Full Changelog 链接」「安装包缺失」「版本号不一致」这类不规范情况。

## 1. 版本号约定

- 版本号唯一来源：根目录 `Directory.Build.props` 的 `<Version>`（个别仓库在主 csproj，如 IEditor/QuickApp）。
- 发布 tag 必须与该版本一致：`<Version>1.2.3</Version>` ↔ tag `v1.2.3`。
- README 的「当前版本」行（如有）同步更新。
- 更新日志：有 `UpdateLog.md` 的仓库，发布前新增对应版本条目（`## 版本 (YYYY-MM-DD)`）。

## 2. 标准发布流程

```text
1. 改版本号（Directory.Build.props / csproj）+ 更新 UpdateLog.md + 同步 README 版本行
2. 提交：chore(release): vX.Y.Z（英文规范提交，仓库另有约定从其约定）
3. 打附注 tag —— tag 的附注消息就是 GitHub Release 的发布说明（见下节）：
       git tag -a vX.Y.Z -m "应用名 X.Y.Z\n\n- 要点一\n- 要点二 ..."
4. 推送分支与 tag：git push origin <branch> && git push origin vX.Y.Z
5. CI（release.yml）自动：测试 → 全平台 NativeAOT 发布 → 制作安装包 → 创建 GitHub Release
6. 到 Release 页确认：说明可读、各平台安装包齐全（win exe / linux deb / mac dmg）
```

## 3. 发布说明规范（重要）

**Release 页的说明来自附注 tag 的消息**。打 tag 时必须写清楚：

```bash
git tag -a vX.Y.Z -m "应用名 X.Y.Z

本版本做了什么（第一段概览）

- 变更要点 1
- 变更要点 2"
```

- 工作流的「准备发布说明」步骤会按以下优先级取内容：
  1. 精选说明 `.github/release-notes/vX.Y.Z.md`（存在则优先）；
  2. **附注 tag 消息**（`git tag -l vX.Y.Z --format='%(contents)'`）；
  3. 兜底：仅版本标题 + 自动生成的 Full Changelog（**不允许停留在这一档**）。
- 不确定时，发布后打开 Release 页检查说明是否可读。

## 4. 平台与安装包矩阵

| 平台 | 产物 | 说明 |
| --- | --- | --- |
| win-x64 | 安装器或 zip（各仓既有形态） | NativeAOT |
| win-x86 | 同上 | NativeAOT 不支持 x86 的仓库保持自包含单文件 |
| linux-x64 / linux-arm64 | `.deb`（amd64/arm64） | NativeAOT；arm64 在 arm runner 上原生编译 |
| osx-x64 / osx-arm64 | `.dmg` | NativeAOT；保留符号（Apple ld_classic 不支持压缩调试段） |

- 统一配方：`-p:PublishAot=true -p:PublishTrimmed=true -p:PublishSingleFile=false
  -p:IlcGenerateCompleteTypeMetadata=true -p:IlcTrimMetadata=false -p:IlcSingleThreaded=true`。
- Linux runner 需 `clang zlib1g-dev`；不要在 x64 runner 上交叉编译其他架构的 AOT。
- 平台相关功能未适配时，应用内以「当前平台功能正在开发中」友好提示，不阻塞发布与启动。

## 5. 发布后检查

- Release 页：说明可读、资产齐全（对照矩阵）、带 `.sha256` 的照旧。
- 安装包抽查：至少在 Windows 本机跑一次发布产物。
