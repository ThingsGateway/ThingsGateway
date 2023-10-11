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


using System.ComponentModel;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// ManageGatewayConfig
/// </summary>
public class ManageGatewayConfig
{
    /// <summary>
    /// 启用
    /// </summary>
    [Description("启用")]
    public bool Enable { get; set; }
    /// <summary>
    /// MqttBrokerIP
    /// </summary>
    [Description("Mqtt-Tcp IP")]
    public string MqttBrokerIP { get; set; }

    /// <summary>
    /// MqttBrokerPort
    /// </summary>
    [Description("Mqtt-Tcp 端口")]
    public int MqttBrokerPort { get; set; }

    /// <summary>
    /// UserName
    /// </summary>
    [Description("Mqtt用户名")]
    public string UserName { get; set; }

    /// <summary>
    /// Password
    /// </summary>
    [Description("Mqtt密码")]
    public string Password { get; set; }

    /// <summary>
    /// WriteRpcTopic，Rpc返回为{WriteRpcTopic}/Return，只有这个topic才开放外部订阅权限
    /// </summary>
    [Description("变量写入Rpc主题")]
    public string WriteRpcTopic { get; set; }

    /// <summary>
    /// DBDownTopic
    /// </summary>
    [Description("配置下发Rpc主题")]
    public string DBDownTopic { get; set; }

    /// <summary>
    /// DBUploadTopic
    /// </summary>
    [Description("配置上传Rpc主题")]
    public string DBUploadTopic { get; set; }

}

/// <summary>
/// ClientGatewayConfig
/// </summary>
public class ClientGatewayConfig : ManageGatewayConfig
{
    /// <summary>
    /// 标识
    /// </summary>
    [Description("子网关标识ID")]
    public string GatewayId { get; set; }

}
/// <summary>
/// 用于Mqtt Json传输，上传/下载配置信息
/// </summary>
public class MqttDBUploadRpcResult
{

    /// <summary>
    /// 采集设备
    /// </summary>
    public List<CollectDevice> CollectDevices { get; set; } = new();

    /// <summary>
    /// 上传设备
    /// </summary>
    public List<UploadDevice> UploadDevices { get; set; } = new();

    /// <summary>
    /// 变量
    /// </summary>
    public List<DeviceVariable> DeviceVariables { get; set; } = new();

}
/// <summary>
/// 用于Mqtt Json传输，上传/下载配置信息
/// </summary>
public class MqttDBDownRpc
{
    /// <summary>
    /// 采集设备
    /// </summary>
    public byte[] CollectDevices { get; set; }

    /// <summary>
    /// 上传设备
    /// </summary>
    public byte[] UploadDevices { get; set; }

    /// <summary>
    /// 变量
    /// </summary>
    public byte[] DeviceVariables { get; set; }

    /// <summary>
    /// true=>删除全部后增加
    /// </summary>
    [Description("是否删除原采集设备表")]
    public bool IsCollectDevicesFullUp { get; set; }
    /// <summary>
    /// true=>删除全部后增加
    /// </summary>
    [Description("是否删除原上传设备表")]
    public bool IsUploadDevicesFullUp { get; set; }
    /// <summary>
    /// true=>删除全部后增加
    /// </summary>
    [Description("是否删除原变量表")]
    public bool IsDeviceVariablesFullUp { get; set; }

    /// <summary>
    /// 是否立即重启，使配置生效
    /// </summary>
    [Description("是否重启子网关线程")]
    public bool IsRestart { get; set; }


}


/// <summary>
/// MqttRpc传入
/// </summary>
public class ManageMqttRpcFrom
{
    /// <summary>
    /// 标识
    /// </summary>
    public string GatewayId { get; set; }

    /// <summary>
    /// 标识
    /// </summary>
    public string RpcId { get; set; }
    /// <summary>
    /// "WriteInfos":{"test":"1"}
    /// </summary>
    public Dictionary<string, string> WriteInfos { get; set; } = new();
}
/// <summary>
/// MqttRpc输出
/// </summary>
public class ManageMqttRpcResult
{
    /// <summary>
    /// 标识
    /// </summary>
    public string GatewayId { get; set; }
    /// <summary>
    /// 标识
    /// </summary>
    public string RpcId { get; set; }
    /// <summary>
    /// 消息
    /// </summary>
    public Dictionary<string, OperResult> Message { get; set; } = new();
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }
}
