$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$directory = Join-Path $repositoryRoot 'scripts/installer/linux'
$metadata = Get-Content -LiteralPath (Join-Path $directory 'product.json') -Raw | ConvertFrom-Json
$source = Get-Content -LiteralPath (Join-Path $directory 'source.json') -Raw | ConvertFrom-Json
if ((Get-FileHash -LiteralPath (Join-Path $directory 'Write-LinuxPackageMetadata.ps1') -Algorithm SHA256).Hash -ne $source.sha256) { throw 'Linux 元数据脚本与主源校验值不同。' }
$fixture = Join-Path ([IO.Path]::GetTempPath()) ('codewf-linux-metadata-' + [Guid]::NewGuid().ToString('N'))
$encoding = [Text.UTF8Encoding]::new($false)
try {
    $desktop = Join-Path $fixture "usr/share/applications/$($metadata.command).desktop"
    $control = Join-Path $fixture 'DEBIAN/control'
    foreach ($folder in @((Split-Path -Parent $desktop), (Split-Path -Parent $control))) {
        New-Item -ItemType Directory -Path $folder -Force | Out-Null
    }
    [IO.File]::WriteAllText($desktop, "[Desktop Entry]`nType=Application`nName=$($metadata.name)`nExec=$($metadata.command)`nTerminal=false`n", $encoding)
    [IO.File]::WriteAllText($control, "Package: $($metadata.package)`nVersion: 1.2.3`nArchitecture: amd64`nDescription: metadata fixture`n", $encoding)
    & (Join-Path $directory 'Write-LinuxPackageMetadata.ps1') -StagingDirectory $fixture -RepositoryRoot $repositoryRoot -Version 1.2.3
    $file = Join-Path $fixture "usr/share/metainfo/$($metadata.id).metainfo.xml"
    [xml]$xml = [IO.File]::ReadAllText($file)
    $manager = [Xml.XmlNamespaceManager]::new($xml.NameTable)
    $manager.AddNamespace('xml', 'http://www.w3.org/XML/1998/namespace')
    if ($xml.component.id -ne $metadata.id -or $xml.component.project_license -ne 'MIT') { throw 'AppStream 产品或许可错误。' }
    if ($xml.component.launchable.'#text' -ne ($metadata.command + '.desktop')) { throw 'AppStream 未绑定桌面入口。' }
    if (-not $xml.SelectSingleNode('/component/summary[@xml:lang="zh-CN"]', $manager)) { throw 'AppStream 缺少中文说明。' }
    if (@($xml.component.branding.color | Where-Object { $_.'#text' -eq '#512BD4' }).Count -ne 2) { throw '.NET 紫品牌色没有覆盖明暗两种偏好。' }
    if ($xml.component.screenshots) { throw '不能引用尚未发布的宣传截图。' }
    $png = Get-ChildItem -LiteralPath (Join-Path $fixture 'usr/share/icons/hicolor') -Recurse -File -Filter ($metadata.command + '.png') | Select-Object -First 1
    if (-not $png -or (Get-FileHash -LiteralPath $png.FullName).Hash -ne (Get-FileHash -LiteralPath (Join-Path $repositoryRoot 'logo.png')).Hash) { throw 'hicolor 图标与产品图标不同。' }
    $pixmap = Join-Path $fixture "usr/share/pixmaps/$($metadata.command).png"
    if ((Get-FileHash -LiteralPath $pixmap).Hash -ne (Get-FileHash -LiteralPath $png.FullName).Hash) { throw '备用图标不一致。' }
    if (Test-Path -LiteralPath (Join-Path $repositoryRoot 'logo.svg')) {
        $svg = Join-Path $fixture "usr/share/icons/hicolor/scalable/apps/$($metadata.command).svg"
        if ((Get-FileHash -LiteralPath $svg).Hash -ne (Get-FileHash -LiteralPath (Join-Path $repositoryRoot 'logo.svg')).Hash) { throw '可缩放图标与产品图标不同。' }
    }
    $desktopText = [IO.File]::ReadAllText($desktop)
    foreach ($value in @("Name[zh_CN]=$($metadata.chineseName)", "TryExec=$($metadata.command)", "Icon=$($metadata.command)")) {
        if (-not $desktopText.Contains($value)) { throw "桌面入口缺少：$value" }
    }
    $copyright = [IO.File]::ReadAllText((Join-Path $fixture "usr/share/doc/$($metadata.package)/copyright"))
    if (-not $copyright.Contains('MIT License')) { throw '没有保留实际 MIT 许可正文。' }
    $controlText = [IO.File]::ReadAllText($control)
    $size = [Math]::Ceiling(((Get-ChildItem -LiteralPath (Join-Path $fixture 'usr') -Recurse -File | Measure-Object Length -Sum).Sum) / 1KB)
    if (-not $controlText.Contains("Homepage: $($metadata.homepage)") -or -not $controlText.Contains("Installed-Size: $size")) { throw 'Homepage 或实际安装体积错误。' }
    $validation = Get-Command appstreamcli -CommandType Application -ErrorAction SilentlyContinue
    if ($validation) {
        & $validation.Source validate --no-net $file
        if ($LASTEXITCODE -ne 0) { throw 'AppStream 原生校验失败。' }
    }
    Write-Host "PASS $($metadata.name)：AppStream、中英文说明、品牌色、图标、许可、体积与桌面入口。"
} finally {
    $resolved = [IO.Path]::GetFullPath($fixture)
    $temporary = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
    if (-not $resolved.StartsWith($temporary, [StringComparison]::OrdinalIgnoreCase)) { throw '拒绝清理临时目录之外的文件。' }
    if (Test-Path -LiteralPath $resolved) { Remove-Item -LiteralPath $resolved -Recurse -Force }
}
