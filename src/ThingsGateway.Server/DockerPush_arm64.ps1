docker pull mcr.microsoft.com/dotnet/aspnet:10.0-noble-arm64v8

docker build -f Dockerfile_arm64 --platform linux/arm64 -t registry.cn-shenzhen.aliyuncs.com/thingsgateway/thingsgateway_arm64:latest .

docker push registry.cn-shenzhen.aliyuncs.com/thingsgateway/thingsgateway_arm64
