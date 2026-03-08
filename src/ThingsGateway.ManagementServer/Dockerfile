
#aspnetcore10.0环境
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-amd64 AS base
#FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
COPY .  /app
WORKDIR /app
#默认web
EXPOSE 5000

# 添加时区环境变量，亚洲，上海
ENV TimeZone=Asia/Shanghai
ENV TZ=Asia/Shanghai
# 转发头
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

ENTRYPOINT ["dotnet", "ThingsGateway.ManagementServer.dll","--urls","http://*:5000"]
