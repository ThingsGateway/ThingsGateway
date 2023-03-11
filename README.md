<div align="center"><h1 align="center">ThingsGateway</a></h1></div>
<div align="center"><h3 align="center">边缘采集网关</h3></div>

#### 介绍

基于[ThingsBlazor](https://gitee.com/diego2098/ThingsBlazor)权限管理框架开发的跨平台边缘采集网关，支持南北端插件式开发，
动态更新插件，
并拥有较完善的北端Rpc权限管理。


####  功能亮点

- Blazor Server架构，开发部署更简单
- 采集/上传配置完全支持Excel导入导出
- 插件式驱动，方便驱动二次开发，并支持动态更新
- Modbus、OPCDA、OPCUA采集插件（支持导入节点）
- Modbus Server、Mqtt Server(Client)、OPC UAServer插件,支持Rpc写入，支持WebApi
- 支持时序数据库存储
- 实时/历史报警(Sql转储)，支持布尔/高低限值

#### nuget

- Modbus库，支持ModbusTcp、ModbusRtuOverTcp、ModbusUdp、ModbusServer等
``` powershell
 dotnet add package ThingsGateway.Foundation.Adapter.Modbus
```
- OPCDA客户端库，支持X64，支持NetCore，支持检测重连
``` powershell
 dotnet add package ThingsGateway.Foundation.Adapter.OPCDA
```
- OPCUA客户端库
``` powershell
 dotnet add package ThingsGateway.Foundation.Adapter.OPCUA
```
####  效果图
 <table>
    <tr>
        <td><img src="https://gitee.com/diego2098/ThingsGateway/raw/master/Image/1.png"/></td>
        <td><img src="https://gitee.com/diego2098/ThingsGateway/raw/master/Image/2.png"/></td>
        <td><img src="https://gitee.com/diego2098/ThingsGateway/raw/master/Image/3.png"/></td>
    </tr>
    <tr>
        <td><img src="https://gitee.com/diego2098/ThingsGateway/raw/master/Image/4.png"/></td>
        <td><img src="https://gitee.com/diego2098/ThingsGateway/raw/master/Image/5.png"/></td>
        <td><img src="https://gitee.com/diego2098/ThingsGateway/raw/master/Image/6.png"/></td>
    </tr>
        <tr>
        <td><img src="https://gitee.com/diego2098/ThingsGateway/raw/master/Image/7.png"/></td>
        <td><img src="https://gitee.com/diego2098/ThingsGateway/raw/master/Image/8.png"/></td>
        <td><img src="https://gitee.com/diego2098/ThingsGateway/raw/master/Image/9.png"/></td>
    </tr>
 </table>


 ####  文档

 使用前请查看Gitee Pages [文档站点](https://diego2098.gitee.io/thingsgateway/)

 
#### 补充说明
* 使用OPC相关插件时请遵循OPC UA基金会的授权规则

#### 开源协议

请仔细阅读 [授权协议](https://diego2098.gitee.io/thingsgateway/docs/)




####  支持作者
 如果对您有帮助，请点击右上角⭐Star关注，感谢支持开源！
 若希望捐赠项目，请跳转到文档站点查看捐赠码或使用Gitee捐赠功能
 

####  联系作者
 * QQ群：605534569
 * 邮箱：2248356998@qq.com

