@echo off
chcp 65001
setlocal enabledelayedexpansion

set "folder=%~dp0/nupkgs"
 rd /s /q "%folder%"
echo 删除了名称为"nupkgs"的文件夹

echo 删除完成！
pause
