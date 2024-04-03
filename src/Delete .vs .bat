@echo off
chcp 65001
setlocal enabledelayedexpansion

set "folder=%~dp0"
attrib -s -h "%folder%\.vs" >nul 2>&1
if exist "%folder%\.vs" (
    rd /s /q "%folder%\.vs"
    echo 删除了.vs文件夹：%folder%\.vs
)

echo 删除完成！
pause
