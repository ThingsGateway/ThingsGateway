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

using NewLife;

namespace ThingsGateway.Plugin.Mqtt;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class MqttServerProperty : BusinessPropertyWithCacheIntervalScript
{
    /// <summary>
    /// IP
    /// </summary>
    [DynamicProperty("IP", "")]
    public string IP { get; set; } = "127.0.0.1";

    /// <summary>
    /// 端口
    /// </summary>
    [DynamicProperty("端口", "")]
    public int Port { get; set; } = 1883;

    /// <summary>
    /// WebSocket端口
    /// </summary>
    [DynamicProperty("WebSocket端口", "")]
    public int WebSocketPort { get; set; } = 8083;

    /// <summary>
    /// 允许连接的ID(前缀)
    /// </summary>
    [DynamicProperty("允许连接的ID(前缀)", "")]
    public string StartWithId { get; set; } = "ThingsGatewayId";

    /// <summary>
    /// 允许Rpc写入
    /// </summary>
    [DynamicProperty("允许Rpc写入", "")]
    public bool DeviceRpcEnable { get; set; }

    private string rpcwriteTopic = "RpcWrite";

    /// <summary>
    /// Rpc写入Topic
    /// </summary>
    [DynamicProperty("Rpc写入Topic", "实际的写入主题为固定通配 {ThingsGateway.Rpc/+/[RpcWrite]} ,其中RpcWrite为该属性填入内容，+通配符是不固定GUID值，每次执行写入时会在不同的主题中返回；返回结果主题会在主题后添加Response , 也就是{ThingsGateway.Rpc/+/[RpcWrite]/Response}")]
    public string RpcWriteTopic
    {
        get
        { return rpcwriteTopic; }
        set
        {
            if (value.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(RpcWriteTopic));
            }

            if (value.Contains("/") || value.Contains("+") || value.Contains("#"))
            {
                throw new ArgumentException("值不能包含  / + # ");
            }
            rpcwriteTopic = value;
        }
    }
}