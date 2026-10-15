[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $Version,
    [string] $PackageDirectory = ''
)
$ErrorActionPreference = 'Stop'
if (-not $PackageDirectory) { $PackageDirectory = Join-Path (Split-Path -Parent $PSScriptRoot) 'artifacts/release' }
$directory = (Resolve-Path -LiteralPath $PackageDirectory).Path
$files = @(Get-ChildItem -LiteralPath $directory -File)
$expected = @{
    'win-x64' = '.exe'
    'linux-x64' = '.deb'
    'linux-arm64' = '.deb'
    'osx-x64' = '.dmg'
    'osx-arm64' = '.dmg'
}
$allowed = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($rid in $expected.Keys) {
    $packages = @($files | Where-Object {
        $_.Extension -eq $expected[$rid] -and $_.Name.Contains($rid, [StringComparison]::Ordinal) -and
        $_.Name -match ('(?<![0-9])' + [regex]::Escape($Version.TrimStart('v')) + '(?![0-9])')
    })
    if ($packages.Count -ne 1) { throw "平台 $rid 必须有且只有一个对应版本的安装包，实际数量：$($packages.Count)" }
    $package = $packages[0]
    if ($package.Length -eq 0) { throw "安装包为空：$($package.Name)" }
    $checksumPath = $package.FullName + '.sha256'
    $checksum = (Get-Content -LiteralPath $checksumPath -Raw).Trim()
    if ($checksum -notmatch ('^([a-fA-F0-9]{64})\s+\*?' + [regex]::Escape($package.Name) + '$')) {
        throw "校验文件格式或目标名称错误：$($package.Name)"
    }
    $expectedHash = $Matches[1]
    if ((Get-FileHash -LiteralPath $package.FullName -Algorithm SHA256).Hash -ne $expectedHash) {
        throw "安装包 SHA-256 不匹配：$($package.Name)"
    }
    [void] $allowed.Add($package.Name)
    [void] $allowed.Add($package.Name + '.sha256')
}
$unexpected = @($files | Where-Object { -not $allowed.Contains($_.Name) })
if ($unexpected.Count -gt 0) { throw "发布目录只允许五平台安装包与校验文件，发现额外文件：$($unexpected.Name -join ', ')" }
Write-Host '五平台安装包、版本及 SHA-256 校验通过；无便携压缩包。'
