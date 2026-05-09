@echo off
setlocal enabledelayedexpansion

if "%~1"=="" (
    set "project_paths=src\CodeWF.Toolbox"
) else (
    set "project_paths=%~1"
)

if "%~2"=="" (
    set "platforms=win-x64 linux-x64"
) else (
    set "platforms=%~2"
)

set "publish_root=%~dp0publish"
set "publish_failed=0"

for %%p in (%platforms%) do (
    set "rid=%%p"

    echo ========================================
    echo Building %%p...
    echo ========================================

    for %%d in (%project_paths%) do (
        for %%n in ("%%d") do set "project_name=%%~nxn"

        echo Publishing %%d for %%p...
        call :publish_with_profile "%%d" "!rid!" "!project_name!"
        if errorlevel 1 (
            echo Error: Failed to publish %%d for %%p
            set "publish_failed=1"
        )
    )
    echo.
)

if "%publish_failed%"=="1" (
    echo ========================================
    echo Build failed. Please check the errors above.
    echo ========================================
    call :maybe_pause
    exit /b 1
)

echo ========================================
echo All platforms published successfully.
echo ========================================
if /i not "%CODEX_NO_EXPLORER%"=="1" explorer "%publish_root%"
call :maybe_pause
goto :eof

:publish_with_profile
set "project_path=%~1"
set "rid=%~2"
set "project_name=%~3"
set "publish_profile=FolderProfile_%rid%"
set "profile_dir=%project_path%\Properties\PublishProfiles"

call :try_publish "%project_path%" "%publish_profile%" "%project_name%" "%profile_dir%"
exit /b %errorlevel%

:try_publish
set "project_path=%~1"
set "publish_profile=%~2"
set "project_name=%~3"
set "profile_dir=%~4"
set "profile_file=%profile_dir%\%publish_profile%.pubxml"
set "target_framework="
set "runtime_identifier="
set "profile_metadata="

if not exist "%profile_file%" (
    echo Missing publish profile: %profile_file%
    exit /b 1
)

for /f "usebackq delims=" %%m in (`powershell -NoProfile -Command "$xml = [xml](Get-Content -LiteralPath '%profile_file%' -Raw -Encoding UTF8); '{0}|{1}' -f $xml.Project.PropertyGroup.TargetFramework, $xml.Project.PropertyGroup.RuntimeIdentifier"`) do (
    set "profile_metadata=%%m"
)

for /f "tokens=1,2 delims=|" %%f in ("%profile_metadata%") do (
    set "target_framework=%%f"
    set "runtime_identifier=%%g"
)

if not defined target_framework (
    echo Missing target framework in publish profile: %profile_file%
    exit /b 1
)

if not defined runtime_identifier (
    echo Missing runtime identifier in publish profile: %profile_file%
    exit /b 1
)

echo   - Using profile %publish_profile%...
dotnet publish "%project_path%" -f %target_framework% -r %runtime_identifier% -p:PublishProfile=%publish_profile%
if errorlevel 1 exit /b 1

if exist "%publish_root%" (
    set /a removed_pdb_count=0
    for /r "%publish_root%" %%f in (*.pdb) do (
        del /q "%%f" 2>nul
        if not exist "%%f" set /a removed_pdb_count+=1
    )
    if !removed_pdb_count! gtr 0 echo   - Removed !removed_pdb_count! *.pdb file(s^)
)

echo   - Success: %project_name% / %rid%
exit /b 0

:maybe_pause
if /i not "%CODEX_NO_PAUSE%"=="1" pause
exit /b 0
