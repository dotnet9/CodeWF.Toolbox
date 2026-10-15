# 与 CI 一致的五平台 NativeAOT 发布入口；只构建，不运行安装器。
[CmdletBinding()]
param(
    [ValidateSet('win-x64','linux-x64','linux-arm64','osx-x64','osx-arm64')]
    [string] $RuntimeIdentifier = 'win-x64',
    [string] $Version = '',
    [string] $OutputDirectory = ''
)
$ErrorActionPreference = 'Stop'
$repositoryRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
if (-not $Version) {
    [xml]$props = Get-Content -LiteralPath (Join-Path $repositoryRoot 'Directory.Build.props')
    $Version = $props.Project.PropertyGroup.Version
}
if (-not $OutputDirectory) { $OutputDirectory = Join-Path $repositoryRoot "publish/$RuntimeIdentifier" }
$project = Join-Path $repositoryRoot 'src/CodeWF.Toolbox/CodeWF.Toolbox.csproj'
dotnet publish $project -c Release -f net10.0 -r $RuntimeIdentifier --self-contained true `
    -p:PublishAot=true -p:PublishTrimmed=true -p:PublishSingleFile=false `
    -p:IlcGenerateCompleteTypeMetadata=true -p:IlcTrimMetadata=false -p:IlcSingleThreaded=true `
    -p:CompressSymbols=false -p:DebugType=none -p:DebugSymbols=false `
    -p:Version=$Version -o $OutputDirectory
if ($LASTEXITCODE -ne 0) { throw "NativeAOT publish failed for $RuntimeIdentifier" }
Write-Host "Published $Version ${RuntimeIdentifier}: $OutputDirectory"
