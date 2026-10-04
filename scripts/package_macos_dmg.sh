#!/usr/bin/env bash
# 打包 macOS .dmg 安装镜像（hdiutil，在 macOS runner 上运行）。
# 用法：scripts/package_macos_dmg.sh <osx-x64|osx-arm64> <version>
set -euo pipefail

RID="${1:?usage: package_macos_dmg.sh <osx-x64|osx-arm64> <version>}"
VERSION="${2:?usage: package_macos_dmg.sh <osx-x64|osx-arm64> <version>}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
PUBLISH_DIR="${PUBLISH_DIR:-$REPO_ROOT/artifacts/publish/$RID/CodeWF.Toolbox}"
OUTPUT_DIR="${OUTPUT_DIR:-$REPO_ROOT/artifacts/release}"
APP_NAME="CodeWF.Toolbox"

[[ -d "$PUBLISH_DIR" ]] || { echo "Publish output was not produced: $PUBLISH_DIR" >&2; exit 1; }

mkdir -p "$OUTPUT_DIR"
DMG_PATH="$OUTPUT_DIR/$APP_NAME-$VERSION-$RID.dmg"
rm -f "$DMG_PATH"

hdiutil create -volname "$APP_NAME $VERSION ($RID)" \
  -srcfolder "$PUBLISH_DIR" \
  -ov -format UDZO \
  "$DMG_PATH"

[[ -f "$DMG_PATH" ]] || { echo "DMG was not produced: $DMG_PATH" >&2; exit 1; }
echo "DMG 打包完成：$DMG_PATH"
