@echo off
setlocal

REM 1. 获取当前执行的路径
set "currentPath=%~dp0"

REM 2. 打开到当前路径下
cd /d "%currentPath%"

REM 3. 执行WindowsService1.exe uninstall
WindowsService1.exe uninstall