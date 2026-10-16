[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$StagingDirectory,
    [Parameter(Mandatory)][string]$RepositoryRoot,
    [Parameter(Mandatory)][string]$Version
)
$ErrorActionPreference = 'Stop'
$encoding = [Text.UTF8Encoding]::new($false)
$metadata = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'product.json') -Raw | ConvertFrom-Json
$stage = (Resolve-Path -LiteralPath $StagingDirectory).Path
$desktopPath = Join-Path $stage "usr/share/applications/$($metadata.command).desktop"
$controlPath = Join-Path $stage 'DEBIAN/control'
foreach ($required in @($desktopPath, $controlPath, (Join-Path $RepositoryRoot 'LICENSE'), (Join-Path $RepositoryRoot 'logo.png'))) {
    if (-not (Test-Path -LiteralPath $required -PathType Leaf)) { throw "Linux 安装元数据缺少输入：$required" }
}

# 保留产品图标；标准图标主题目录供应用菜单和软件管理器读取。
$png = [IO.File]::ReadAllBytes((Join-Path $RepositoryRoot 'logo.png'))
if ($png.Length -lt 24 -or [Convert]::ToHexString($png[0..7]) -ne '89504E470D0A1A0A') { throw '产品图标不是有效的 PNG 文件。' }
$width = ([int]$png[16] -shl 24) -bor ([int]$png[17] -shl 16) -bor ([int]$png[18] -shl 8) -bor [int]$png[19]
$height = ([int]$png[20] -shl 24) -bor ([int]$png[21] -shl 16) -bor ([int]$png[22] -shl 8) -bor [int]$png[23]
if ($width -ne $height -or $width -le 0) { throw 'Linux 应用图标应为正方形。' }
$iconDirectory = Join-Path $stage "usr/share/icons/hicolor/${width}x${height}/apps"
New-Item -ItemType Directory -Path $iconDirectory -Force | Out-Null
[IO.File]::WriteAllBytes((Join-Path $iconDirectory ($metadata.command + '.png')), $png)
$svg = Join-Path $RepositoryRoot 'logo.svg'
if (Test-Path -LiteralPath $svg -PathType Leaf) {
    $scalable = Join-Path $stage 'usr/share/icons/hicolor/scalable/apps'
    New-Item -ItemType Directory -Path $scalable -Force | Out-Null
    Copy-Item -LiteralPath $svg -Destination (Join-Path $scalable ($metadata.command + '.svg')) -Force
}
$pixmaps = Join-Path $stage 'usr/share/pixmaps'
New-Item -ItemType Directory -Path $pixmaps -Force | Out-Null
[IO.File]::WriteAllBytes((Join-Path $pixmaps ($metadata.command + '.png')), $png)

# 外观由发行版软件管理器决定；品牌色仅通过 AppStream 标准元数据提供。
function Escape-Xml([string]$Value) { [Security.SecurityElement]::Escape($Value) }
$name = Escape-Xml $metadata.name
$chineseName = Escape-Xml $metadata.chineseName
$summary = Escape-Xml $metadata.summary
$chineseSummary = Escape-Xml $metadata.chineseSummary
$description = Escape-Xml $metadata.description
$chineseDescription = Escape-Xml $metadata.chineseDescription
$homepage = Escape-Xml $metadata.homepage
$categories = ($metadata.categories | ForEach-Object { '<category>' + (Escape-Xml $_) + '</category>' }) -join ''
$appstream = @"
<?xml version="1.0" encoding="utf-8"?>
<component type="desktop-application">
  <id>$($metadata.id)</id>
  <metadata_license>CC0-1.0</metadata_license>
  <project_license>MIT</project_license>
  <name>$name</name>
  <name xml:lang="zh-CN">$chineseName</name>
  <summary>$summary</summary>
  <summary xml:lang="zh-CN">$chineseSummary</summary>
  <description><p>$description</p><p xml:lang="zh-CN">$chineseDescription</p></description>
  <developer id="com.codewf"><name>Dotnet9</name></developer>
  <url type="homepage">$homepage</url>
  <url type="bugtracker">$homepage/issues</url>
  <launchable type="desktop-id">$($metadata.command).desktop</launchable>
  <icon type="stock">$($metadata.command)</icon>
  <categories>$categories</categories>
  <provides><binary>$($metadata.command)</binary></provides>
  <branding><color type="primary" scheme_preference="light">#512BD4</color><color type="primary" scheme_preference="dark">#512BD4</color></branding>
</component>
"@
$metainfo = Join-Path $stage 'usr/share/metainfo'
New-Item -ItemType Directory -Path $metainfo -Force | Out-Null
[IO.File]::WriteAllText((Join-Path $metainfo ($metadata.id + '.metainfo.xml')), $appstream.ReplaceLineEndings("`n") + "`n", $encoding)

$desktop = [IO.File]::ReadAllText($desktopPath).ReplaceLineEndings("`n").TrimEnd()
foreach ($entry in @("Name[zh_CN]=$($metadata.chineseName)", "Comment[zh_CN]=$($metadata.chineseSummary)", "TryExec=$($metadata.command)", "Icon=$($metadata.command)", "X-AppStream-Id=$($metadata.id)")) {
    $key = $entry.Substring(0, $entry.IndexOf('='))
    $desktop = [regex]::Replace($desktop, '(?m)^' + [regex]::Escape($key) + '=.*\n?', '')
    $desktop += "`n" + $entry
}
[IO.File]::WriteAllText($desktopPath, $desktop + "`n", $encoding)

$documentation = Join-Path $stage "usr/share/doc/$($metadata.package)"
New-Item -ItemType Directory -Path $documentation -Force | Out-Null
$license = [IO.File]::ReadAllText((Join-Path $RepositoryRoot 'LICENSE')).ReplaceLineEndings("`n")
[IO.File]::WriteAllText((Join-Path $documentation 'copyright'), "Upstream-Name: $($metadata.name)`nSource: $($metadata.homepage)`nLicense: MIT`n`n" + $license, $encoding)
$control = [IO.File]::ReadAllText($controlPath).ReplaceLineEndings("`n").TrimEnd()
$control = [regex]::Replace($control, '(?m)^Homepage:.*\n?', '') + "`nHomepage: $($metadata.homepage)"
$installedSize = [Math]::Ceiling(((Get-ChildItem -LiteralPath (Join-Path $stage 'usr') -Recurse -File | Measure-Object Length -Sum).Sum) / 1KB)
$control = [regex]::Replace($control, '(?m)^Installed-Size:.*\n?', '') + "`nInstalled-Size: $installedSize"
[IO.File]::WriteAllText($controlPath, $control + "`n", $encoding)
if ($IsLinux) {
    & chmod 644 $controlPath $desktopPath
    if ($LASTEXITCODE -ne 0) { throw '无法设置 Linux 安装元数据权限。' }
}
