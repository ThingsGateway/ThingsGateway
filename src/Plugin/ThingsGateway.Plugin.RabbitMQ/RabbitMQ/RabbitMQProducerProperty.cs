//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using RabbitMQ.Client;

namespace ThingsGateway.Plugin.RabbitMQ;

/// <summary>
/// RabbitMQProducerProperty
/// </summary>
public class RabbitMQProducerProperty : BusinessPropertyWithCacheIntervalScript
{
    /// <summary>
    /// IP
    /// </summary>
    [DynamicProperty]
    public string IP { get; set; } = "localhost";

    /// <summary>
    /// 端口
    /// </summary>
    [DynamicProperty]
    public int Port { get; set; } = 5672;

    /// <summary>
    /// UserName
    /// </summary>
    [DynamicProperty]
    public string UserName { get; set; } = "guest";

    /// <summary>
    /// Password
    /// </summary>
    [DynamicProperty]
    public string Password { get; set; } = "guest";
    /// <summary>
    /// 是否显示详细日志
    /// </summary>
    [DynamicProperty]
    public bool DetailLog { get; set; } = true;
    /// <summary>
    /// IsQueueDeclare
    /// </summary>
    [DynamicProperty]
    public bool IsQueueDeclare { get; set; } = false;

    /// <summary>
    /// VirtualHost
    /// </summary>
    [DynamicProperty]
    public string VirtualHost { get; set; } = ConnectionFactory.DefaultVHost;

    ///// <summary>
    ///// RoutingKey
    ///// </summary>
    //[DynamicProperty("路由名称", "")]
    //public string RoutingKey { get; set; } = "TG";

    /// <summary>
    /// 交换机名称
    /// </summary>
    [DynamicProperty]
    public string? ExchangeName { get; set; } = "";
}
