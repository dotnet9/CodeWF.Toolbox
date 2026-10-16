#!/usr/bin/env bash
# macOS 命令均由夹具替身实现；不挂载镜像，不访问 Finder 或应用数据。
set -euo pipefail
asset_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
fixture="$(mktemp -d "${TMPDIR:-/tmp}/codewf-brand-test-XXXXXX")"
[[ "$fixture" == */codewf-brand-test-* ]] || exit 1
trap 'rm -rf "$fixture"' EXIT
mkdir -p "$fixture/bin" "$fixture/stage/Test App.app" "$fixture/tmp"
printf '/Applications' >"$fixture/stage/Applications"
cat >"$fixture/bin/hdiutil" <<'STUB'
#!/usr/bin/env bash
set -euo pipefail
printf '%s\n' "$1" >>"$TEST_TRACE"
[[ "$1" == detach || "${FAIL_PHASE:-}" != "$1" ]] || exit 28
case "$1" in
  create) printf 'writable image' >"${!#}" ;;
  attach)
    [[ "$2" == -plist ]] || exit 30
    printf '%s' "${!#}" >"$TEST_IMAGE"
    printf '/dev/disk41' >"$TEST_DEVICE"
    while [[ $# -gt 0 ]]; do
      if [[ "$1" == -mountpoint ]]; then
        mkdir -p "$2"; printf '%s' "$2" >"$TEST_MOUNT"; printf 'retained content' >"$2/fixture-content"
      fi
      shift
    done
    printf 'attach plist' ;;
  detach)
    [[ $# == 2 && "$2" == /dev/disk41 ]] || { echo 'Detach used an unstable mount point' >&2; exit 30; }
    count=0; [[ ! -f "$TEST_DETACH_COUNT" ]] || count="$(cat "$TEST_DETACH_COUNT")"
    count=$((count + 1)); printf '%s' "$count" >"$TEST_DETACH_COUNT"
    [[ -f "$TEST_DEVICE" ]] || { echo 'No such device' >&2; exit 6; }
    if [[ "${DETACH_MODE:-}" == gone ]]; then
      rm -f "$TEST_MOUNT" "$TEST_DEVICE"
      echo 'Resource busy' >&2; exit 28
    fi
    if [[ "${DETACH_MODE:-}" == partial && "$count" == 1 ]]; then
      rm -f "$TEST_MOUNT"
      echo 'Resource busy' >&2; exit 28
    fi
    if [[ "${FAIL_PHASE:-}" == detach || "$count" -le "${DETACH_FAILURES:-0}" ]]; then echo 'Resource busy' >&2; exit 28; fi
    rm -f "$TEST_MOUNT" "$TEST_DEVICE" ;;
  info)
    [[ $# == 2 && "$2" == -plist ]] || exit 30
    [[ "${INFO_MODE:-}" != failure ]] || exit 1
    case "${INFO_MODE:-}" in
      malformed-plist|malformed-json) printf 'invalid'; exit 0 ;;
      malformed-schema) printf '{}'; exit 0 ;;
      malformed-entity) printf '[{"image-path":"/unrelated/image.dmg","system-entities":[{}]}]'; exit 0 ;;
    esac
    if [[ -f "$TEST_DEVICE" ]]; then
      if [[ "${INFO_MODE:-}" == device-only ]]; then image='/another/path.dmg'; else image="$(cat "$TEST_IMAGE")"; fi
      if [[ "${INFO_MODE:-}" == image-only ]]; then device='/dev/disk99'; else device='/dev/disk41'; fi
      printf '[{"image-path":"%s","system-entities":[{"dev-entry":"%s"}]}]' "$image" "$device"
    else
      printf '[{"image-path":"/unrelated/image.dmg","system-entities":[{"dev-entry":"/dev/disk99"}]}]'
    fi ;;
  convert)
    [[ ! -f "$TEST_DEVICE" ]] || { echo 'Converted an attached image' >&2; exit 30; }
    while [[ $# -gt 0 ]]; do if [[ "$1" == -o ]]; then printf 'compressed image' >"$2"; fi; shift; done ;;
  verify) [[ -s "$2" ]] ;;
  *) exit 2 ;;
esac
STUB
cat >"$fixture/bin/plutil" <<'STUB'
#!/usr/bin/env bash
set -euo pipefail
[[ "$1" == -extract && "$4" == -o && "$5" == - && -f "$6" ]] || exit 31
case "$2:$3" in
  system-entities:json)
    [[ "${ATTACH_MODE:-}" != invalid ]] || { printf '[{"dev-entry":"/dev/disk41s1"}]'; exit 0; }
    [[ "${ATTACH_MODE:-}" != malformed ]] || exit 1
    # 整盘实体故意不放首位，验证不会把分区或挂载点当作整盘设备。
    printf '[{"dev-entry":"/dev/disk41s1"},{"dev-entry":"/dev/disk41"}]' ;;
  *) exit 31 ;;
esac
STUB
cat >"$fixture/bin/osascript" <<'STUB'
#!/usr/bin/env bash
set -euo pipefail
if [[ "$1" == -l ]]; then
  [[ "$2" == JavaScript && "$3" == -e && ( $# == 5 || $# == 7 ) ]] || exit 32
  if [[ $# == 7 ]]; then printf 'state\n' >>"$TEST_TRACE"; fi
  if command -v node >/dev/null; then
    if [[ $# == 7 ]]; then
      MSYS2_ARG_CONV_EXCL='*' node -e 'eval(process.argv[1]); process.stdout.write(attachmentState(JSON.parse(process.argv[2]), process.argv[3], process.argv[4]));' "$4" "$(cat "$5")" "$6" "$7"
    else
      MSYS2_ARG_CONV_EXCL='*' node -e 'eval(process.argv[1]); process.stdout.write(run(process.argv.slice(2)));' "$4" "${@:5}"
    fi
  else
    if [[ $# == 5 ]]; then
      [[ "${ATTACH_MODE:-}" != invalid ]] || exit 32
      printf '/dev/disk41'; exit
    fi
    case "${INFO_MODE:-}" in malformed-json|malformed-schema|malformed-entity) exit 32 ;; esac
    if [[ -f "$TEST_DEVICE" ]]; then printf 'attached'; else printf 'detached'; fi
  fi
  exit
fi
printf 'layout\n' >>"$TEST_TRACE"
[[ "${FAIL_PHASE:-}" != layout ]] || exit 29
[[ -f "$1" && "$3" == 'Test App.app' ]]
grep -q '720' "$1" || grep -q '840, 600' "$1"
if [[ "${FAIL_PHASE:-}" != missing-layout ]]; then printf 'Finder metadata' >"$2/.DS_Store"; fi
STUB
cat >"$fixture/bin/sync" <<'STUB'
#!/usr/bin/env bash
exit 0
STUB
cat >"$fixture/bin/sleep" <<'STUB'
#!/usr/bin/env bash
set -euo pipefail
printf 'sleep:%s\n' "$1" >>"$TEST_TRACE"
STUB
chmod +x "$fixture/bin/"*
export PATH="$fixture/bin:$PATH" TMPDIR="$fixture/tmp" TEST_TRACE="$fixture/trace" TEST_MOUNT="$fixture/mounted" TEST_DETACH_COUNT="$fixture/detach-count"
export TEST_IMAGE="$fixture/image" TEST_DEVICE="$fixture/device" TEST_JXA="$fixture/state.js"
reset_fixture() {
  : >"$TEST_TRACE"
  rm -f "$TEST_DETACH_COUNT" "$TEST_MOUNT" "$TEST_DEVICE" "$TEST_IMAGE"
}
for language in bilingual en zh; do
  grep -Fq '>CodeWF</text>' "$asset_root/background-$language.svg"
  grep -Fq 'NATIVE APPLICATION · CodeWF' "$asset_root/background-$language.svg"
  grep -Fq '>codewf.com</text>' "$asset_root/background-$language.svg"
  ! grep -Fq '.NET' "$asset_root/background-$language.svg"
  reset_fixture
  CODEWF_DMG_LANGUAGE="$language" bash "$asset_root/create-branded-dmg.sh" "$fixture/stage" 'Test App' "$fixture/$language.dmg" 'Test volume' >"$fixture/$language.log" 2>&1
  [[ -s "$fixture/$language.dmg" && ! -e "$TEST_MOUNT" && ! -e "$TEST_DEVICE" ]]
  cmp "$asset_root/background-$language.png" "$fixture/stage/.background/codewf.png"
  [[ "$(find "$fixture/stage" -mindepth 1 -maxdepth 1 | wc -l | tr -d '[:space:]')" == 3 ]]
  [[ "$(paste -sd, "$TEST_TRACE")" == 'create,attach,layout,detach,convert,verify' ]]
done
for phase in create attach layout missing-layout convert verify; do
  reset_fixture
  if FAIL_PHASE="$phase" bash "$asset_root/create-branded-dmg.sh" "$fixture/stage" 'Test App' "$fixture/failure-$phase.dmg" >"$fixture/$phase.log" 2>&1; then
    echo "Failure was swallowed: $phase" >&2; exit 1
  fi
  [[ ! -e "$TEST_MOUNT" && ! -e "$TEST_DEVICE" ]] || { echo "Temporary attachment retained after $phase" >&2; exit 1; }
done
# 每次卸载失败都确认设备仍连接，再有限退避。
reset_fixture
DETACH_FAILURES=2 bash "$asset_root/create-branded-dmg.sh" "$fixture/stage" 'Test App' "$fixture/transient.dmg" >"$fixture/transient.log" 2>&1
[[ ! -e "$TEST_DEVICE" && -s "$fixture/transient.dmg" ]]
[[ "$(paste -sd, "$TEST_TRACE")" == 'create,attach,layout,detach,info,state,sleep:1,detach,info,state,sleep:2,detach,convert,verify' ]]
# 卷已卸载但整盘设备仍连接；继续按整盘设备分离。
for mode in normal device-only image-only; do
  reset_fixture
  DETACH_MODE=partial INFO_MODE="$mode" bash "$asset_root/create-branded-dmg.sh" "$fixture/stage" 'Test App' "$fixture/partial-$mode.dmg" >"$fixture/partial-$mode.log" 2>&1
  [[ ! -e "$TEST_DEVICE" && -s "$fixture/partial-$mode.dmg" && "$(cat "$TEST_DETACH_COUNT")" == 2 ]]
  [[ "$(paste -sd, "$TEST_TRACE")" == 'create,attach,layout,detach,info,state,sleep:1,detach,convert,verify' ]]
done
# 命令返回失败，但 info 成功证明本镜像和设备均已消失。
reset_fixture
DETACH_MODE=gone bash "$asset_root/create-branded-dmg.sh" "$fixture/stage" 'Test App' "$fixture/already-detached.dmg" >"$fixture/already-detached.log" 2>&1
[[ ! -e "$TEST_DEVICE" && -s "$fixture/already-detached.dmg" && "$(cat "$TEST_DETACH_COUNT")" == 1 ]]
[[ "$(paste -sd, "$TEST_TRACE")" == 'create,attach,layout,detach,info,state,convert,verify' ]]
# 查询/解析失败不能解释成“已分离”，EXIT 也必须保留证据。
for mode in failure malformed-plist malformed-json malformed-schema malformed-entity; do
  reset_fixture
  if DETACH_MODE=partial INFO_MODE="$mode" bash "$asset_root/create-branded-dmg.sh" "$fixture/stage" 'Test App' "$fixture/unknown-$mode.dmg" >"$fixture/unknown-$mode.log" 2>&1; then exit 1; fi
  [[ -e "$TEST_DEVICE" && ! -e "$TEST_MOUNT" && ! -e "$fixture/unknown-$mode.dmg" && "$(cat "$TEST_DETACH_COUNT")" == 1 ]]
  [[ -s "$(cat "$TEST_IMAGE")" ]] && grep -q 'retained at' "$fixture/unknown-$mode.log"
  ! grep -q '^convert$' "$TEST_TRACE"
done
# attach plist 无法解析或返回非整盘设备也保留临时目录。
for mode in invalid malformed; do
  reset_fixture
  if ATTACH_MODE="$mode" bash "$asset_root/create-branded-dmg.sh" "$fixture/stage" 'Test App' "$fixture/attach-$mode.dmg" >"$fixture/attach-$mode.log" 2>&1; then exit 1; fi
  [[ -e "$TEST_DEVICE" && -s "$(cat "$TEST_IMAGE")" && ! -e "$fixture/attach-$mode.dmg" ]]
  ! grep -q '^convert$' "$TEST_TRACE"
done
# 布局失败的 EXIT 清理可恢复，但不能吞掉原始失败。
reset_fixture
if FAIL_PHASE=layout DETACH_FAILURES=2 bash "$asset_root/create-branded-dmg.sh" "$fixture/stage" 'Test App' "$fixture/cleanup-recovery.dmg" >"$fixture/cleanup-recovery.log" 2>&1; then exit 1; fi
[[ ! -e "$TEST_DEVICE" && ! -e "$fixture/cleanup-recovery.dmg" ]]
[[ "$(paste -sd, "$TEST_TRACE")" == 'create,attach,layout,detach,info,state,sleep:1,detach,info,state,sleep:2,detach' ]]
reset_fixture
if DETACH_FAILURES=5 bash "$asset_root/create-branded-dmg.sh" "$fixture/stage" 'Test App' "$fixture/cleanup-after-exhaustion.dmg" >"$fixture/cleanup-after-exhaustion.log" 2>&1; then exit 1; fi
[[ ! -e "$TEST_DEVICE" && ! -e "$fixture/cleanup-after-exhaustion.dmg" && "$(cat "$TEST_DETACH_COUNT")" == 6 ]]
! grep -q '^convert$' "$TEST_TRACE"
# 永久占用保留镜像与挂载内容，不强卸载、不转换、不删除。
reset_fixture
if FAIL_PHASE=detach bash "$asset_root/create-branded-dmg.sh" "$fixture/stage" 'Test App' "$fixture/failure-detach.dmg" >"$fixture/detach.log" 2>&1; then exit 1; fi
[[ -e "$TEST_DEVICE" && -e "$TEST_MOUNT" ]] && grep -q 'retained at' "$fixture/detach.log"
[[ "$(cat "$TEST_DETACH_COUNT")" == 10 && -s "$(cat "$TEST_MOUNT")/fixture-content" ]]
[[ "$(grep '^sleep:' "$TEST_TRACE" | paste -sd,)" == 'sleep:1,sleep:2,sleep:4,sleep:8,sleep:1,sleep:2,sleep:4,sleep:8' ]]
[[ ! -e "$fixture/failure-detach.dmg" ]]
! grep -q '^convert$' "$TEST_TRACE"
if CODEWF_DMG_LANGUAGE=invalid bash "$asset_root/create-branded-dmg.sh" "$fixture/stage" 'Test App' "$fixture/invalid.dmg" >"$fixture/invalid.log" 2>&1; then exit 1; fi
echo 'PASS: three CodeWF languages, stable device detach, partial volume unmount, already-detached proof, conservative unknown state, bounded busy retries, EXIT cleanup and failure propagation.'
