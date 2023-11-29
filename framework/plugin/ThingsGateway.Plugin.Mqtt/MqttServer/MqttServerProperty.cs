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

namespace ThingsGateway.Plugin.Mqtt;
/// <summary>
/// <inheritdoc/>
/// </summary>
public class MqttServerProperty : UpDriverPropertyBase
{
    [DeviceProperty("是否选择全部变量", "")] public bool IsAllVariable { get; set; } = false;

    /// <summary>
    /// 端口
    /// </summary>
    [DeviceProperty("端口", "")]
    public override int Port { get; set; } = 1883;
    /// <summary>
    /// WebSocket端口
    /// </summary>
    [DeviceProperty("WebSocket端口", "")]
    public int WebSocketPort { get; set; } = 8083;
    /// <summary>
    /// 允许连接的ID(前缀)
    /// </summary>
    [DeviceProperty("允许连接的ID(前缀)", "")]
    public string StartWithId { get; set; } = "ThingsGatewayId";
    /// <summary>
    /// 允许Rpc写入
    /// </summary>
    [DeviceProperty("允许Rpc写入", "")]
    public bool DeviceRpcEnable { get; set; }
    /// <summary>
    /// 列表分割大小
    /// </summary>
    [DeviceProperty("列表分割大小", "默认1千条")]
    public int SplitSize { get; set; } = 1000;
    /// <summary>
    /// 线程循环间隔
    /// </summary>
    [DeviceProperty("线程循环间隔", "最小10ms")]
    public int CycleInterval { get; set; } = 1000;
    /// <summary>
    /// 设备Topic
    /// </summary>
    [DeviceProperty("设备Topic", "")]
    public string DeviceTopic { get; set; } = "ThingsGateway/Device";
    /// <summary>
    /// 变量Topic
    /// </summary>
    [DeviceProperty("变量Topic", "")]
    public string VariableTopic { get; set; } = "ThingsGateway/Variable";
    /// <summary>
    /// Rpc返回Topic
    /// </summary>
    [DeviceProperty("Rpc返回Topic", "")]
    public string RpcSubTopic { get; set; } = "ThingsGateway/RpcSub";
    /// <summary>
    /// Rpc写入Topic
    /// </summary>
    [DeviceProperty("Rpc写入Topic", "不允许订阅")]
    public string RpcWriteTopic { get; set; } = "ThingsGateway/RpcWrite";
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
