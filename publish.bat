@echo off
setlocal enabledelayedexpansion

if "%~1"=="" (
    set "project_paths=src\CodeWF.Toolbox.Desktop"
) else (
    set "project_paths=%~1"
)

if "%~2"=="" (
    set "platforms=linux-x64 linux-arm64 win-x64 win-x86"
) else (
    set "platforms=%~2"
)

for %%p in (%platforms%) do (
    set "tfm="
    set "pubxml="

    if "%%p"=="linux-x64" set "tfm=net10.0" & set "pubxml=FolderProfile_linux-x64.pubxml"
    if "%%p"=="linux-arm64" set "tfm=net10.0" & set "pubxml=FolderProfile_linux-arm64.pubxml"
    if "%%p"=="win-x64" set "tfm=net10.0-windows" & set "pubxml=FolderProfile_win-x64.pubxml"
    if "%%p"=="win-x86" set "tfm=net10.0-windows" & set "pubxml=FolderProfile_win-x86.pubxml"

    echo ========================================
    echo Building %%p...
    echo ========================================

    for %%f in (GlobalAssemblies\*) do (
        echo Updating assembly version for %%f...
        powershell -ExecutionPolicy Bypass -File "UpdateAssemblyVersion.ps1" -AssemblyInfoFile "%%f" -Configuration "Release" -Platform "%%p"
        if errorlevel 1 (
            echo Error: Failed to update assembly version for %%f
            goto :error
        )
    )

    powershell -ExecutionPolicy Bypass -File "SetPlatformMacro.ps1" -Platform "%%p"
    if errorlevel 1 (
        echo Error: Failed to set platform macro
        goto :error
    )

    for %%d in (%project_paths%) do (
        echo Publishing %%d for %%p...
        dotnet publish "%%d" -f !tfm! /p:PublishProfile="%%d\Properties\PublishProfiles\!pubxml!"
        if errorlevel 1 (
            echo Error: Failed to publish %%d for %%p
            goto :error
        )
    )
    echo.
)

echo ========================================
echo All platforms published successfully!
echo ========================================
echo Removing *.pdb files...
if exist "%~dp0publish" (
    for /r "%~dp0publish" %%f in (*.pdb) do del /q "%%f" 2>nul
    echo *.pdb files removed.
)
explorer "%~dp0publish"
pause
goto :eof

:error
echo ========================================
echo Build failed! Please check the errors above.
echo ========================================
pause
exit /b 1
