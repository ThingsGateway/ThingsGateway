//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Plugin.Mqtt;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class MqttCollectProperty : CollectPropertyBase
{
    /// <summary>
    /// IP
    /// </summary>
    [DynamicProperty]
    public string IP { get; set; } = "127.0.0.1";

    /// <summary>
    /// 是否显示详细日志
    /// </summary>
    [DynamicProperty]
    public bool DetailLog { get; set; } = true;

    /// <summary>
    /// 端口
    /// </summary>
    [DynamicProperty]
    public int Port { get; set; } = 1883;

    /// <summary>
    /// 是否websocket连接
    /// </summary>
    [DynamicProperty]
    public bool IsWebSocket { get; set; } = false;

    /// <summary>
    /// WebSocketUrl
    /// </summary>
    [DynamicProperty]
    public string? WebSocketUrl { get; set; } = "ws://127.0.0.1:8083/mqtt";

    /// <summary>
    /// 账号
    /// </summary>
    [DynamicProperty]
    public string? UserName { get; set; } = "admin";

    /// <summary>
    /// 密码
    /// </summary>
    [DynamicProperty]
    public string? Password { get; set; } = "111111";

    /// <summary>
    /// 连接Id
    /// </summary>
    [DynamicProperty]
    public string ConnectId { get; set; } = "ThingsGatewayId";

    /// <summary>
    /// 连接超时时间
    /// </summary>
    [DynamicProperty]
    public int ConnectTimeout { get; set; } = 3000;

    public override int ReIntervalTime { get; set; } = 30;
    public override int RetryCount { get; set; } = 3;
}
