@echo off
setlocal enabledelayedexpansion

rem 遍历 GlobalAssemblies 目录下的文件执行 PowerShell 脚本
for %%f in (GlobalAssemblies\*) do (
    powershell -ExecutionPolicy Bypass -File "UpdateAssemblyVersion.ps1" -AssemblyInfoFile "%%f" -Configuration "Release" -Platform "x64"
    rem 检查 PowerShell 脚本执行是否成功
    if !errorlevel! neq 0 (
        echo Failed to update assembly version for file %%f!
        endlocal
        exit /b !errorlevel!
    )
)

rem 设置发布路径
set PUBLISH_PATH=publish\win-x64

rem 定义项目信息数组，格式为 项目名称,项目路径（只写项目目录）,相对发布目录名
set "projects=码坊工具箱,src\CodeWF.Toolbox.Desktop,codewf Avalonia发布测试,tests\AvaloniaAotDemo,AvaloniaAotDemo"

rem 遍历项目数组
for %%a in ("%projects: =","%") do (
    set "current_project=%%~a"
    for /f "tokens=1,2,3 delims=," %%b in ("!current_project!") do (
        set "projectName=%%b"
        set "projectPath=%%c"
        set "relativePublishDir=%%d"

        rem 拼接 .pubxml 文件的默认路径
        set "pubxmlPath=!projectPath!\Properties\PublishProfiles\FolderProfile-win_x64.pubxml"
        rem 拼接当前项目的发布路径
        set "CURRENT_PUBLISH_DIR=!PUBLISH_PATH!\!relativePublishDir!"

        echo "projectName after assignment: !projectName!"
        echo "projectPath after assignment: !projectPath!"
        echo "relativePublishDir after assignment: !relativePublishDir!"
        echo "pubxmlPath after assignment: !pubxmlPath!"
        echo "current publish dir after assignment: !CURRENT_PUBLISH_DIR!"


        echo Publishing !projectName! for win-64...
        rem 清空发布目录
        if exist "!CURRENT_PUBLISH_DIR!" (
            rd /s /q "!CURRENT_PUBLISH_DIR!"
        )
        rem 创建发布目录
        mkdir /p "!CURRENT_PUBLISH_DIR!" 2>nul
        rem 执行 dotnet publish 命令进行 AOT 发布，并指定目标框架
        dotnet publish "!projectPath!" /p:PublishProfile="!pubxmlPath!" -f net9.0-windows -o "!CURRENT_PUBLISH_DIR!"
        rem 检查 dotnet publish 命令的退出代码
        if !errorlevel! neq 0 (
            echo Publish of !projectName! failed!
            endlocal
            exit /b !errorlevel!
        )
        rem 检查发布目录是否存在
        if exist "!CURRENT_PUBLISH_DIR!" (
            rem 删除调试符号文件
            del "!CURRENT_PUBLISH_DIR!\*.pdb"
        )
        echo Publish of !projectName! succeeded!
        echo Published files of !projectName! are located at: "!CURRENT_PUBLISH_DIR!"
    )
)

endlocal