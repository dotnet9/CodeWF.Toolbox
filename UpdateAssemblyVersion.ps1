param(
	[Parameter(Mandatory=$true)]
	[string]$AssemblyInfoFile,

	[Parameter(Mandatory=$true)]
	[string]$Configuration,

	[Parameter(Mandatory=$true)]
	[string]$Platform
)

# 检查文件是否存在
if (-not (Test-Path $AssemblyInfoFile)) {
    throw "错误：文件 $AssemblyInfoFile 不存在"
}

Write-Output "AssemblyInfoFile: $AssemblyInfoFile"
Write-Output "Configuration: $Configuration"
Write-Output "Platform: $Platform"



#获取当前日期时间，并生成格式化的时间戳
$currentDateTime =Get-Date
#获取当前分支，签出标签创建时间[基准时间]
$strBranchCreateTime =git log -1 --format=%ai $latest_tag
$dateBranchCreateTime=[DateTime]::Parse($strBranchCreateTime)
#时间差的总分数
$timeDifMinutes =[math]::Round(($currentDateTime-$dateBranchcreateTime).TotalMinutes)
#版本号格式处理成x.x.65535.65535
$yy=[math]::Floor($timeDifMinutes/65535)
$thirdInfo=$timeDifMinutes-$yy*65535
#读取文件内容
$content = Get-Content -Path $AssemblyInfoFile -Encoding Unicode
#使用正则表达式获职 AssemblyProduct 的值
$productPattern='^\[assembly: AssemblyProduct\("([^"]+)"\)\]'
$product = ""
#查找包含 AssemblyProduct 的行，并匹配正则表达式
foreach($line in $content){
	if($line -match $productPattern){
		$product= $matches[1]
		break #找到匹配后退出循环
	}
}
$items=$product -split '_'
$product=$items[0]
#当前最近标签、checkout hash值
$currentCommitHash=git describe --always --tag --long
$items=$currentCommitHash.Split('-')
$hashInfo=$items[$items.Length-1]
$firstFileVersion="0.0"
$secondFileVersion="100"
if($items.Length -eq 4)
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
	"x86" {
		$platformInfo += "-86"
	}
	"x64" {
		$platformInfo += "-64"
	}
	"ARM" {
		$platformInfo += "-ARM"
	}
	"AnyCPU" {
		$platformInfo += "-AnyCPU"
	}
	default {
		$platformInfo += "-Unknow"
	}
}

#定义新的 AssemblyProduct和 AssemblyFileversion
$strDayInfo= $currentDateTime.Tostring("yyyyMMddHHmm")
$newAssemblyProduct = "$product"+"_"+"$platformInfo"+"_"+"$hashInfo"+"_"+"$strDayInfo"
if($secondFileVersion -eq "000")
{
	$secondFileVersion="0"
}
$newAssemblyFileVersion = "$firstFileVersion"+"."+"$secondFileVersion"+"."+"$thirdInfo"

# 更新 AssemblyProduct
$content = $content -replace '^\[assembly: AssemblyProduct\(".*"\)\]', "[assembly: AssemblyProduct(`"$newAssemblyProduct`")]"

# 更新AssemblyVersion
$content = $content -replace '^\[assembly: AssemblyVersion\(".*"\)\]', "[assembly: AssemblyVersion(`"$newAssemblyFileVersion`")]"

# 更新AssemblyFileVersion
$content = $content -replace '^\[assembly: AssemblyFileVersion\(".*"\)\]', "[assembly: AssemblyFileVersion(`"$newAssemblyFileVersion`")]"

# 将更新后的内容写入 AssemblyInfo.cs 文件
Set-Content -Path $AssemblyInfoFile -Value $content -Encoding Unicode