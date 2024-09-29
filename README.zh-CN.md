# ThingsGateway

## 介绍

基于net8的跨平台高性能边缘采集网关，单机采集数据点位可达百万

## 文档

[文档](https://thingsgateway.cn/)

[NuGet](https://www.nuget.org/packages?q=Tags%3A%22ThingsGateway%22)

### 插件列表

#### 采集插件


| 插件名称    | 备注                                  |
| ----------- | ------------------------------------- |
| Modbus      | Rtu/Tcp报文格式，支持串口/Tcp/Udp链路 |
| SiemensS7   | 西门子PLC S7系列                      |
| Dlt6452007  | 支持串口/Tcp/Udp链路                  |
| OpcDaMaster | 64位编译                              |
| OpcUaMaster | 支持证书登录，扩展对象，Json读写      |

#### 业务插件


| 插件名称         | 备注                                                       |
| ---------------- | ---------------------------------------------------------- |
| ModbusSlave      | Rtu/Tcp报文格式，支持串口/Tcp/Udp链路，支持Rpc反写         |
| OpcUaServer      | OpcUa服务端，支持Rpc反写                                   |
| MqttClient       | Mqtt客户端，支持Rpc反写，脚本自定义上传内容                |
| MqttServer       | Mqtt服务端，支持WebSocket，支持Rpc反写，脚本自定义上传内容 |
| KafkaProducer    | 脚本自定义上传内容                                         |
| RabbitMQProducer | 脚本自定义上传内容                                         |
| SqlDB            | 关系数据库存储，支持历史存储和实时数据更新                 |
| SqlHistoryAlarm      | 报警历史数据关系数据库存储                                 |
| TDengineDB       | 时序数据库存储                                             |
| QuestDB          | 时序数据库存储                                             |

## 协议

[Apache-2.0](https://gitee.com/diego2098/ThingsGateway/blob/master/LICENSE)

## 演示

[ThingsGateway演示地址](http://47.119.161.158:5000/)

账户	:  **SuperAdmin**

密码 : **111111**

**右上角个人弹出框中，切换到物联网关模块**

## Docker

```shell

docker pull registry.cn-shenzhen.aliyuncs.com/thingsgateway/thingsgateway

docker pull registry.cn-shenzhen.aliyuncs.com/thingsgateway/thingsgateway_arm64
```

## 赞助

[赞助途径](https://thingsgateway.cn/docs/1000)

## 社区

QQ群：605534569 [跳转](http://qm.qq.com/cgi-bin/qm/qr?_wv=1027&k=NnBjPO-8kcNFzo_RzSbdICflb97u2O1i&authKey=V1MI3iJtpDMHc08myszP262kDykbx2Yev6ebE4Me0elTe0P0IFAmtU5l7Sy5w0jx&noverify=0&group_code=605534569)

## Pro插件

[插件列表](https://thingsgateway.cn/docs/1001)
