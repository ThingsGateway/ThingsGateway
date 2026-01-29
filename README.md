
<p align="center">
<img src="logo.svg" width = "400" height = "200" alt="The name of the image" align=center />
</p>

[![star](https://gitee.com/ThingsGateway/ThingsGateway/badge/star.svg?theme=gvp)](https://gitee.com/ThingsGateway/ThingsGateway/stargazers) 
[![star](https://img.shields.io/github/stars/ThingsGateway/ThingsGateway?logo=github)](https://github.com/ThingsGateway/ThingsGateway)
[![Ask DeepWiki](https://deepwiki.com/badge.svg)](https://deepwiki.com/ThingsGateway/ThingsGateway)﻿
[![NuGet(ThingsGateway)](https://img.shields.io/nuget/v/ThingsGateway.Foundation.svg?label=ThingsGateway)](https://www.nuget.org/packages/ThingsGateway.Foundation/)
[![NuGet(ThingsGateway)](https://img.shields.io/nuget/dt/ThingsGateway.Foundation.svg)](https://www.nuget.org/packages/ThingsGateway.Foundation/)
[![License](https://img.shields.io/badge/license-Apache%202-4EB1BA.svg)](https://thingsgateway.cn/docs/1)
[![QQ](https://img.shields.io/badge/QQ群-605534569-red)](http://qm.qq.com/cgi-bin/qm/qr?_wv=1027&k=NnBjPO-8kcNFzo_RzSbdICflb97u2O1i&authKey=V1MI3iJtpDMHc08myszP262kDykbx2Yev6ebE4Me0elTe0P0IFAmtU5l7Sy5w0jx&noverify=0&group_code=605534569)


## 📋 项目简介

**ThingsGateway** 是一个开源的工业物联网（IIoT）边缘计算网关，专注于工业现场设备数据的**高效采集、边缘处理与可靠转发**。  
项目面向真实工业场景设计，强调 **稳定性、高性能、可扩展性与工程可维护性**，适用于工业自动化、能源、电力、制造、楼宇等多种应用环境。

ThingsGateway 采用模块化与插件化架构，支持多种工业通信协议，可在边缘侧完成数据采集、预处理、协议转换与转发，有效降低系统耦合度与云端压力，是构建工业物联网系统的基础设施组件。



- [官网地址](https://thingsgateway.cn/)
- [演示地址](https://demo.thingsgateway.cn/)
- [版权声明](https://thingsgateway.cn/docs/1)
- [赞助途径](https://thingsgateway.cn/docs/1000)



---

## 🎯 核心优势

🚀 **高性能运行时**  
基于 .NET 高性能运行时与异步模型设计，设备采集支持变量智能扫描打包与批量读取，支持高并发设备连接与数据处理。

🔧 **插件化架构**  
南北向均采用插件化设计，支持按需扩展，方便二次开发与深度定制。

⚡ **边缘计算能力**  
支持在边缘侧完成数据过滤、转换、预处理与规则判断，减少无效数据上云，提升系统整体响应效率。

🔒 **工业级稳定性**  
内建断线重连、缓存与补偿机制，保障通信可靠性与数据完整性。

📦 **跨平台部署**  
支持 Windows / Linux 等环境，可灵活部署于工控机、边缘服务器。

---

## ✨ 功能特性

### 📡 数据采集

- **高性能采集**  
  异步非阻塞通信模型，支持海量设备的高效数据采集；变量自动扫描打包与批量读取，提升采集效率
- **多协议支持**  
  支持 Modbus（RTU / TCP）、OPC 、MQTT、S7 等常见协议  
- **统一设备模型**  
  通道、设备、点位分层管理，清晰映射工业现场结构  
- **高可靠通信**  
  支持心跳检测、超时控制、自动重连与异常隔离  

---

### 🔄 数据处理

- **边缘侧预处理**  
  支持数据过滤、转换、重命名、单位换算等常见工业处理逻辑  
- **结构化数据模型**  
  统一的数据抽象，便于后续北向传输、存储 
- **二次计算变量**  
  灵活的数据计算与逻辑处理，支持C#脚本扩展

---

### 📤 数据输出

- **多目标数据转发**  
  支持将数据输出至 MQTT、数据库、Web API 、Kafka 等。同时也支持建立OPCUAServer、ModbusServer等服务端功能。
- 
- **数据缓存机制**  
  网络异常时支持缓存与补偿，支持失败重试、批量发送与异常处理策略 
---

### 🎛️ 管理与运维

- **集中配置管理**  
  统一管理协议、设备、点位与运行参数  
- **运行状态监控**  
  实时查看设备状态、通信状态与系统运行情况  
- **日志与诊断**  
  提供详细日志与异常信息，便于现场排障  
- **安全与权限控制**  
  满足工业与企业内部系统的安全需求  

---


## 🧩 PRO 插件与商业支持

在保持 **ThingsGateway 核心功能完全开源** 的同时，项目还提供 **PRO 付费插件**，用于满足更复杂、更专业的工业通信与企业级应用需求。

PRO 插件主要面向对 **协议覆盖面、稳定性、现场兼容性** 要求较高的工业场景，作为开源版本的能力补充，可按需选用。

---

### 🚀 PRO 通讯协议支持

[PRO](https://thingsgateway.cn/docs/1001) 插件提供对多种 **主流工业通信协议** 的支持，包括但不限于：

- **FINS**（欧姆龙 FINS）
- **CIP / EtherNet/IP**（罗克韦尔 / Allen-Bradley）
- **MC Protocol**（三菱MC）
- **IEC 60870-5-104（IEC104 北向）**
- **OPC AE**（报警与事件）
- **VIGOR** (丰炜)
- **SECS / SECS-I / HSMS**（半导体设备通信）

---


## 相关项目仓库

默认支持net10.0/net8.0。驱动库支持netstandard2.1/netstandard2.0/net4.62/net6.0  

| 项目 | 说明 |
| :-- | :-- |
| [**ThingsGateway.Foundation**](https://gitee.com/ThingsGateway/ThingsGateway.Foundation) | 工具库、驱动接口、驱动实现 |
| [**ThingsGateway.SqlOrm**](https://gitee.com/ThingsGateway/ThingsGateway.SqlOrm) | 轻量级 ORM 库 |
| [**ThingsGateway.AspNetCore**](https://gitee.com/ThingsGateway/ThingsGateway.AspNetCore) | ASP.NET Core 工具与扩展库 |
| [**ThingsGateway.Admin**](https://gitee.com/ThingsGateway/ThingsGateway.Admin) | 后台管理系统 |
| [**Docs**](https://gitee.com/ThingsGateway/Docs) | 说明文档 |
| [**ThingsGateway.Plugin**](https://gitee.com/ThingsGateway/ThingsGateway.Plugin) | 网关插件扩展 |
| [**ThingsGateway**](https://gitee.com/ThingsGateway/ThingsGateway) | 工业网关平台 |


---

## 特别声明

ThingsGateway 项目已加入 [dotNET China](https://gitee.com/dotnetchina)  组织。<br/>

![dotnetchina](https://gitee.com/dotnetchina/home/raw/master/assets/dotnetchina-raw.png "dotNET China LOGO")
