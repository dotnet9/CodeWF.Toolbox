#!/usr/bin/env bash
# Finder 负责拖放、复制和权限交互；这里只写入镜像背景与图标布局。

CODEWF_DMG_ASSET_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

codewf_create_branded_dmg() (
  set -euo pipefail
  [[ $# -eq 3 || $# -eq 4 ]] || { echo 'Usage: codewf_create_branded_dmg <stage> <app-name> <output.dmg> [volume-name]' >&2; exit 2; }
  local_stage="$1"
  local_app="$2"
  local_output="$3"
  local_volume="${4:-$2}"
  local_language="${CODEWF_DMG_LANGUAGE:-bilingual}"
  case "$local_language" in en|zh|bilingual) ;; *) echo 'CODEWF_DMG_LANGUAGE must be en, zh or bilingual.' >&2; exit 2 ;; esac
  [[ -d "$local_stage/$local_app.app" && -e "$local_stage/Applications" ]] || { echo 'DMG stage must contain the app bundle and Applications shortcut.' >&2; exit 2; }
  [[ -s "$CODEWF_DMG_ASSET_ROOT/background-$local_language.png" ]] || { echo 'DMG brand background is missing.' >&2; exit 2; }
  for local_command in hdiutil osascript sync; do
    command -v "$local_command" >/dev/null || { echo "Required macOS tool missing: $local_command" >&2; exit 2; }
  done
  local_work="$(mktemp -d "${TMPDIR:-/tmp}/codewf-branded-dmg-XXXXXX")" || exit 1
  local_mount="$local_work/mount"
  local_rw="$local_work/writable.dmg"
  local_mounted=0
  cleanup_brand_dmg() {
    # 仅回收本次 mktemp 路径；布局失败也要先卸载，避免删除挂载中的内容。
    if [[ "$local_mounted" == 1 ]]; then
      if ! hdiutil detach "$local_mount" >/dev/null 2>&1; then
        echo "Could not detach temporary DMG; retained at $local_work" >&2
        return 1
      fi
    fi
    [[ "$local_work" == */codewf-branded-dmg-* ]] && rm -rf "$local_work"
  }
  trap cleanup_brand_dmg EXIT
  mkdir -p "$local_stage/.background" "$local_mount" || exit 1
  cp "$CODEWF_DMG_ASSET_ROOT/background-$local_language.png" "$local_stage/.background/codewf.png" || exit 1
  # 先创建可写 HFS+，让 Finder 写入 .DS_Store，再转换为只读压缩镜像。
  hdiutil create -volname "$local_volume" -srcfolder "$local_stage" -fs HFS+ -format UDRW -ov "$local_rw" >/dev/null || exit 1
  hdiutil attach -readwrite -noverify -noautoopen -mountpoint "$local_mount" "$local_rw" >/dev/null || exit 1
  local_mounted=1
  osascript "$CODEWF_DMG_ASSET_ROOT/configure-finder.applescript" "$local_mount" "$local_app.app" >/dev/null || exit 1
  [[ -s "$local_mount/.DS_Store" ]] || { echo 'Finder did not persist the DMG layout (.DS_Store missing).' >&2; exit 1; }
  sync || exit 1
  hdiutil detach "$local_mount" >/dev/null || exit 1
  local_mounted=0
  hdiutil convert "$local_rw" -format UDZO -imagekey zlib-level=9 -ov -o "$local_output" >/dev/null || exit 1
  hdiutil verify "$local_output" >/dev/null || exit 1
  [[ -s "$local_output" ]] || { echo 'Branded DMG output is missing or empty.' >&2; exit 1; }
)

if [[ "${BASH_SOURCE[0]}" == "$0" ]]; then
  codewf_create_branded_dmg "$@"
fi
