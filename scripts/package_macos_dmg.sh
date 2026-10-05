#!/usr/bin/env bash
# 打包 CodeWF.Toolbox macOS .app 与 dmg：bundle + 启动台图标 + PkgInfo + ad-hoc 签名。
# 用法：./package_macos_dmg.sh osx-x64|osx-arm64 <version>
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
APP_NAME="CodeWF.Toolbox"
EXECUTABLE_NAME="CodeWF.Toolbox"
BUNDLE_ID="com.dotnet9.codewftoolbox"
CATEGORY="public.app-category.utilities"
ICON_SOURCE="${ICON_SOURCE:-$ROOT_DIR/logo.png}"
PUBLISH_DIR="${PUBLISH_DIR:-}"
OUTPUT_DIR="${OUTPUT_DIR:-$ROOT_DIR/artifacts/release}"
CODESIGN_IDENTITY="${CODESIGN_IDENTITY:-}"

die() { echo "Error: $*" >&2; exit 1; }

usage() {
  cat <<USAGE
Usage:
  ./package_macos_dmg.sh osx-x64|osx-arm64 <version>

Environment:
  ICON_SOURCE         PNG used to build the .icns. Default: <repo>/logo.png
                      (if missing, the bundle is built without a Launchpad icon)
  PUBLISH_DIR         Published output dir. Default: <repo>/artifacts/publish/$RID/CodeWF.Toolbox
  OUTPUT_DIR          Where the dmg lands. Default: <repo>/artifacts/release
  CODESIGN_IDENTITY   Developer ID identity. Empty means ad-hoc signing.
USAGE
}

to_macos_version() {
  # CFBundleShortVersionString 最多三段，第四段起丢弃（dmg 文件名仍用完整版本）。
  local core major minor patch
  core="${1%%[-+]*}"
  IFS='.' read -r major minor patch _ <<<"$core"
  major="${major:-0}"; minor="${minor:-0}"; patch="${patch:-0}"
  [[ "$major" =~ ^[0-9]+$ ]] || major="0"
  [[ "$minor" =~ ^[0-9]+$ ]] || minor="0"
  [[ "$patch" =~ ^[0-9]+$ ]] || patch="0"
  echo "$major.$minor.$patch"
}

create_icon() {
  # logo.png -> iconset -> .icns。Info.plist 缺 CFBundleIconFile 时启动台只会显示通用占位图标。
  local resources_dir="$1"
  local iconset_base iconset_dir
  iconset_base="$(mktemp -d "${TMPDIR:-/tmp}/iconset-XXXXXX")"
  # iconutil 要求目录以 .iconset 结尾，否则报 Invalid Iconset。
  iconset_dir="${iconset_base}.iconset"
  mv "$iconset_base" "$iconset_dir"

  sips -z 16 16 "$ICON_SOURCE" --out "$iconset_dir/icon_16x16.png" >/dev/null
  sips -z 32 32 "$ICON_SOURCE" --out "$iconset_dir/icon_16x16@2x.png" >/dev/null
  sips -z 32 32 "$ICON_SOURCE" --out "$iconset_dir/icon_32x32.png" >/dev/null
  sips -z 64 64 "$ICON_SOURCE" --out "$iconset_dir/icon_32x32@2x.png" >/dev/null
  sips -z 128 128 "$ICON_SOURCE" --out "$iconset_dir/icon_128x128.png" >/dev/null
  sips -z 256 256 "$ICON_SOURCE" --out "$iconset_dir/icon_128x128@2x.png" >/dev/null
  sips -z 256 256 "$ICON_SOURCE" --out "$iconset_dir/icon_256x256.png" >/dev/null
  sips -z 512 512 "$ICON_SOURCE" --out "$iconset_dir/icon_256x256@2x.png" >/dev/null
  sips -z 512 512 "$ICON_SOURCE" --out "$iconset_dir/icon_512x512.png" >/dev/null
  sips -z 1024 1024 "$ICON_SOURCE" --out "$iconset_dir/icon_512x512@2x.png" >/dev/null

  iconutil -c icns "$iconset_dir" -o "$resources_dir/$APP_NAME.icns"
  rm -rf "$iconset_dir"
}

write_info_plist() {
  local plist_path="$1"
  local macos_version="$2"
  local icon_entry=""
  if [[ -f "$ICON_SOURCE" ]]; then
    icon_entry="
  <key>CFBundleIconFile</key>
  <string>$APP_NAME</string>"
  fi

  cat >"$plist_path" <<PLIST
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>CFBundleDisplayName</key><string>$APP_NAME</string>
  <key>CFBundleExecutable</key><string>$EXECUTABLE_NAME</string>$icon_entry
  <key>CFBundleIdentifier</key><string>$BUNDLE_ID</string>
  <key>CFBundleName</key><string>$APP_NAME</string>
  <key>CFBundlePackageType</key><string>APPL</string>
  <key>CFBundleInfoDictionaryVersion</key><string>6.0</string>
  <key>CFBundleShortVersionString</key><string>$macos_version</string>
  <key>CFBundleVersion</key><string>$macos_version</string>
  <key>LSApplicationCategoryType</key><string>public.app-category.utilities</string>
  <key>LSMinimumSystemVersion</key><string>11.0</string>
  <key>NSHighResolutionCapable</key><true/>
  <key>NSSupportsAutomaticGraphicsSwitching</key><true/>
</dict>
</plist>
PLIST
}

[[ $# -eq 2 ]] || { usage; exit 1; }
RID="$1"
VERSION="$2"
[[ -n "$PUBLISH_DIR" ]] || PUBLISH_DIR="$ROOT_DIR/artifacts/publish/$RID/CodeWF.Toolbox"
case "$RID" in
  osx-x64|osx-arm64) ;;
  *) die "Unknown RID: $RID (use osx-x64 or osx-arm64)" ;;
esac
[[ -d "$PUBLISH_DIR" ]] || die "Publish output not found: $PUBLISH_DIR (run dotnet publish first)"
[[ -e "$PUBLISH_DIR/$EXECUTABLE_NAME" ]] || die "Executable not found: $PUBLISH_DIR/$EXECUTABLE_NAME"
MACOS_VERSION="$(to_macos_version "$VERSION")"

STAGE_ROOT="$(mktemp -d "${TMPDIR:-/tmp}/codewf.toolbox-dmg-XXXXXX")"
trap 'rm -rf "$STAGE_ROOT"' EXIT
APP_DIR="$STAGE_ROOT/$APP_NAME.app"
CONTENTS_DIR="$APP_DIR/Contents"
mkdir -p "$CONTENTS_DIR/MacOS" "$CONTENTS_DIR/Resources"

# 全部发布内容拷入 Contents/MacOS 并保持相对布局：
# AOT 单目录发布按 AppContext.BaseDirectory 探测相邻文件，拆散会破坏启动。
cp -R "$PUBLISH_DIR/." "$CONTENTS_DIR/MacOS/"
chmod +x "$CONTENTS_DIR/MacOS/$EXECUTABLE_NAME"

if [[ -f "$ICON_SOURCE" ]]; then
  create_icon "$CONTENTS_DIR/Resources"
else
  echo "WARN: icon source not found ($ICON_SOURCE); bundling without a Launchpad icon." >&2
fi

write_info_plist "$CONTENTS_DIR/Info.plist" "$MACOS_VERSION"
# 老式 bundle 标识，缺了部分系统工具识别不了这是应用包。
printf 'APPL????' >"$CONTENTS_DIR/PkgInfo"

if [[ -n "$CODESIGN_IDENTITY" ]]; then
  codesign --force --deep --options runtime --timestamp --sign "$CODESIGN_IDENTITY" "$APP_DIR"
else
  # ad-hoc 签名：让系统认它是个完整应用包，避免未签名 bundle 被 Gatekeeper 拦。
  codesign --force --deep --sign - --timestamp=none "$APP_DIR"
fi

DMG_STAGE="$STAGE_ROOT/dmg"
mkdir -p "$DMG_STAGE"
cp -R "$APP_DIR" "$DMG_STAGE/$APP_NAME.app"
ln -s /Applications "$DMG_STAGE/Applications"

mkdir -p "$OUTPUT_DIR"
DMG_PATH="$OUTPUT_DIR/$APP_NAME-$VERSION-$RID.dmg"
rm -f "$DMG_PATH"
hdiutil create -volname "$APP_NAME" -srcfolder "$DMG_STAGE" -ov -format UDZO "$DMG_PATH" >/dev/null

echo "Created: $DMG_PATH"
