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
/// <inheritdoc/>
/// </summary>
public class RabbitMQClientProperty : UpDriverPropertyBase
{
    /// <summary>
    /// IP
    /// </summary>
    [DeviceProperty("IP", "")]
    public string IP { get; set; } = "localhost";
    /// <summary>
    /// 端口
    /// </summary>
    [DeviceProperty("端口", "")]
    public int Port { get; set; } = 5672;
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
    /// 是否发布List
    /// </summary>
    [DeviceProperty("是否发布List", "")]
    public bool IsList { get; set; } = false;
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
    /// 变量队列名称
    /// </summary>
    [DeviceProperty("变量队列名称", "")]
    public string VariableQueueName { get; set; } = "ThingsGateway/Variable";
    /// <summary>
    /// 设备队列名称
    /// </summary>
    [DeviceProperty("设备队列名称", "")]
    public string DeviceQueueName { get; set; } = "ThingsGateway/Device";
    /// <summary>
    /// 线程循环间隔
    /// </summary>
    [DeviceProperty("线程循环间隔", "最小10ms")]
    public int CycleInterval { get; set; } = 1000;
    /// <summary>
    /// 缓存最大条数
    /// </summary>
    [DeviceProperty("缓存最大条数", "默认2千条")]
    public int CacheMaxCount { get; set; } = 2000;
    /// <summary>
    /// 列表分割大小
    /// </summary>
    [DeviceProperty("列表分割大小", "默认1千条")]
    public int SplitSize { get; set; } = 1000;
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

    /// <summary>
    /// 是否间隔上传
    /// </summary>
    [DeviceProperty("是否间隔上传", "False时为变化检测上传")]
    public bool IsInterval { get; set; } = false;
    /// <summary>
    /// 上传间隔时间
    /// </summary>
    [DeviceProperty("上传间隔时间", "最小1000ms")]
    public int UploadInterval { get; set; } = 1000;

}
