docker pull mcr.microsoft.com/dotnet/aspnet:10.0-noble-amd64

docker build --platform linux/amd64 -t registry.cn-shenzhen.aliyuncs.com/thingsgateway/thingsgateway:latest .

docker push registry.cn-shenzhen.aliyuncs.com/thingsgateway/thingsgateway
