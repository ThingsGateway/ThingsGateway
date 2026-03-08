docker pull mcr.microsoft.com/dotnet/aspnet:10.0-noble-amd64

docker build -t registry.cn-shenzhen.aliyuncs.com/thingsgateway/thingsgatewaymanagement:latest .

docker push registry.cn-shenzhen.aliyuncs.com/thingsgateway/thingsgatewaymanagement
