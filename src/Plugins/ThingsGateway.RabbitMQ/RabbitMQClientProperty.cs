#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using RabbitMQ.Client;

using ThingsGateway.Web.Foundation;

namespace ThingsGateway.RabbitMQ;

public class RabbitMQClientProperty : UpDriverPropertyBase
{

    [DeviceProperty("IP", "")] public string IP { get; set; } = "localhost";
    [DeviceProperty("端口", "")] public int Port { get; set; } = 5672;

    [DeviceProperty("账号", "")] public string UserName { get; set; } = "guest";
    [DeviceProperty("密码", "")] public string Password { get; set; } = "guest";
    [DeviceProperty("是否发布List", "")] public bool IsList { get; set; } = false;
    [DeviceProperty("是否声明队列", "")] public bool IsQueueDeclare { get; set; } = false;
    [DeviceProperty("虚拟Host", "")] public string VirtualHost { get; set; } = ConnectionFactory.DefaultVHost;
    [DeviceProperty("路由名称", "")] public string RoutingKey { get; set; } = "TG";
    //[DeviceProperty("交换机名称", "")] public string ExchangeName { get; set; } = "RM";
    [DeviceProperty("变量队列名称", "")] public string VariableQueueName { get; set; } = "ThingsGateway/Variable";
    [DeviceProperty("设备队列名称", "")] public string DeviceQueueName { get; set; } = "ThingsGateway/Device";
    [DeviceProperty("线程循环间隔", "最小10ms")] public int CycleInterval { get; set; } = 1000;


    [DeviceProperty("设备实体脚本", "查看文档说明，为空时不起作用")] public string BigTextScriptDeviceModel { get; set; }
    [DeviceProperty("变量实体脚本", "查看文档说明，为空时不起作用")] public string BigTextScriptVariableModel { get; set; }
}
