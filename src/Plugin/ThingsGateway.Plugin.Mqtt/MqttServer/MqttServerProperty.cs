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
public class MqttServerProperty : BusinessPropertyWithCacheIntervalScript
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
    /// WebSocket端口
    /// </summary>
    [DynamicProperty]
    public int WebSocketPort { get; set; } = 8083;

    /// <summary>
    /// 允许连接的ID(前缀)
    /// </summary>
    [DynamicProperty]
    public string StartWithId { get; set; } = "ThingsGatewayId";

    /// <summary>
    /// 允许Rpc写入
    /// </summary>
    [DynamicProperty]
    public bool DeviceRpcEnable { get; set; }


    /// <summary>
    /// Rpc写入Topic
    /// </summary>
    [DynamicProperty(Remark = "实际的写入主题为固定通配 {RpcWrite/+} ，其中RpcWrite为该属性填入内容，+通配符是请求GUID值；返回结果主题会在主题后添加Response , 也就是{RpcWrite/+/Response}")]
    public string RpcWriteTopic { get; set; }
}
