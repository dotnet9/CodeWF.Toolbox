[CmdletBinding()]
param(
    [string] $RepositoryRoot = '',
    [string] $InnoSetupCompiler = '',
    [switch] $Compile
)
$ErrorActionPreference = 'Stop'
if (-not $RepositoryRoot) { $RepositoryRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot) }
$repository = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$scripts = Join-Path $repository 'scripts'
$definitions = @(Get-ChildItem -LiteralPath $scripts -Filter '*.iss' -File)
if ($definitions.Count -ne 1) { throw '每仓必须有一个产品安装入口。' }
$definition = $definitions[0]
$content = Get-Content -LiteralPath $definition.FullName -Raw -Encoding UTF8
if ($content -notmatch '(?m)^#include\s+"installer\\CodeWF\.Setup\.iss"\s*$') { throw '安装入口未包含本仓视觉引擎。' }
if ($content -notmatch '(?m)^LicenseFile=\.\.\\LICENSE\s*$') { throw '安装入口未声明本仓许可。' }
if ($content -notmatch 'Name: "english"; MessagesFile: "compiler:Default\.isl"') { throw '英文基础资源缺失。' }
if ($content -notmatch 'Name: "chinesesimplified"; MessagesFile: "Languages\\ChineseSimplified\.isl"') { throw '本仓中文基础资源缺失。' }
if ($definition.BaseName -eq 'Vex') {
    if ($content -notmatch '(?m)^Name: "markdownassociation"; Description: "\{cm:MarkdownAssociation\}"; GroupDescription: "\{cm:FileAssociations\}"\s*$') { throw 'Vex 必须提供默认选中的 Markdown 打开候选选项。' }
    $registryLines = @($content -split "`r?`n" | Where-Object { $_ -match '^Root:' })
    if ($registryLines.Count -ne 11 -or @($registryLines | Where-Object { $_ -notmatch '; Tasks: markdownassociation\s*$' }).Count -ne 0) { throw 'Vex 所有候选、图标和右键注册项必须受 Markdown 选项控制。' }
    if ($content -match 'UserChoice|ValueName: "ProgId"') { throw 'Vex 不能修改用户的默认打开选择。' }
}
$license = Join-Path $repository 'LICENSE'
if (-not (Test-Path -LiteralPath $license -PathType Leaf)) { throw '仓库许可文件缺失。' }
$languagePath = Join-Path $scripts 'Languages'
foreach ($name in @('ChineseSimplified.isl', 'INNO-SETUP-LICENSE.txt')) {
    if (-not (Test-Path -LiteralPath (Join-Path $languagePath $name) -PathType Leaf)) { throw "安装语言资源或上游许可缺失：$name" }
}
$installerPath = Join-Path $scripts 'installer'
foreach ($name in @('CodeWF.Setup.iss', 'CodeWF.Setup.Messages.iss', 'CodeWF.Setup.Progress.iss', 'CodeWF.Setup.Maintenance.iss', 'CodeWF.Setup.Uninstall.iss', 'frame-dark.bmp', 'frame-light.bmp', 'reactor-dark.bmp', 'reactor-light.bmp', 'core-dark.bmp', 'core-light.bmp', 'source-sha256.json')) {
    $path = Join-Path $installerPath $name
    if (-not (Test-Path -LiteralPath $path -PathType Leaf) -or (Get-Item -LiteralPath $path).Length -eq 0) { throw "视觉引擎资源缺失或为空：$name" }
}
$manifest = Get-Content -LiteralPath (Join-Path $installerPath 'source-sha256.json') -Raw -Encoding UTF8 | ConvertFrom-Json
if ($manifest.schema -ne 1 -or $manifest.files.Count -lt 11) { throw '共享快照源清单格式或文件数不正确。' }
foreach ($entry in $manifest.files) {
    if ($entry.path -notmatch '^(?:tests/)?[A-Za-z0-9_.-]+$' -or $entry.path -in @('.', '..', 'source-sha256.json')) { throw '共享快照清单仅允许根目录或 tests 目录的明确文件名。' }
    $path = Join-Path $installerPath $entry.path
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "共享快照清单文件缺失：$($entry.path)" }
    if ($entry.mode -eq 'normalized-lf') {
        $text = [IO.File]::ReadAllText($path).Replace("`r`n", "`n").Replace("`r", "`n")
        $bytes = [Text.UTF8Encoding]::new($false).GetBytes($text)
        $sha = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
    }
    elseif ($entry.mode -eq 'binary') { $sha = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant() }
    else { throw "共享快照清单使用未知哈希模式：$($entry.mode)" }
    if ($sha -ne $entry.sha256) { throw "共享快照源哈希不一致：$($entry.path)" }
}
foreach ($source in @(Get-ChildItem -LiteralPath $installerPath -File -Filter '*.iss') + @($definition)) {
    $text = Get-Content -LiteralPath $source.FullName -Raw -Encoding UTF8
    if ($text -match '(?im)^#include\s+"(?:[A-Z]:|\\\\|\.\.\\\.\.\\)') { throw "包含文件依赖仓库之外的路径：$($source.Name)" }
    if ($text -match '(?i)E:[\\/]github[\\/](?:Libs|Apps)') { throw "安装源码依赖本机其他仓库：$($source.Name)" }
}
Write-Host 'PASS: 本仓安装器、许可与两语资源完整。'
if (-not $Compile) { return }
if (-not $InnoSetupCompiler) {
    $command = Get-Command ISCC.exe -CommandType Application -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($command) { $InnoSetupCompiler = $command.Source }
    elseif (${env:ProgramFiles(x86)}) { $InnoSetupCompiler = Join-Path ${env:ProgramFiles(x86)} 'Inno Setup 6\ISCC.exe' }
}
if (-not $InnoSetupCompiler -or -not (Test-Path -LiteralPath $InnoSetupCompiler -PathType Leaf)) { throw '真编译检查需要通过 -InnoSetupCompiler 指定 ISCC.exe。' }
$temporaryRoot = if ($env:CODEWF_TEST_TMP) { $env:CODEWF_TEST_TMP } else { [IO.Path]::GetTempPath() }
$temporaryRoot = [IO.Path]::GetFullPath($temporaryRoot).TrimEnd([IO.Path]::DirectorySeparatorChar)
$fixture = Join-Path $temporaryRoot ('codewf-setup-contract-' + [guid]::NewGuid().ToString('N'))
# 仅复制安装所需文件，防止相邻仓库或原工作区意外满足缺失引用。
$isolated = Join-Path $fixture '独立仓库 with spaces'
$isolatedScripts = Join-Path $isolated 'scripts'
$payload = Join-Path $fixture '受控载荷 with spaces'
$output = Join-Path $fixture 'output'
New-Item -ItemType Directory -Path $isolatedScripts, $payload, $output -Force | Out-Null
Copy-Item -LiteralPath $definition.FullName -Destination $isolatedScripts
Copy-Item -LiteralPath $installerPath -Destination $isolatedScripts -Recurse
Copy-Item -LiteralPath $languagePath -Destination $isolatedScripts -Recurse
Copy-Item -LiteralPath $license -Destination $isolated
if ($content -notmatch 'Filename: "\{app\}\\([^"\\]+)"') { throw '安装入口未声明目标程序。' }
# 编译夹具不执行；载荷仅验证安装源码的解析与资源封装。
[IO.File]::WriteAllText((Join-Path $payload $Matches[1]), 'MZ controlled compile fixture only')
& $InnoSetupCompiler '/Q' (Join-Path $isolatedScripts $definition.Name) '/DAppVersion=9.9.9' "/DSourceDir=$payload" "/DOutputDir=$output"
if ($LASTEXITCODE -ne 0) { throw "独立安装器编译失败：$LASTEXITCODE" }
$packages = @(Get-ChildItem -LiteralPath $output -Filter '*.exe' -File)
if ($packages.Count -ne 1 -or $packages[0].Length -eq 0) { throw '独立编译未生成唯一且非空的 Windows 安装包。' }
[ordered]@{
    InstallerScript = $definition.Name
    IsolatedSourceDirectory = $isolated
    SourceManifestSha256 = (Get-FileHash -LiteralPath (Join-Path $installerPath 'source-sha256.json') -Algorithm SHA256).Hash.ToLowerInvariant()
    InstallerSha256 = (Get-FileHash -LiteralPath $packages[0].FullName -Algorithm SHA256).Hash.ToLowerInvariant()
    InstallerExecuted = $false
    ContainsOnlyControlledPayload = $true
} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $fixture 'verification.json') -Encoding utf8
Write-Host "PASS: 独立中文与空格路径下的安装器真编译通过，未执行安装器。夹具：$fixture"
