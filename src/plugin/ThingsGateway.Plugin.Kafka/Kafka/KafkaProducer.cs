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

using ThingsGateway.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.Plugin.Kafka;

/// <summary>
/// Kafka消息生产
/// </summary>
public partial class KafkaProducer : BusinessBaseWithCacheIntervalScript<VariableData, DeviceData, AlarmVariable>
{
    private readonly KafkaProducerProperty _driverPropertys = new();
    private readonly KafkaProducerVariableProperty _variablePropertys = new();
    public override VariablePropertyBase VariablePropertys => _variablePropertys;

    protected override BusinessPropertyWithCacheIntervalScript _businessPropertyWithCacheIntervalScript => _driverPropertys;

    protected override void Init(IChannel? channel = null)
    {
        base.Init(channel);

        #region 初始化

        #region Kafka 生产者

        //1、生产者配置
        _producerconfig = new ProducerConfig
        {
            BootstrapServers = _driverPropertys.BootStrapServers,
            SecurityProtocol = _driverPropertys.SecurityProtocol,
            SaslMechanism = _driverPropertys.SaslMechanism,
        };
        if (!string.IsNullOrEmpty(_driverPropertys.SaslUsername))
            _producerconfig.SaslUsername = _driverPropertys.SaslUsername;
        if (!string.IsNullOrEmpty(_driverPropertys.SaslPassword))
            _producerconfig.SaslPassword = _driverPropertys.SaslPassword;

        //2、创建生产者
        _producerBuilder = new ProducerBuilder<Null, string>(_producerconfig);
        //3、错误日志监视
        _producerBuilder.SetErrorHandler((p, msg) =>
        {
            if (producerSuccess)
                LogMessage?.LogWarning(msg.Reason);
            producerSuccess = !msg.IsError;
        });

        _producer = _producerBuilder.Build();

        #endregion Kafka 生产者

        #endregion 初始化
    }

    /// <inheritdoc/>
    public override bool IsConnected() => success;

    /// <inheritdoc/>
    public override string ToString()
    {
        return $" {nameof(KafkaProducer)} :{_driverPropertys.BootStrapServers}";
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        _producer?.SafeDispose();
        base.Dispose(disposing);
    }

    protected override async ValueTask ProtectedExecuteAsync(CancellationToken cancellationToken)
    {
        await Update(cancellationToken).ConfigureAwait(false);

        await Delay(cancellationToken).ConfigureAwait(false);
    }
}
