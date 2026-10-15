$ErrorActionPreference = 'Stop'
$temporaryRoot = if ($env:CODEWF_TEST_TMP) { $env:CODEWF_TEST_TMP } else { [IO.Path]::GetTempPath() }
$temporaryRoot = [IO.Path]::GetFullPath($temporaryRoot)
$fixture = Join-Path $temporaryRoot ('codewf-installers-test-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $fixture -Force | Out-Null
$verify = Join-Path $PSScriptRoot '../verify_release_installers.ps1'
function Assert-Rejected([scriptblock] $Action) {
    $rejected = $false
    try { & $Action } catch { $rejected = $true }
    if (-not $rejected) { throw '错误的发布产物未被拒绝。' }
}
try {
    foreach ($rid in @('win-x64', 'linux-x64', 'linux-arm64', 'osx-x64', 'osx-arm64')) {
        $extension = if ($rid.StartsWith('win')) { '.exe' } elseif ($rid.StartsWith('linux')) { '.deb' } else { '.dmg' }
        $file = Join-Path $fixture ('Example-v1.2.3-' + $rid + $extension)
        Set-Content -LiteralPath $file -Value ('Controlled packaging fixture ' + $rid) -Encoding utf8
        $hash = (Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash.ToLowerInvariant()
        "$hash  $([IO.Path]::GetFileName($file))" | Set-Content -LiteralPath ($file + '.sha256') -Encoding ascii
    }
    & $verify -Version '1.2.3' -PackageDirectory $fixture
    $extra = Join-Path $fixture 'Example-portable.zip'
    Set-Content -LiteralPath $extra -Value 'Forbidden portable distribution'
    Assert-Rejected { & $verify -Version '1.2.3' -PackageDirectory $fixture }
    Remove-Item -LiteralPath $extra -Force
    $checksum = Get-ChildItem -LiteralPath $fixture -Filter '*.sha256' | Select-Object -First 1
    Set-Content -LiteralPath $checksum.FullName -Value ('0' * 64 + '  ' + $checksum.BaseName)
    Assert-Rejected { & $verify -Version '1.2.3' -PackageDirectory $fixture }
    Remove-Item -LiteralPath $checksum.FullName -Force
    Assert-Rejected { & $verify -Version '1.2.3' -PackageDirectory $fixture }
    Write-Host 'PASS: 五平台完整产物通过，便携压缩包、篡改校验和缺失文件均被拒绝。'
}
finally {
    $resolved = [IO.Path]::GetFullPath($fixture)
    if ([IO.Path]::GetDirectoryName($resolved) -ne $temporaryRoot.TrimEnd([IO.Path]::DirectorySeparatorChar) -or
        -not [IO.Path]::GetFileName($resolved).StartsWith('codewf-installers-test-')) { throw '临时目录范围错误。' }
    Remove-Item -LiteralPath $resolved -Recurse -Force
}
