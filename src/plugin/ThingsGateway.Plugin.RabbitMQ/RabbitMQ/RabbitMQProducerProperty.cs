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
/// RabbitMQProducerProperty
/// </summary>
public class RabbitMQProducerProperty : BusinessPropertyWithCacheIntervalScript
{
    /// <summary>
    /// IP
    /// </summary>
    [DynamicProperty("IP", "")]
    public string IP { get; set; } = "localhost";

    /// <summary>
    /// 端口
    /// </summary>
    [DynamicProperty("端口", "")]
    public int Port { get; set; } = 5672;

    /// <summary>
    /// UserName
    /// </summary>
    [DynamicProperty("账号", "")]
    public string UserName { get; set; } = "guest";

    /// <summary>
    /// Password
    /// </summary>
    [DynamicProperty("密码", "")]
    public string Password { get; set; } = "guest";

    /// <summary>
    /// IsQueueDeclare
    /// </summary>
    [DynamicProperty("是否声明队列", "不支持动态通配符主题")]
    public bool IsQueueDeclare { get; set; } = false;

    /// <summary>
    /// VirtualHost
    /// </summary>
    [DynamicProperty("虚拟Host", "")]
    public string VirtualHost { get; set; } = ConnectionFactory.DefaultVHost;

    ///// <summary>
    ///// RoutingKey
    ///// </summary>
    //[DynamicProperty("路由名称", "")]
    //public string RoutingKey { get; set; } = "TG";

    /// <summary>
    /// 交换机名称
    /// </summary>
    [DynamicProperty("交换机名称", "")]
    public string? ExchangeName { get; set; } = "";
}