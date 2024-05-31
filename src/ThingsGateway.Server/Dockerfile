#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

#aspnetcore6.0环境
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
COPY .  /app
WORKDIR /app
#默认web
EXPOSE 7100
#默认冗余
EXPOSE 7101
#默认mqtt
EXPOSE 1883
#默认mqtt
EXPOSE 8083
#默认opcua
EXPOSE 49321
#默认modbus
EXPOSE 502
#图像库
RUN echo "deb https://mirrors.tuna.tsinghua.edu.cn/debian/ sid main contrib non-free" > /etc/apt/sources.list
RUN apt-get update && apt-get -y install libfontconfig1


ENTRYPOINT ["dotnet", "ThingsGateway.Server.dll","--urls","http://*:7100"]
