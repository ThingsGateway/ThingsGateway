
# ThingsGateway

## 介绍

 **NetCore** 跨平台边缘采集网关(工业设备采集)

 **ThingsGateway** 存储库同时提供 [**设备采集驱动**](https://www.nuget.org/packages?q=Tags%3A%22ThingsGateway%22)

 **ThingsGateway** 存储库同时提供 **基于Blazor的权限框架** 查看 **ThingsGateway - Admin**


## 文档

[ThingsGateway](https://diego2098.gitee.io/thingsgateway-docs/) 文档。

### 插件列表

#### 采集插件
| 插件名称 | 备注 | 
|-------|-------|
| SiemensS7Master | Rtu/Tcp报文格式，支持串口/Tcp/Udp链路 | 
| S7 | 西门子PLC S7系列 | 
| Dlt6452007 | Master，支持串口/Tcp/Udp链路 | 
| OpcDaClient | 64位编译 |
| OpcUaClient | 支持证书登录，扩展对象Json读写 |

#### 业务插件
| 插件名称 | 备注 | 
|-------|-------|
| SiemensS7Slave | Rtu/Tcp报文格式，支持串口/Tcp/Udp链路，支持Rpc反写 | 
| OpcUaServer | OpcUa服务端，支持Rpc反写 | 
| Mqtt Client | Mqtt客户端，支持Rpc反写，脚本自定义上传内容 | 
| Mqtt Server | Mqtt服务端，支持WebSocket，支持Rpc反写，脚本自定义上传内容 | 
| Kafka Client | 数据生产，脚本自定义上传内容 | 
| RabbitMQ Client | 数据生产，脚本自定义上传内容 | 
| SqlDB | 关系数据库存储，支持历史存储和实时数据更新 | 
| SqlHisAlarm | 报警历史数据关系数据库存储 | 
| TDengineDB | 时序数据库存储 | 
| QuestDB | 时序数据库存储 | 

## 协议

[ThingsGateway](https://gitee.com/diego2098/ThingsGateway) 采用 [Apache-2.0](https://gitee.com/diego2098/ThingsGateway/blob/master/LICENSE) 开源协议。

## 演示

[ThingsGateway演示地址](http://120.24.62.140:5000/)

账户	:  **superAdmin**	

密码 : **111111**

## 赞助

[ThingsGateway赞助途径](https://diego2098.gitee.io/thingsgateway-docs/docs/1000)

## 社区

QQ群：605534569 [跳转](http://qm.qq.com/cgi-bin/qm/qr?_wv=1027&k=NnBjPO-8kcNFzo_RzSbdICflb97u2O1i&authKey=V1MI3iJtpDMHc08myszP262kDykbx2Yev6ebE4Me0elTe0P0IFAmtU5l7Sy5w0jx&noverify=0&group_code=605534569)

## Pro插件

[插件列表](https://diego2098.gitee.io/thingsgateway-docs/docs/1001)



