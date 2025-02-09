@echo off
setlocal

rem 遍历 GlobalAssemblies 目录下的文件执行 PowerShell 脚本
for %%f in (GlobalAssemblies\*) do (
    powershell -ExecutionPolicy Bypass -File "UpdateAssemblyVersion.ps1" -AssemblyInfoFile "%%f" -Configuration "Release" -Platform "x64"
    rem 检查 PowerShell 脚本执行是否成功
    if %errorlevel% neq 0 (
        echo Failed to update assembly version for file %%f!
        endlocal
        exit /b %errorlevel%
    )
)

rem 设置发布路径
set PUBLISH_PATH=publish
set WIN64_DIR="%PUBLISH_PATH%/win64"

echo Publishing for win-64...

rem 清空发布目录
if exist %WIN64_DIR% (
    rd /s /q %WIN64_DIR%
)
rem 创建发布目录
mkdir "%WIN64_DIR%" 2>nul

rem 执行 dotnet publish 命令进行 AOT 发布，并指定目标框架
dotnet publish src/CodeWF.Toolbox.Desktop/ -r win-x64 -c Release --sc -f net9.0-windows /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true /p:PublishAot=true -o %WIN64_DIR%

rem 检查 dotnet publish 命令的退出代码
if %errorlevel% neq 0 (
    echo Publish failed!
    endlocal
    exit /b %errorlevel%
)

rem 检查发布目录是否存在
if exist %WIN64_DIR% (
    rem 删除调试符号文件
    del %WIN64_DIR%\*.pdb
)
echo Publish succeeded!
echo Published files are located at: %WIN64_DIR%

endlocal