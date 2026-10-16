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
  for local_command in hdiutil plutil osascript sync sleep; do
    command -v "$local_command" >/dev/null || { echo "Required macOS tool missing: $local_command" >&2; exit 2; }
  done
  local_work="$(mktemp -d "${TMPDIR:-/tmp}/codewf-branded-dmg-XXXXXX")" || exit 1
  local_work="$(cd "$local_work" && pwd -P)" || exit 1
  local_mount="$local_work/mount"
  local_rw="$local_work/writable.dmg"
  local_mounted=0
  local_device=''
  local_attachment_unknown=0
  brand_dmg_is_detached() {
    local state
    hdiutil info -plist >"$local_work/info.plist" || return 2
    # 系统 Foundation 只读取所需字段；info 的 image-alias 可能是 NSData，不能整体转 JSON。
    state="$(osascript -l JavaScript -e '
      function attachmentState(images, imagePath, device) {
        if (!Array.isArray(images)) throw new Error("Invalid hdiutil images array");
        var attached = false;
        images.forEach(function (image) {
          if (!image || typeof image["image-path"] !== "string" ||
              !Array.isArray(image["system-entities"])) throw new Error("Invalid hdiutil image");
          if (image["image-path"] === imagePath) attached = true;
          var hasDevice = false;
          image["system-entities"].forEach(function (entity) {
            if (!entity || typeof entity !== "object") throw new Error("Invalid hdiutil entity");
            if (entity["dev-entry"] !== undefined) {
              if (typeof entity["dev-entry"] !== "string") throw new Error("Invalid hdiutil device");
              hasDevice = true;
              if (entity["dev-entry"] === device) attached = true;
            }
          });
          if (!hasDevice) throw new Error("Missing hdiutil image device");
        });
        return attached ? "attached" : "detached";
      }
      function run(argv) {
        ObjC.import("Foundation");
        var document = $.NSDictionary.dictionaryWithContentsOfFile(argv[0]);
        if (!document || document.isNil()) throw new Error("Invalid hdiutil info plist");
        var images = document.objectForKey("images");
        if (!images || !images.isKindOfClass($.NSArray)) throw new Error("Invalid hdiutil images");
        var values = [];
        for (var i = 0; i < Number(images.count); i++) {
          var image = images.objectAtIndex(i);
          if (!image.isKindOfClass($.NSDictionary)) throw new Error("Invalid hdiutil image dictionary");
          var path = image.objectForKey("image-path");
          var entities = image.objectForKey("system-entities");
          if (!path || !path.isKindOfClass($.NSString) || !entities ||
              !entities.isKindOfClass($.NSArray)) throw new Error("Invalid hdiutil image fields");
          var devices = [];
          for (var j = 0; j < Number(entities.count); j++) {
            var entity = entities.objectAtIndex(j);
            if (!entity.isKindOfClass($.NSDictionary)) throw new Error("Invalid hdiutil entity dictionary");
            var entry = entity.objectForKey("dev-entry");
            if (entry && !entry.isNil()) {
              if (!entry.isKindOfClass($.NSString)) throw new Error("Invalid hdiutil device field");
              devices.push({"dev-entry": ObjC.unwrap(entry)});
            }
          }
          values.push({"image-path": ObjC.unwrap(path), "system-entities": devices});
        }
        return attachmentState(values, argv[1], argv[2]);
      }
    ' "$local_work/info.plist" "$local_rw" "$local_device")" || return 2
    case "$state" in detached) return 0 ;; attached) return 1 ;; *) return 2 ;; esac
  }
  detach_brand_dmg() {
    local attempt=1 delay=1 state
    [[ -n "$local_device" && "$local_attachment_unknown" == 0 ]] || return 1
    # Finder/Spotlight 可能短暂持有镜像；仅正常卸载，最多五次，退避共十五秒。
    while [[ "$attempt" -le 5 ]]; do
      if hdiutil detach "$local_device" >/dev/null; then
        local_mounted=0
        return 0
      fi
      if brand_dmg_is_detached; then
        local_mounted=0
        return 0
      else
        state=$?
        if [[ "$state" != 1 ]]; then
          local_attachment_unknown=1
          echo "Could not confirm temporary DMG attachment; retaining $local_work" >&2
          return 1
        fi
      fi
      [[ "$attempt" -lt 5 ]] || break
      echo "Temporary DMG busy; detach retry $((attempt + 1))/5 in ${delay}s: $local_device" >&2
      sleep "$delay" || return 1
      attempt=$((attempt + 1))
      delay=$((delay * 2))
    done
    return 1
  }
  cleanup_brand_dmg() {
    # 仅回收本次 mktemp 路径；布局失败也要先卸载，避免删除挂载中的内容。
    if [[ "$local_mounted" == 1 ]]; then
      if ! detach_brand_dmg; then
        echo "Could not detach temporary DMG; retained at $local_work (device: $local_device, mount: $local_mount)" >&2
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
  hdiutil attach -plist -readwrite -noverify -noautoopen -mountpoint "$local_mount" "$local_rw" >"$local_work/attach.plist" || exit 1
  local_mounted=1
  local_entities="$(plutil -extract system-entities json -o - "$local_work/attach.plist")" || { local_attachment_unknown=1; exit 1; }
  local_device="$(osascript -l JavaScript -e '
    function run(argv) {
      var entities = JSON.parse(argv[0]);
      if (!Array.isArray(entities)) throw new Error("Invalid hdiutil attach entities");
      var devices = entities.filter(function (entity) {
        return entity && typeof entity["dev-entry"] === "string" && /^\/dev\/disk[0-9]+$/.test(entity["dev-entry"]);
      });
      if (devices.length !== 1) throw new Error("Expected one temporary DMG whole-disk device");
      return devices[0]["dev-entry"];
    }
  ' "$local_entities")" || { local_attachment_unknown=1; exit 1; }
  [[ "$local_device" =~ ^/dev/disk[0-9]+$ ]] || { local_attachment_unknown=1; echo 'Could not identify temporary DMG device; retaining working directory.' >&2; exit 1; }
  osascript "$CODEWF_DMG_ASSET_ROOT/configure-finder.applescript" "$local_mount" "$local_app.app" >/dev/null || exit 1
  [[ -s "$local_mount/.DS_Store" ]] || { echo 'Finder did not persist the DMG layout (.DS_Store missing).' >&2; exit 1; }
  sync || exit 1
  detach_brand_dmg || exit 1
  hdiutil convert "$local_rw" -format UDZO -imagekey zlib-level=9 -ov -o "$local_output" >/dev/null || exit 1
  hdiutil verify "$local_output" >/dev/null || exit 1
  [[ -s "$local_output" ]] || { echo 'Branded DMG output is missing or empty.' >&2; exit 1; }
)

if [[ "${BASH_SOURCE[0]}" == "$0" ]]; then
  codewf_create_branded_dmg "$@"
fi
