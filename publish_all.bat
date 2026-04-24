@echo off
setlocal enabledelayedexpansion

set "project_paths=src\CodeWF.Toolbox.Desktop"
set "platforms=linux-x64 linux-arm64 win-x64 win-x86"

call "%~dp0publish.bat" "%project_paths%" "%platforms%"
