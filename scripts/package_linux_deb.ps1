# 打包 Linux .deb 安装包（dpkg-deb，在 ubuntu runner 上运行）。
# 用法：pwsh scripts/package_linux_deb.ps1 -RuntimeIdentifier linux-x64 -Version 1.2.3
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("linux-x64", "linux-arm64")]
    [string] $RuntimeIdentifier,

    [Parameter(Mandatory = $true)]
    [string] $Version,

    [string] $SourceDirectory = "",
    [string] $OutputDirectory = "",
    [switch] $Force
)

$ErrorActionPreference = "Stop"
$scriptRoot = (Resolve-Path -LiteralPath $PSScriptRoot).Path
$repositoryRoot = (Resolve-Path -LiteralPath (Join-Path $scriptRoot "..")).Path

if ([string]::IsNullOrWhiteSpace($SourceDirectory)) {
    $SourceDirectory = Join-Path $repositoryRoot "publish/$RuntimeIdentifier"
}
if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = Join-Path $repositoryRoot "artifacts/release"
}

if (-not (Test-Path -LiteralPath $SourceDirectory)) {
    throw "Publish output was not produced: $SourceDirectory"
}

New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$architecture = if ($RuntimeIdentifier -eq "linux-arm64") { "arm64" } else { "amd64" }
$packageName = "CodeWF.Toolbox-v$Version-$RuntimeIdentifier.deb"
$outputPath = Join-Path $OutputDirectory $packageName
if ((Test-Path -LiteralPath $outputPath) -and -not $Force) {
    throw "Output already exists (use -Force to overwrite): $outputPath"
}

# 每次打包使用唯一临时目录，只清理当前任务创建的暂存文件。
$staging = Join-Path ([IO.Path]::GetTempPath()) ('CodeWF.Toolbox-deb-' + [Guid]::NewGuid().ToString('N'))
try {
New-Item -ItemType Directory -Path (Join-Path $staging "DEBIAN") -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $staging "usr/local/share/CodeWF.Toolbox") -Force | Out-Null

# 调试符号保留在构建产物，安装包保留运行文件和宿主语言资源。
$sourcePath = (Resolve-Path -LiteralPath $SourceDirectory).Path
$appPath = Join-Path $staging 'usr/local/share/CodeWF.Toolbox'
if (-not (Test-Path -LiteralPath (Join-Path $sourcePath 'CodeWF.Toolbox') -PathType Leaf)) { throw '发布目录缺少主程序。' }
Get-ChildItem -LiteralPath $sourcePath -Recurse -File -Force | ForEach-Object {
        $relative = [IO.Path]::GetRelativePath($sourcePath, $_.FullName)
        if ($_.Extension -in '.pdb', '.dbg' -or $relative -match '(^|[\\/])[^\\/]+\.dSYM([\\/]|$)') { return }
        $destination = Join-Path $appPath $relative
        New-Item -ItemType Directory -Path (Split-Path -Parent $destination) -Force | Out-Null
        Copy-Item -LiteralPath $_.FullName -Destination $destination -Force
    }

$appDirectory = $appPath
$launcherDirectory = Join-Path $staging 'usr/local/bin'
New-Item -ItemType Directory -Path $launcherDirectory -Force | Out-Null
$launcherPath = Join-Path $launcherDirectory 'codewf-toolbox'
[IO.File]::WriteAllText($launcherPath, "#!/bin/sh`n" + 'exec /usr/local/share/CodeWF.Toolbox/CodeWF.Toolbox "$@"' + "`n")
$applications = Join-Path $staging 'usr/share/applications'
$icons = Join-Path $staging 'usr/share/icons/hicolor/512x512/apps'
New-Item -ItemType Directory -Path $applications, $icons -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $repositoryRoot 'logo.png') -Destination (Join-Path $icons 'codewf-toolbox.png')
$desktop = @'
[Desktop Entry]
Type=Application
Name=CodeWF Toolbox
Comment=Developer tools with independent documents
Exec=codewf-toolbox
Icon=codewf-toolbox
Terminal=false
Categories=Development;Utility;
StartupWMClass=CodeWF.Toolbox
'@
Set-Content -LiteralPath (Join-Path $applications 'codewf-toolbox.desktop') -Value $desktop -Encoding utf8

# control（dpkg-deb 要求 DEBIAN/control 为 root 所属、644）
$installedSizeKb = [Math]::Ceiling(
    ((Get-ChildItem -LiteralPath $SourceDirectory -Recurse -File | Measure-Object Length -Sum).Sum) / 1KB)
$control = @"
Package: codewf-toolbox
Version: $Version
Architecture: $architecture
Maintainer: Dotnet9 <dotnet9.com@gmail.com>
Installed-Size: $installedSizeKb
Depends: libfontconfig1, libx11-6
Section: utils
Priority: optional
Description: 码匠工具箱：面向开发者日常效率场景的模块化桌面工具箱
"@
Set-Content -LiteralPath (Join-Path $staging "DEBIAN/control") -Value ($control + [Environment]::NewLine) -Encoding UTF8

# dpkg-deb 需要 posix 权限语义；control 文件权限固定 644
if ($IsLinux) {
    $null = chmod 644 (Join-Path $staging "DEBIAN/control")
    $null = chmod 644 (Join-Path $applications 'codewf-toolbox.desktop')
    $null = chmod 755 $launcherPath (Join-Path $appDirectory 'CodeWF.Toolbox')
    if ($LASTEXITCODE -ne 0) { throw 'Unable to mark Linux executable.' }
}

# 补齐独立安装包的图标、品牌和软件管理器元数据。

& (Join-Path $scriptRoot 'installer/linux/Write-LinuxPackageMetadata.ps1') -StagingDirectory $staging -RepositoryRoot $repositoryRoot -Version $Version

dpkg-deb --build --root-owner-group $staging $outputPath
if ($LASTEXITCODE -ne 0) {
    throw "dpkg-deb failed for $RuntimeIdentifier"
}
if (-not (Test-Path -LiteralPath $outputPath -PathType Leaf)) {
    throw "Deb was not produced: $outputPath"
}

$sha = (Get-FileHash -LiteralPath $outputPath -Algorithm SHA256).Hash.ToLowerInvariant()
"$sha  $packageName" | Set-Content -LiteralPath "$outputPath.sha256" -Encoding ASCII


Write-Host "Deb 打包完成：$outputPath"

} finally {
    $resolved = [IO.Path]::GetFullPath($staging)
    $temporary = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
    if (-not $resolved.StartsWith($temporary, [StringComparison]::OrdinalIgnoreCase)) { throw '拒绝清理临时目录之外的路径。' }
    if (Test-Path -LiteralPath $resolved) { Remove-Item -LiteralPath $resolved -Recurse -Force }
}
