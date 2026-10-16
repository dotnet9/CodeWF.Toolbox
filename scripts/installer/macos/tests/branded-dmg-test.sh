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
[[ "${FAIL_PHASE:-}" != "$1" ]] || exit 28
case "$1" in
  create) printf 'writable image' >"${!#}" ;;
  attach) while [[ $# -gt 0 ]]; do if [[ "$1" == -mountpoint ]]; then mkdir -p "$2"; printf 'mounted' >"$TEST_MOUNT"; fi; shift; done ;;
  detach) rm -f "$TEST_MOUNT" ;;
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
chmod +x "$fixture/bin/"*
export PATH="$fixture/bin:$PATH" TMPDIR="$fixture/tmp" TEST_TRACE="$fixture/trace" TEST_MOUNT="$fixture/mounted"
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
export FAIL_PHASE=detach
if bash "$asset_root/create-branded-dmg.sh" "$fixture/stage" 'Test App' "$fixture/failure-detach.dmg" >"$fixture/detach.log" 2>&1; then exit 1; fi
[[ -e "$TEST_MOUNT" ]] && grep -q 'retained at' "$fixture/detach.log"
rm -f "$TEST_MOUNT"
unset FAIL_PHASE
if CODEWF_DMG_LANGUAGE=invalid bash "$asset_root/create-branded-dmg.sh" "$fixture/stage" 'Test App' "$fixture/invalid.dmg" >"$fixture/invalid.log" 2>&1; then exit 1; fi
echo 'PASS: three languages, real-layout command arguments, mounted cleanup and create/attach/layout/metadata/detach/convert/verify failure propagation.'
