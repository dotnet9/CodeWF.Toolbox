@echo off
setlocal enabledelayedexpansion

set "project_paths=src\CodeWF.Toolbox.Desktop"
set "platforms=win-x64"

call "%~dp0publish.bat" "%project_paths%" "%platforms%"
