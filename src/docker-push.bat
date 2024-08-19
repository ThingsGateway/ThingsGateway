chcp 65001
rmdir /S /Q %~dp0\nupkgs
rem 构建解决方案
dotnet clean ThingsGateway.sln
dotnet build ThingsGateway.sln -c Release

rem 切换到 ThingsGateway.Server 目录
cd .\ThingsGateway.Server
set SolutionDir=%~dp0

rem 发布 ThingsGateway.Server 项目
dotnet publish -r linux-x64 -c Release --framework net8.0 


rem 切换到发布文件夹
cd bin\Release\net8.0\linux-x64\publish


rem 执行发布文件夹下的 ps1 文件
powershell.exe -ExecutionPolicy Bypass -File DockerPush.ps1


cd %~dp0
cd .\ThingsGateway.Server

rem 发布 ThingsGateway.Server 项目
dotnet publish -r linux-arm64 -c Release --framework net8.0 


rem 切换到发布文件夹
cd bin\Release\net8.0\linux-arm64\publish

rem 执行发布文件夹下的 ps1 文件
powershell.exe -ExecutionPolicy Bypass -File DockerPush_arm64.ps1



