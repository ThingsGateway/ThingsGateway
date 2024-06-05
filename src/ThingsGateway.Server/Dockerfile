#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

#构建：docker build -t diego2098/thingsgateway:latest .


#aspnetcore8.0环境
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
COPY .  /app
WORKDIR /app
#默认web
EXPOSE 5000
#图像库
RUN echo "deb https://mirrors.tuna.tsinghua.edu.cn/debian/ sid main contrib non-free" > /etc/apt/sources.list
RUN apt-get update && apt-get -y install libfontconfig1

# 添加时区环境变量，亚洲，上海
ENV TimeZone=Asia/Shanghai
# 使用软连接，并且将时区配置覆盖/etc/timezone
RUN ln -snf /usr/share/zoneinfo/$TimeZone /etc/localtime && echo $TimeZone > /etc/timezone

ENTRYPOINT ["dotnet", "ThingsGateway.Server.dll","--urls","http://*:5000"]
