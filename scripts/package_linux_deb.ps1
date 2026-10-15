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

$staging = Join-Path $repositoryRoot "artifacts/deb/$RuntimeIdentifier/codewf-toolbox"
if (Test-Path -LiteralPath $staging) {
    Remove-Item -LiteralPath $staging -Recurse -Force
}
New-Item -ItemType Directory -Path (Join-Path $staging "DEBIAN") -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $staging "usr/local/share/CodeWF.Toolbox") -Force | Out-Null

# 程序文件
Copy-Item -Path (Join-Path $SourceDirectory "*") -Destination (Join-Path $staging "usr/local/share/CodeWF.Toolbox/") -Recurse -Force

$appDirectory = Join-Path $staging 'usr/local/share/CodeWF.Toolbox'
Get-ChildItem -LiteralPath $appDirectory -Recurse -File |
    Where-Object { $_.Extension -in '.pdb', '.dbg', '.xml' } | Remove-Item -Force
$applications = Join-Path $staging 'usr/share/applications'
$icons = Join-Path $staging 'usr/share/icons/hicolor/256x256/apps'
New-Item -ItemType Directory -Path $applications, $icons -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $repositoryRoot 'logo.png') -Destination (Join-Path $icons 'codewf-toolbox.png')
$desktop = @'
[Desktop Entry]
Type=Application
Name=CodeWF Toolbox
Comment=Developer tools with independent documents
Exec=/usr/local/share/CodeWF.Toolbox/CodeWF.Toolbox
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
    $null = chmod +x (Join-Path $appDirectory 'CodeWF.Toolbox')
    if ($LASTEXITCODE -ne 0) { throw 'Unable to mark Linux executable.' }
}

dpkg-deb --build --root-owner-group $staging $outputPath
if ($LASTEXITCODE -ne 0) {
    throw "dpkg-deb failed for $RuntimeIdentifier"
}
if (-not (Test-Path -LiteralPath $outputPath -PathType Leaf)) {
    throw "Deb was not produced: $outputPath"
}

$sha = (Get-FileHash -LiteralPath $outputPath -Algorithm SHA256).Hash.ToLowerInvariant()
"$sha  $packageName" | Set-Content -LiteralPath "$outputPath.sha256" -Encoding ASCII

Remove-Item -LiteralPath $staging -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "Deb 打包完成：$outputPath"
