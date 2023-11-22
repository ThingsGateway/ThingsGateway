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

using Confluent.Kafka;

namespace ThingsGateway.Plugin.Kafka;

/// <summary>
/// kafka 生产者属性
/// </summary>
public class KafkaProducerProperty : UploadPropertyWithCacheT
{
    /// <summary>
    /// 服务地址
    /// </summary>
    [DeviceProperty("服务地址", "")]
    public string BootStrapServers { get; set; } = "127.0.0.1:9092";
    /// <summary>
    /// 设备主题
    /// </summary>
    [DeviceProperty("设备主题", "")]
    public string DeviceTopic { get; set; } = "test1";
    /// <summary>
    /// 变量主题
    /// </summary>
    [DeviceProperty("变量主题", "")]
    public string VariableTopic { get; set; } = "test2";
    /// <summary>
    /// 客户端ID
    /// </summary>
    [DeviceProperty("客户端ID", "")]
    public string ClientId { get; set; } = "test-consumer";
    /// <summary>
    /// 发布超时时间
    /// </summary>
    [DeviceProperty("发布超时时间", "ms")]
    public int TimeOut { get; set; } = 5000;

    /// <summary>
    /// 用户名
    /// </summary>
    [DeviceProperty("用户名", "")]
    public string SaslUsername { get; set; } = "none";
    /// <summary>
    /// 密码
    /// </summary>
    [DeviceProperty("密码", "")]
    public string SaslPassword { get; set; } = "none";
    [DeviceProperty("SecurityProtocol", "Plaintext, Ssl, SaslPlaintext, SaslSsl")]
    public SecurityProtocol SecurityProtocol { get; set; } = SecurityProtocol.Plaintext;
    [DeviceProperty("SaslMechanism", " Gssapi, Plain, ScramSha256, ScramSha512, OAuthBearer")]
    public SaslMechanism SaslMechanism { get; set; } = SaslMechanism.Plain;

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
