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


namespace ThingsGateway.Application;

/// <summary>
/// MqttBrokerConfig
/// </summary>
public class MqttBrokerConfig
{
    /// <summary>
    /// 是否启用管理网关
    /// </summary>
    public bool IsMqttBroker { get; set; }

    /// <summary>
    /// MqttBrokerIP
    /// </summary>
    public string MqttBrokerIP { get; set; }

    /// <summary>
    /// MqttBrokerPort
    /// </summary>
    public int MqttBrokerPort { get; set; }



}

