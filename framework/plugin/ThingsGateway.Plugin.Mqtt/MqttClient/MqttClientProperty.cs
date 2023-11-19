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
public class MqttClientProperty : UploadPropertyWithCacheT
{

    /// <summary>
    /// IP
    /// </summary>
    [DeviceProperty("IP", "")]
    public override string IP { get; set; } = "127.0.0.1";
    /// <summary>
    /// 端口
    /// </summary>
    [DeviceProperty("端口", "")]
    public override int Port { get; set; } = 1883;
    /// <summary>
    /// 账号
    /// </summary>
    [DeviceProperty("账号", "")]
    public string UserName { get; set; } = "admin";
    /// <summary>
    /// 密码
    /// </summary>
    [DeviceProperty("密码", "")]
    public string Password { get; set; } = "123456";
    /// <summary>
    /// 连接Id
    /// </summary>
    [DeviceProperty("连接Id", "")]
    public string ConnectId { get; set; } = "ThingsGatewayId";
    /// <summary>
    /// 连接超时时间
    /// </summary>
    [DeviceProperty("连接超时时间", "")]
    public int ConnectTimeOut { get; set; } = 3000;

    /// <summary>
    /// 允许Rpc写入
    /// </summary>
    [DeviceProperty("允许Rpc写入", "")]
    public bool DeviceRpcEnable { get; set; }
    /// <summary>
    /// 数据请求RpcTopic
    /// </summary>
    [DeviceProperty("数据请求RpcTopic", "这个主题接收到任何数据都会把全部的信息发送到变量/设备主题中")]
    public string QuestRpcTopic { get; set; } = "ThingsGateway/Quest";

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
    [DeviceProperty("Rpc写入Topic", "")]
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
