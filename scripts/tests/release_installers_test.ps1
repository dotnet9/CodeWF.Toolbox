$ErrorActionPreference = 'Stop'
$temporaryRoot = if ($env:CODEWF_TEST_TMP) { $env:CODEWF_TEST_TMP } else { [IO.Path]::GetTempPath() }
$temporaryRoot = [IO.Path]::GetFullPath($temporaryRoot)
$fixture = Join-Path $temporaryRoot ('codewf-installers-test-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $fixture -Force | Out-Null
$verify = Join-Path $PSScriptRoot '../verify_release_installers.ps1'
function New-FixtureSet([string] $Product, [string] $ArtifactVersion) {
    foreach ($old in Get-ChildItem -LiteralPath $fixture -File) { Remove-Item -LiteralPath $old.FullName -Force }
    foreach ($rid in @('win-x64', 'linux-x64', 'linux-arm64', 'osx-x64', 'osx-arm64')) {
        $suffix = if ($rid.StartsWith('win')) { '-setup.exe' } elseif ($rid.StartsWith('linux')) { '.deb' } else { '.dmg' }
        $file = Join-Path $fixture ("$Product-v$ArtifactVersion-" + $rid + $suffix)
        Set-Content -LiteralPath $file -Value ('Controlled packaging fixture ' + $rid) -Encoding utf8
        $hash = (Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash.ToLowerInvariant()
        "$hash  $([IO.Path]::GetFileName($file))" | Set-Content -LiteralPath ($file + '.sha256') -Encoding ascii
    }
}
function Assert-Rejected([scriptblock] $Action) {
    $rejected = $false
    try { & $Action } catch { $rejected = $true }
    if (-not $rejected) { throw '错误的发布产物未被拒绝。' }
}
try {
    New-FixtureSet 'CodeWF.Toolbox' '1.2.3'
    & $verify -Version '1.2.3' -PackageDirectory $fixture
    & $verify -Version 'v1.2.3' -PackageDirectory $fixture
    $extra = Join-Path $fixture 'Example-portable.zip'
    Set-Content -LiteralPath $extra -Value 'Forbidden portable distribution'
    Assert-Rejected { & $verify -Version '1.2.3' -PackageDirectory $fixture }
    Remove-Item -LiteralPath $extra -Force
    $checksum = Get-ChildItem -LiteralPath $fixture -Filter '*.sha256' | Select-Object -First 1
    Set-Content -LiteralPath $checksum.FullName -Value ('0' * 64 + '  ' + $checksum.BaseName)
    Assert-Rejected { & $verify -Version '1.2.3' -PackageDirectory $fixture }
    Remove-Item -LiteralPath $checksum.FullName -Force
    Assert-Rejected { & $verify -Version '1.2.3' -PackageDirectory $fixture }
    New-FixtureSet 'CodeWF.Toolbox' '1.2.3.4'
    Assert-Rejected { & $verify -Version '1.2.3' -PackageDirectory $fixture }
    & $verify -Version '1.2.3.4' -PackageDirectory $fixture
    New-FixtureSet 'Example' '1.2.3'
    Assert-Rejected { & $verify -Version '1.2.3' -PackageDirectory $fixture }
    Write-Host 'PASS: 精确产品与版本的五平台产物通过，错版本、错产品、便携包、篡改校验和缺失文件均被拒绝。'
}
finally {
    $resolved = [IO.Path]::GetFullPath($fixture)
    if ([IO.Path]::GetDirectoryName($resolved) -ne $temporaryRoot.TrimEnd([IO.Path]::DirectorySeparatorChar) -or
        -not [IO.Path]::GetFileName($resolved).StartsWith('codewf-installers-test-')) { throw '临时目录范围错误。' }
    Remove-Item -LiteralPath $resolved -Recurse -Force
}
