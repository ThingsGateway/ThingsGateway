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
/// MqttRpc传入
/// </summary>
public class MqttRpcNameVaueWithId
{
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
public class MqttRpcResult
{
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
