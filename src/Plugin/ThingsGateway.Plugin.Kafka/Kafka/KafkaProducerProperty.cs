//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
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
    [DynamicProperty]
    public string BootStrapServers { get; set; } = "127.0.0.1:9092";

    /// <summary>
    /// 是否显示详细日志
    /// </summary>
    [DynamicProperty]
    public bool DetailLog { get; set; } = true;
    /// <summary>
    /// 发布超时时间
    /// </summary>
    [DynamicProperty]
    public int Timeout { get; set; } = 5000;

    /// <summary>
    /// 用户名
    /// </summary>
    [DynamicProperty]
    public string? SaslUsername { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    [DynamicProperty]
    public string? SaslPassword { get; set; }

    [DynamicProperty]
    public SecurityProtocol SecurityProtocol { get; set; } = SecurityProtocol.Plaintext;

    [DynamicProperty]
    public SaslMechanism SaslMechanism { get; set; } = SaslMechanism.Plain;
}
