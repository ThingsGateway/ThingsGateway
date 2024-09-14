//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using RabbitMQ.Client;

using ThingsGateway.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.Plugin.RabbitMQ;

/// <summary>
/// RabbitMQProducer
/// </summary>
public partial class RabbitMQProducer : BusinessBaseWithCacheIntervalScript<VariableData, DeviceData, AlarmVariable>
{
    private readonly RabbitMQProducerProperty _driverPropertys = new();
    private readonly RabbitMQProducerVariableProperty _variablePropertys = new();
    public override VariablePropertyBase VariablePropertys => _variablePropertys;

    protected override BusinessPropertyWithCacheIntervalScript _businessPropertyWithCacheIntervalScript => _driverPropertys;

    protected override void Init(IChannel? channel = null)
    {
        base.Init(channel);

        #region 初始化

        _connectionFactory = new ConnectionFactory
        {
            HostName = _driverPropertys.IP,
            Port = _driverPropertys.Port,
            UserName = _driverPropertys.UserName,
            Password = _driverPropertys.Password,
            VirtualHost = _driverPropertys.VirtualHost,
        };

        #endregion 初始化
    }

    /// <inheritdoc/>
    public override bool IsConnected() => success;

    /// <inheritdoc/>
    public override string ToString()
    {
        return $" {nameof(RabbitMQProducer)} IP:{_driverPropertys.IP} Port:{_driverPropertys.Port}";
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        _model?.SafeDispose();
        _connection?.SafeDispose();
        base.Dispose(disposing);
    }

    protected override async ValueTask ProtectedExecuteAsync(CancellationToken cancellationToken)
    {
        if (_model == null)
        {
            try
            {
                // 创建连接
                _connection ??= _connectionFactory.CreateConnection();
                // 创建通道
                _model ??= _connection.CreateModel();
                // 声明路由队列
                if (_driverPropertys.IsQueueDeclare)
                {
                    _model?.QueueDeclare(_driverPropertys.VariableTopic, true, false, false);
                    _model?.QueueDeclare(_driverPropertys.DeviceTopic, true, false, false);
                    _model?.QueueDeclare(_driverPropertys.AlarmTopic, true, false, false);
                }
                success = true;
            }
            catch (Exception ex)
            {
                if (success)
                {
                    LogMessage?.LogWarning(ex);
                    success = false;
                }
                await Task.Delay(10000, cancellationToken).ConfigureAwait(false);
            }
        }
        else
        {
            await Update(cancellationToken).ConfigureAwait(false);
        }

        await Delay(cancellationToken).ConfigureAwait(false);
    }
}
