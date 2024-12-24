chcp 65001

rem 更新主仓库
git pull

rem 初始化并更新所有子模块
git submodule update --init

pause