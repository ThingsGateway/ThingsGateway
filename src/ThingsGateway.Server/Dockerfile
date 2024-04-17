#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

#aspnetcore6.0环境
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
COPY .  /app
WORKDIR /app
EXPOSE 7200
#图像库
RUN echo "deb https://mirrors.tuna.tsinghua.edu.cn/debian/ sid main contrib non-free" > /etc/apt/sources.list
RUN apt-get update && apt-get -y install libfontconfig1


ENTRYPOINT ["dotnet", "ThingsGateway.Server.dll","--urls","http://*:5000"]