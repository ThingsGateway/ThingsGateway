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
public class MqttClientProperty : BusinessPropertyWithCacheIntervalScript
{
    /// <summary>
    /// IP
    /// </summary>
    [DynamicProperty]
    public string IP { get; set; } = "127.0.0.1";

    /// <summary>
    /// 端口
    /// </summary>
    [DynamicProperty]
    public int Port { get; set; } = 1883;

    /// <summary>
    /// 是否显示详细日志
    /// </summary>
    [DynamicProperty]
    public bool DetailLog { get; set; } = true;

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

    /// <summary>
    /// 允许Rpc写入
    /// </summary>
    [DynamicProperty]
    public bool DeviceRpcEnable { get; set; }

    /// <summary>
    /// Rpc写入Topic
    /// </summary>
    [DynamicProperty(Remark = "支持ThingsGateway自定义格式、thingsboard gateway格式，查阅文档获取详细说明")]
    public string RpcWriteTopic { get; set; }

    /// <summary>
    /// 数据请求Topic
    /// </summary>
    [DynamicProperty(Remark = "这个主题接收到任何数据都会把全部的信息发送到变量/设备/报警主题中")]
    public string RpcQuestTopic { get; set; }
}
