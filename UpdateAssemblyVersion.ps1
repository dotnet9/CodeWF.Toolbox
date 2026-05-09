param(
	[Parameter(Mandatory=$true)]
	[string]$AssemblyInfoFile,

	[Parameter(Mandatory=$true)]
	[string]$Configuration,

	[Parameter(Mandatory=$true)]
	[string]$Platform
)

# Validate the input file path.
if (-not (Test-Path $AssemblyInfoFile)) {
    throw "Error: file '$AssemblyInfoFile' does not exist."
}

Write-Output "AssemblyInfoFile: $AssemblyInfoFile"
Write-Output "Configuration: $Configuration"
Write-Output "Platform: $Platform"



# Get the current date and time for the build stamp.
$currentDateTime =Get-Date
# Read the latest tag timestamp as the baseline time.
$strBranchCreateTime =git log -1 --format=%ai $latest_tag
$dateBranchCreateTime=[DateTime]::Parse($strBranchCreateTime)
# Calculate the elapsed minutes since the baseline.
$timeDifMinutes =[math]::Round(($currentDateTime-$dateBranchcreateTime).TotalMinutes)
# Split the version into x.x.65535.65535 style components.
$yy=[math]::Floor($timeDifMinutes/65535)
$thirdInfo=$timeDifMinutes-$yy*65535
# Read the AssemblyInfo content.
$content = Get-Content -Path $AssemblyInfoFile -Encoding Unicode
# Extract the AssemblyProduct value.
$productPattern='^\[assembly: AssemblyProduct\("([^"]+)"\)\]'
$product = ""
# Find the AssemblyProduct line and stop after the first match.
foreach($line in $content){
	if($line -match $productPattern){
		$product= $matches[1]
		break
	}
}
$items=$product -split '_'
$product=$items[0]
# Get the current tag description and checkout hash.
$currentCommitHash=git describe --always --tag --long
$items=$currentCommitHash.Split('-')
$hashInfo=$items[$items.Length-1]
$firstFileVersion="0.0"
$secondFileVersion="100"
if($items.Length -eq 3)
{
	$firstFileVersion=$items[0].Replace("v","");
	$firstFileItems=$firstFileVersion.split('.');
	$firstFileversion=$firstFileItems[0]+"."+$firstFileItems[1]
	$secondFileVersion=$items[1]+"$yy".PadLeft(2,'0')
}

$platformInfo = ""
if ($Configuration -eq "Debug") {
	$platformInfo = "D"
} elseif ($Configuration -eq "Release") {
	$platformInfo = "R"
} else {
	$platformInfo = "A"
}
switch ($Platform) {
	"win-x86" {
		$platformInfo += "-x86"
	}
	"win-x64" {
		$platformInfo += "-x64"
	}
	"linux-x64" {
		$platformInfo += "-x64"
	}
	"linux-arm64" {
		$platformInfo += "-arm64"
	}
	default {
		$platformInfo += "-Unknow"
	}
}

# Build the new AssemblyProduct and AssemblyFileVersion values.
$strDayInfo= $currentDateTime.Tostring("yyyyMMddHHmm")
$newAssemblyProduct = "$product"+"_"+"$platformInfo"+"_"+"$hashInfo"+"_"+"$strDayInfo"
if($secondFileVersion -eq "000")
{
	$secondFileVersion="0"
}
$newAssemblyFileVersion = "$firstFileVersion"+"."+"$secondFileVersion"+"."+"$thirdInfo"

# Update AssemblyProduct.
$content = $content -replace '^\[assembly: AssemblyProduct\(".*"\)\]', "[assembly: AssemblyProduct(`"$newAssemblyProduct`")]"

# Update AssemblyVersion.
$content = $content -replace '^\[assembly: AssemblyVersion\(".*"\)\]', "[assembly: AssemblyVersion(`"$newAssemblyFileVersion`")]"

# Update AssemblyFileVersion.
$content = $content -replace '^\[assembly: AssemblyFileVersion\(".*"\)\]', "[assembly: AssemblyFileVersion(`"$newAssemblyFileVersion`")]"

# Write the updated content back to AssemblyInfo.cs.
Set-Content -Path $AssemblyInfoFile -Value $content -Encoding Unicode
