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


using ThingsGateway.Foundation;

namespace ThingsGateway.Application;

/// <summary>
/// ManageGatewayConfig
/// </summary>
public class ManageGatewayConfig
{
    /// <summary>
    /// 是否管理网关
    /// </summary>
    public bool Enable { get; set; }
    /// <summary>
    /// MqttBrokerIP
    /// </summary>
    public string MqttBrokerIP { get; set; }

    /// <summary>
    /// MqttBrokerPort
    /// </summary>
    public int MqttBrokerPort { get; set; }

    /// <summary>
    /// UserName
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Password
    /// </summary>
    public string Password { get; set; }


    /// <summary>
    /// DBDownTopic，Rpc返回为{DBDownTopic}/Return
    /// </summary>
    public string DBDownTopic { get; set; }

    /// <summary>
    /// DBUploadTopic，Rpc返回为{DBUploadTopic}/Return
    /// </summary>
    public string DBUploadTopic { get; set; }

    /// <summary>
    /// WriteRpcTopic，Rpc返回为{WriteRpcTopic}/Return
    /// </summary>
    public string WriteRpcTopic { get; set; }
}


/// <summary>
/// 用于Mqtt Json传输，上传/下载配置信息
/// </summary>
public class MqttDB
{
    /// <summary>
    /// 标识
    /// </summary>
    public string GatewayId { get; set; }
    /// <summary>
    /// 采集设备
    /// </summary>
    public List<CollectDevice> CollectDevices { get; set; }
    /// <summary>
    /// true=>删除全部后增加
    /// </summary>
    public bool IsCollectDevicesFullUp { get; set; }
    /// <summary>
    /// 上传设备
    /// </summary>
    public List<UploadDevice> UploadDevices { get; set; }
    /// <summary>
    /// true=>删除全部后增加
    /// </summary>
    public bool IsUploadDevicesFullUp { get; set; }
    /// <summary>
    /// 变量
    /// </summary>
    public List<DeviceVariable> DeviceVariables { get; set; }
    /// <summary>
    /// true=>删除全部后增加
    /// </summary>
    public bool IsDeviceVariablesFullUp { get; set; }
    /// <summary>
    /// 配置项
    /// </summary>
    public List<SysConfig> SysConfigs { get; set; }
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
