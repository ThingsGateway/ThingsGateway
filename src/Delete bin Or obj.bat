@echo off
chcp 65001
setlocal enabledelayedexpansion

set "folder=%~dp0"
for /r "%folder%" /d %%i in (*) do (
    set "dirname=%%~nxi"
    if /I "!dirname!"=="bin" (
        rd /s /q "%%i"
        echo 删除了名称为"bin"的文件夹：%%i
    )
        if /I "!dirname!"=="obj" (
        rd /s /q "%%i"
        echo 删除了名称为"obj"的文件夹：%%i
    )
)

echo 删除完成！
pause
