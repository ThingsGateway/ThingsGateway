# ThingsGateway

﻿

## Introduction

﻿
A cross-platform, high-performance edge data collection gateway based on net8, capable of handling millions of data points per.
﻿

## Documentation

﻿
[Documentation](https://thingsgateway.cn/).
﻿
[NuGet](https://www.nuget.org/packages?q=Tags%3A%22ThingsGateway%22)
﻿

### Plugin List

﻿

#### Data Collection Plugins


| Plugin Name | Remarks                                                       |
| ----------- | ------------------------------------------------------------- |
| Modbus      | Supports Rtu/Tcp message formats, with Serial/Tcp/Udp links   |
| SiemensS7   | Siemens PLC S7 series                                         |
| Dlt6452007  | Supports Serial/Tcp/Udp links                                 |
| OpcDaMaster | Compiled for 64-bit                                           |
| OpcUaMaster | Supports certificate login, object extension, Json read/write |

#### Business Plugins


| Plugin Name      | Remarks                                                                                           |
| ---------------- | ------------------------------------------------------------------------------------------------- |
| ModbusSlave      | Supports Rtu/Tcp message formats, with Serial/Tcp/Udp links, supports Rpc reverse writing         |
| OpcUaServer      | OpcUa server, supports Rpc reverse writing                                                        |
| MqttClient       | Mqtt client, supports Rpc reverse writing, script-customizable upload content                     |
| MqttServer       | Mqtt server, supports WebSocket, supports Rpc reverse writing, script-customizable upload content |
| KafkaProducer    | Script-customizable upload content                                                                |
| RabbitMQProducer | Script-customizable upload content                                                                |
| SqlDB            | Relational database storage, supports historical storage and real-time data updates               |
| SqlHistoryAlarm      | Alarm historical data relational database storage                                                 |
| TDengineDB       | Time-series database storage                                                                      |
| QuestDB          | Time-series database storage                                                                      |

﻿

## License

﻿
[Apache-2.0](https://gitee.com/diego2098/ThingsGateway/blob/master/LICENSE)
﻿

## Demo

﻿
[Demo](http://47.119.161.158:5000/)
﻿
Account: **SuperAdmin**
﻿
Password: **111111**
﻿
**In the upper-right corner, switch to the IoT Gateway module in the personal popup box**

## Docker

```shell

docker pull registry.cn-shenzhen.aliyuncs.com/thingsgateway/thingsgateway

docker pull registry.cn-shenzhen.aliyuncs.com/thingsgateway/thingsgateway_arm64
```

﻿

## Sponsorship

﻿
[Sponsorship Approach](https://thingsgateway.cn/docs/1000)
﻿

## Community

﻿
QQ Group: 605534569 [Jump](http://qm.qq.com/cgi-bin/qm/qr?_wv=1027&k=NnBjPO-8kcNFzo_RzSbdICflb97u2O1i&authKey=V1MI3iJtpDMHc08myszP262kDykbx2Yev6ebE4Me0elTe0P0IFAmtU5l7Sy5w0jx&noverify=0&group_code=605534569)
﻿

## Pro Plugins

﻿
[Plugin List](https://thingsgateway.cn/docs/1001)
