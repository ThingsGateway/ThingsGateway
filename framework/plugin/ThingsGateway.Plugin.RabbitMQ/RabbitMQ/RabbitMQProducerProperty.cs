#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using RabbitMQ.Client;

namespace ThingsGateway.Plugin.RabbitMQ;

/// <summary>
/// kafka 生产者属性
/// </summary>
public class RabbitMQProducerProperty : UploadPropertyWithCacheT
{
    /// <summary>
    /// IP
    /// </summary>
    [DeviceProperty("IP", "")]
    public override string IP { get; set; } = "localhost";
    /// <summary>
    /// 端口
    /// </summary>
    [DeviceProperty("端口", "")]
    public override int Port { get; set; } = 5672;
    /// <summary>
    /// UserName
    /// </summary>
    [DeviceProperty("账号", "")]
    public string UserName { get; set; } = "guest";
    /// <summary>
    /// Password
    /// </summary>
    [DeviceProperty("密码", "")]
    public string Password { get; set; } = "guest";

    /// <summary>
    /// IsQueueDeclare
    /// </summary>
    [DeviceProperty("是否声明队列", "")]
    public bool IsQueueDeclare { get; set; } = false;

    /// <summary>
    /// VirtualHost
    /// </summary>
    [DeviceProperty("虚拟Host", "")]
    public string VirtualHost { get; set; } = ConnectionFactory.DefaultVHost;

    ///// <summary>
    ///// RoutingKey
    ///// </summary>
    //[DeviceProperty("路由名称", "")]
    //public string RoutingKey { get; set; } = "TG";

    /// <summary>
    /// 交换机名称
    /// </summary>
    [DeviceProperty("交换机名称", "")]
    public string ExchangeName { get; set; } = "";
    /// <summary>
    /// 设备主题
    /// </summary>
    [DeviceProperty("设备主题", "")]
    public string DeviceTopic { get; set; } = "test1";
    /// <summary>
    /// 变量主题
    /// </summary>
    [DeviceProperty("变量主题", "")]
    public string VariableTopic { get; set; } = "test2";

    /// <summary>
    /// 设备实体脚本
    /// </summary>
    [DeviceProperty("设备实体脚本", "查看文档说明，为空时默认Json传输")]
    public string BigTextScriptDeviceModel { get; set; }
    /// <summary>
    /// 变量实体脚本
    /// </summary>
    [DeviceProperty("变量实体脚本", "查看文档说明，为空时默认Json传输")]
    public string BigTextScriptVariableModel { get; set; }
}
