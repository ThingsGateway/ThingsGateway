//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Confluent.Kafka;

namespace ThingsGateway.Plugin.Kafka;

/// <summary>
/// kafka 生产者属性
/// </summary>
public class KafkaProducerProperty : BusinessPropertyWithCacheIntervalScript
{
    /// <summary>
    /// 服务地址
    /// </summary>
    [DynamicProperty("服务地址", "")]
    public string BootStrapServers { get; set; } = "127.0.0.1:9092";

    /// <summary>
    /// 发布超时时间
    /// </summary>
    [DynamicProperty("发布超时时间", "ms")]
    public int Timeout { get; set; } = 5000;

    /// <summary>
    /// 用户名
    /// </summary>
    [DynamicProperty("用户名", "")]
    public string? SaslUsername { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    [DynamicProperty("密码", "")]
    public string? SaslPassword { get; set; }

    [DynamicProperty("SecurityProtocol", "Plaintext, Ssl, SaslPlaintext, SaslSsl")]
    public SecurityProtocol SecurityProtocol { get; set; } = SecurityProtocol.Plaintext;

    [DynamicProperty("SaslMechanism", " Gssapi, Plain, ScramSha256, ScramSha512, OAuthBearer")]
    public SaslMechanism SaslMechanism { get; set; } = SaslMechanism.Plain;
}