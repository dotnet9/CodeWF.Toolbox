# 在打包边界检查暂存结构；Linux 同时调用真实 dpkg-deb，Windows 仅验证受控边界。
$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$fixtureRoot = Join-Path ([IO.Path]::GetTempPath()) ('CodeWF.Toolbox-deb-test-' + [Guid]::NewGuid().ToString('N'))
$source = Join-Path $fixtureRoot 'source with spaces'
$output = Join-Path $fixtureRoot 'output'
$nativeDpkg = Get-Command dpkg-deb -CommandType Application -ErrorAction SilentlyContinue | Select-Object -First 1
$global:CodeWFDebFixture = @{
    App = 'CodeWF.Toolbox'; Command = 'codewf-toolbox'; Executable = 'CodeWF.Toolbox'; AppDirectory = 'usr/local/share/CodeWF.Toolbox'
    BinDirectory = 'usr/local/bin'; Icon = 'usr/share/pixmaps/codewf-toolbox.png'; NativeDpkg = $nativeDpkg; Fail = $false; Inspections = 0
}
function global:dpkg-deb {
    $context = $global:CodeWFDebFixture
    if ($context.Fail) { $global:LASTEXITCODE = 31; return }
    if ($args.Count -ne 4 -or $args[0] -ne '--build' -or $args[1] -ne '--root-owner-group') { throw '打包参数错误。' }
    $stage = $args[2]
    foreach ($entry in @(
        'DEBIAN/control', "$($context.AppDirectory)/$($context.Executable)",
        "$($context.AppDirectory)/I18n/en-US.json", "$($context.BinDirectory)/$($context.Command)",
        "usr/share/applications/$($context.Command).desktop", $context.Icon
    )) {
        if (-not (Test-Path -LiteralPath (Join-Path $stage $entry) -PathType Leaf)) { throw "缺少安装入口或资源：$entry" }
    }
    if (@(Get-ChildItem -LiteralPath $stage -Recurse -File | Where-Object {
        $_.Extension -in '.pdb', '.dbg' -or $_.FullName -match '\.dSYM[\\/]'
    }).Count) { throw '安装器包含调试符号。' }
    $launcherPath = Join-Path $stage "$($context.BinDirectory)/$($context.Command)"
    $launcher = [IO.File]::ReadAllText($launcherPath)
    if (-not $launcher.Contains('"$@"') -or $launcher.Contains([char]13)) { throw '启动参数或 LF 换行错误。' }
    $desktop = [IO.File]::ReadAllText((Join-Path $stage "usr/share/applications/$($context.Command).desktop"))
    if (-not $desktop.Contains("Icon=$($context.Command)") -or -not $desktop.Contains("Exec=$($context.Command)")) { throw '桌面菜单未绑定入口或图标。' }
    $metadata = Get-Content -LiteralPath (Join-Path $repositoryRoot 'scripts/installer/linux/product.json') -Raw | ConvertFrom-Json
    foreach ($entry in @("usr/share/metainfo/$($metadata.id).metainfo.xml", "usr/share/doc/$($metadata.package)/copyright")) {
        if (-not (Test-Path -LiteralPath (Join-Path $stage $entry) -PathType Leaf)) { throw "缺少软件管理器元数据或许可：$entry" }
    }
    if (-not $desktop.Contains("Name[zh_CN]=$($metadata.chineseName)") -or -not $desktop.Contains("TryExec=$($metadata.command)")) { throw '本地化桌面入口没有同步。' }
    $control = [IO.File]::ReadAllText((Join-Path $stage 'DEBIAN/control'))
    $actualSize = [Math]::Ceiling(((Get-ChildItem -LiteralPath (Join-Path $stage 'usr') -Recurse -File | Measure-Object Length -Sum).Sum) / 1KB)
    if (-not $control.Contains("Installed-Size: $actualSize")) { throw '安装体积没有按实际运行文件计算。' }
    if ($IsLinux) {
        $mode = [IO.File]::GetUnixFileMode($launcherPath)
        if (-not ($mode -band [IO.UnixFileMode]::UserExecute)) { throw '启动入口没有执行权限。' }
    }
    $context.Inspections++
    if ($context.NativeDpkg) { & $context.NativeDpkg.Source @args }
    else { [IO.File]::WriteAllText($args[3], '受控打包边界夹具，不是真实 DEB。'); $global:LASTEXITCODE = 0 }
}
try {
    New-Item -ItemType Directory -Path (Join-Path $source 'I18n'), (Join-Path $source 'fixture.dSYM/Contents') -Force | Out-Null
    [IO.File]::WriteAllText((Join-Path $source 'CodeWF.Toolbox'), '#!/bin/sh' + [char]10 + 'exit 0' + [char]10)
    [IO.File]::WriteAllText((Join-Path $source 'I18n/en-US.json'), '{}')
    foreach ($symbol in 'fixture.pdb', 'fixture.dbg', 'fixture.dSYM/Contents/symbol') { [IO.File]::WriteAllText((Join-Path $source $symbol), 'symbols') }
    foreach ($rid in 'linux-x64', 'linux-arm64') {
        & (Join-Path $repositoryRoot 'scripts/package_linux_deb.ps1') -RuntimeIdentifier $rid -Version 1.2.3 -SourceDirectory $source -OutputDirectory $output -Force
        $package = Join-Path $output "CodeWF.Toolbox-v1.2.3-$rid.deb"
        $expected = (Get-FileHash -LiteralPath $package -Algorithm SHA256).Hash.ToLowerInvariant()
        if (-not ([IO.File]::ReadAllText("$package.sha256")).StartsWith("$expected  ")) { throw '安装包校验和错误。' }
    }
    $global:CodeWFDebFixture.Fail = $true
    $failed = $false
    try { & (Join-Path $repositoryRoot 'scripts/package_linux_deb.ps1') -RuntimeIdentifier linux-x64 -Version 1.2.3 -SourceDirectory $source -OutputDirectory $output -Force }
    catch { $failed = $_.Exception.Message -match 'dpkg-deb' }
    if (-not $failed) { throw 'dpkg-deb 的失败未传递给发布任务。' }
    if ($global:CodeWFDebFixture.Inspections -ne 2) { throw '两个 Linux 架构的安装结构均应验证。' }
    # 负向夹具已确认失败传播，避免 CI 把其预期退出码当作整个测试的结果。
    $global:LASTEXITCODE = 0
    Write-Host 'PASS：Linux 双架构安装入口、图标、语言资源、符号剔除、体积、校验和与失败传播。'
} finally {
    Remove-Item Function:/dpkg-deb
    Remove-Variable -Name CodeWFDebFixture -Scope Global
    $resolvedFixture = [IO.Path]::GetFullPath($fixtureRoot)
    $resolvedTemp = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
    if (-not $resolvedFixture.StartsWith($resolvedTemp, [StringComparison]::OrdinalIgnoreCase)) { throw '拒绝清理临时目录之外的路径。' }
    if (Test-Path -LiteralPath $resolvedFixture) { Remove-Item -LiteralPath $resolvedFixture -Recurse -Force }
}
