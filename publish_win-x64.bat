@echo off
setlocal enabledelayedexpansion

set "project_paths=src\CodeWF.Toolbox"
set "platforms=win-x64"

call "%~dp0publishbase.bat" "%project_paths%" "%platforms%"
