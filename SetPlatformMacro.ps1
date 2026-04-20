param(
	[Parameter(Mandatory=$true)]
	[string]$Platform
)

$propsFile = Join-Path $PSScriptRoot "Directory.Build.props"

if (-not (Test-Path $propsFile)) {
    Write-Error "File not found: $propsFile"
    exit 1
}

$macro = ""
switch ($Platform) {
    "linux-x64" { $macro = "PLATFORM_LINUX_X64" }
    "linux-arm64" { $macro = "PLATFORM_LINUX_ARM64" }
    "win-x64" { $macro = "PLATFORM_WIN_X64" }
    "win-x86" { $macro = "PLATFORM_WIN_X86" }
    default {
        Write-Warning "Unknown platform: $Platform"
        exit 1
    }
}

$content = Get-Content -Path $propsFile -Encoding UTF8 -Raw
$pattern = '<DefineConstants>.*?</DefineConstants>'
$replacement = "<DefineConstants>`$(DefineConstants);$macro</DefineConstants>"

$content = $content -replace $pattern, $replacement
Set-Content -Path $propsFile -Value $content -Encoding UTF8
Write-Output "Set DefineConstants to: $macro"
