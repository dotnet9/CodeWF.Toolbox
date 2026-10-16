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
  attach) while [[ $# -gt 0 ]]; do if [[ "$1" == -mountpoint ]]; then mkdir -p "$2"; printf '%s' "$2" >"$TEST_MOUNT"; printf 'retained content' >"$2/fixture-content"; fi; shift; done ;;
  detach)
    [[ $# == 2 && "$2" == "$(cat "$TEST_MOUNT")" ]] || exit 30
    count=0; [[ ! -f "$TEST_DETACH_COUNT" ]] || count="$(cat "$TEST_DETACH_COUNT")"
    count=$((count + 1)); printf '%s' "$count" >"$TEST_DETACH_COUNT"
    if [[ "${FAIL_PHASE:-}" == detach || "$count" -le "${DETACH_FAILURES:-0}" ]]; then echo 'Resource busy' >&2; exit 28; fi
    rm -f "$TEST_MOUNT" ;;
  convert) while [[ $# -gt 0 ]]; do if [[ "$1" == -o ]]; then printf 'compressed image' >"$2"; fi; shift; done ;;
  verify) [[ -s "$2" ]] ;;
  *) exit 2 ;;
esac
STUB
cat >"$fixture/bin/osascript" <<'STUB'
#!/usr/bin/env bash
set -euo pipefail
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
for language in bilingual en zh; do
  : >"$TEST_TRACE"
  CODEWF_DMG_LANGUAGE="$language" bash "$asset_root/create-branded-dmg.sh" "$fixture/stage" 'Test App' "$fixture/$language.dmg" 'Test volume' >"$fixture/$language.log" 2>&1
  [[ -s "$fixture/$language.dmg" && ! -e "$TEST_MOUNT" ]]
  cmp "$asset_root/background-$language.png" "$fixture/stage/.background/codewf.png"
  [[ "$(paste -sd, "$TEST_TRACE")" == 'create,attach,layout,detach,convert,verify' ]]
done
for phase in create attach layout missing-layout convert verify; do
  export FAIL_PHASE="$phase"
  if bash "$asset_root/create-branded-dmg.sh" "$fixture/stage" 'Test App' "$fixture/failure-$phase.dmg" >"$fixture/$phase.log" 2>&1; then
    echo "Failure was swallowed: $phase" >&2; exit 1
  fi
  [[ ! -e "$TEST_MOUNT" ]] || { echo "Temporary mount retained after $phase" >&2; exit 1; }
done
unset FAIL_PHASE
# 正常卸载遇到短暂占用，重试成功后才允许转换；替身 sleep 不真的等待。
: >"$TEST_TRACE"; rm -f "$TEST_DETACH_COUNT"
DETACH_FAILURES=2 bash "$asset_root/create-branded-dmg.sh" "$fixture/stage" 'Test App' "$fixture/transient.dmg" >"$fixture/transient.log" 2>&1
[[ ! -e "$TEST_MOUNT" && -s "$fixture/transient.dmg" ]]
[[ "$(paste -sd, "$TEST_TRACE")" == 'create,attach,layout,detach,sleep:1,detach,sleep:2,detach,convert,verify' ]]
# 布局失败时由 EXIT 清理重试；清理恢复也不能吞掉原始失败或继续转换。
: >"$TEST_TRACE"; rm -f "$TEST_DETACH_COUNT"
if FAIL_PHASE=layout DETACH_FAILURES=2 bash "$asset_root/create-branded-dmg.sh" "$fixture/stage" 'Test App' "$fixture/cleanup-recovery.dmg" >"$fixture/cleanup-recovery.log" 2>&1; then exit 1; fi
[[ ! -e "$TEST_MOUNT" && ! -e "$fixture/cleanup-recovery.dmg" ]]
[[ "$(paste -sd, "$TEST_TRACE")" == 'create,attach,layout,detach,sleep:1,detach,sleep:2,detach' ]]
# 正常阶段耗尽五次后，EXIT 可再恢复挂载；本轮仍失败且不能转换。
: >"$TEST_TRACE"; rm -f "$TEST_DETACH_COUNT"
if DETACH_FAILURES=5 bash "$asset_root/create-branded-dmg.sh" "$fixture/stage" 'Test App' "$fixture/cleanup-after-exhaustion.dmg" >"$fixture/cleanup-after-exhaustion.log" 2>&1; then exit 1; fi
[[ ! -e "$TEST_MOUNT" && ! -e "$fixture/cleanup-after-exhaustion.dmg" && "$(cat "$TEST_DETACH_COUNT")" == 6 ]]
! grep -q '^convert$' "$TEST_TRACE"
# 永久占用时，正常与 EXIT 各最多五次；不得转换、强卸载或删掉挂载内容。
: >"$TEST_TRACE"; rm -f "$TEST_DETACH_COUNT"
export FAIL_PHASE=detach
if bash "$asset_root/create-branded-dmg.sh" "$fixture/stage" 'Test App' "$fixture/failure-detach.dmg" >"$fixture/detach.log" 2>&1; then exit 1; fi
[[ -e "$TEST_MOUNT" ]] && grep -q 'retained at' "$fixture/detach.log"
[[ "$(cat "$TEST_DETACH_COUNT")" == 10 && -s "$(cat "$TEST_MOUNT")/fixture-content" ]]
[[ "$(grep '^sleep:' "$TEST_TRACE" | paste -sd,)" == 'sleep:1,sleep:2,sleep:4,sleep:8,sleep:1,sleep:2,sleep:4,sleep:8' ]]
[[ ! -e "$fixture/failure-detach.dmg" ]]
! grep -q '^convert$' "$TEST_TRACE"
rm -f "$TEST_MOUNT"
unset FAIL_PHASE
if CODEWF_DMG_LANGUAGE=invalid bash "$asset_root/create-branded-dmg.sh" "$fixture/stage" 'Test App' "$fixture/invalid.dmg" >"$fixture/invalid.log" 2>&1; then exit 1; fi
echo 'PASS: three languages, bounded detach backoff/recovery, EXIT cleanup, permanent mount retention/no conversion and create/attach/layout/metadata/convert/verify failure propagation.'
