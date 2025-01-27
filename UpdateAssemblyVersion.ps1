param($assemblyInfoFile)

#获取当前日期时间，并生成格式化的时间戳
$currentDateTime =Get-Date
#获取当前分支，签出标签创建时间[基准时间]
$strBranchCreateTime =git log -1 --format=%ai $latest tag
$dateBranchCreateTime=[DateTime]::Parse($strBranchCreateTime)
#时间差的总分数
$timeDifMinutes =[math]::Round(($currentDateTime-$dateBranchcreateTime).TotalMinutes)
#版本号格式处理成x.x.65535.65535
$yy=[math]::Floor($timeDifMinutes/65535)
$thirdInfo=$timeDifMinutes-$yy*65535
#读取文件内容
$content = Get-Content -Path $assemblyInfoFile -Encoding Unicode
#使用正则表达式获职 AssemblyProduct 的值
$productPattern='^\[assembly: AssemblyProduct\("([^"]+)"\)\]'
$product =""
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
if($items.Length -eq 3)
{
	$firstFileVersion=$items[0].Replace("v","");
	$firstFileItems=$firstFileVersion.split('.');
	$firstFileversion=$firstFileItems[0]+"."+$firstFileItems[1]
	$secondFileVersion=$items[1]+"$yy".PadLeft(2,'0')
}
#定义新的 AssemblyProduct和 AssemblyFileversion
$strDayInfo= $currentDateTime.Tostring("yyyyMMddHHmm")
$newAssemblyProduct = "$product"+"_"+"$hashInfo"+"_"+"$strDayInfo"
if($secondFileVersion -eq "000")
{
	$secondFileVersion="0"
}
$newAssemblyFileVersion = "$firstFileVersion"+"."+"$secondFileVersion"+"."+"$thirdInfo"

# 更新 AssemblyProduct
$content = $content -replace '^\[assembly: AssemblyProduct\(".*"\)\]', "[assembly: AssemblyProduct(`"$newAssemblyProduct`")]"

# 更新AssemblyFileVersion
$content = $content -replace '^\[assembly: AssemblyFileVersion\(".*"\)\]', "[assembly: AssemblyFileVersion(`"$newAssemblyFileVersion`")]"

# 将更新后的内容写入 AssemblyInfo.cs 文件
Set-Content -Path $assemblyInfoFile -Value $content -Encoding Unicode