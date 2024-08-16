chcp 65001
rmdir /S /Q %~dp0\nupkgs
rem 构建解决方案
dotnet clean ThingsGateway.sln
dotnet build ThingsGateway.sln -c Release

rem 切换到 ThingsGateway.Server 目录
cd .\ThingsGateway.Server
set SolutionDir=%~dp0

rem 发布 ThingsGateway.Server 项目
dotnet publish -c Release --framework net8.0

rem 切换到发布文件夹
cd bin\Release\net8.0\publish

rem 执行发布文件夹下的 ps1 文件
powershell.exe -ExecutionPolicy Bypass -File DockerPush.ps1

rem 切换文件夹
cd %~dp0

rem 执行文件夹下的 ps1 文件
powershell.exe -ExecutionPolicy Bypass -File nuget-push.ps1

pause