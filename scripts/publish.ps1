# 统一发布入口：scripts/publish.ps1
# 用法：pwsh scripts/publish.ps1 [-RuntimeIdentifier win-x64] [-Version 0.0.0]
# 输出：artifacts/publish/<rid>/<AppName>/
[CmdletBinding()]
param(
    [string] $RuntimeIdentifier = "win-x64",
    [string] $Version = ""
)

$ErrorActionPreference = "Stop"
$repositoryRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..")).Path

if ([string]::IsNullOrWhiteSpace($Version)) {
    $props = Join-Path $repositoryRoot "Directory.Build.props"
    if (Test-Path -LiteralPath $props) {
        $match = Select-String -LiteralPath $props -Pattern '<Version>([^<]+)</Version>' |
            Select-Object -First 1
        if ($match) { $Version = $match.Matches[0].Groups[1].Value }
    }
}
Write-Host "发布 $RuntimeIdentifier (Version=$Version)"

$project = Join-Path $repositoryRoot "src/CodeWF.Toolbox/CodeWF.Toolbox.csproj"
$pubxml = Join-Path $repositoryRoot "src/CodeWF.Toolbox/Properties/PublishProfiles/FolderProfile_$RuntimeIdentifier.pubxml"
$tfmNode = Select-String -LiteralPath $pubxml -Pattern '<TargetFramework>([^<]+)</TargetFramework>' | Select-Object -First 1
$tfm = if ($tfmNode) { $tfmNode.Matches[0].Groups[1].Value } else { "net11.0-windows" }
# NativeAOT 的 ILC 在 preview SDK 上并行编译可能崩溃，单线程更稳
$ilcArgs = @()
if ($RuntimeIdentifier.StartsWith("win-", [StringComparison]::OrdinalIgnoreCase)) { $ilcArgs += "-p:IlcSingleThreaded=true" }
dotnet publish $project -c Release -f $tfm -r $RuntimeIdentifier @ilcArgs -p:PublishProfile=FolderProfile_$RuntimeIdentifier -p:Version=$Version
if ($LASTEXITCODE -ne 0) { throw "publish failed for $RuntimeIdentifier" }
