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

using LiteDB;

using Microsoft.Extensions.Logging;

using RabbitMQ.Client;

using ThingsGateway.Foundation.Extension.ConcurrentQueue;
using ThingsGateway.Foundation.Extension.Generic;
using ThingsGateway.Foundation.Sockets;

namespace ThingsGateway.Plugin.RabbitMQ;

/// <summary>
/// Kafka消息生产
/// </summary>
public partial class RabbitMQProducer : UpLoadBaseWithCacheT<DeviceData, VariableData>
{
    /// <inheritdoc/>
    public override Type DriverDebugUIType => null;

    public override Type DriverUIType => null;

    /// <inheritdoc/>
    public override VariablePropertyBase VariablePropertys => _variablePropertys;

    protected override IReadWrite _readWrite => null;

    protected override UploadPropertyWithCacheT _uploadPropertyWithCache => _driverPropertys;


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
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
        _collectDeviceRunTimes.Clear();
        _collectVariableRunTimes.Clear();
        base.Dispose(disposing);
    }

    protected override void Init(ISenderClient client = null)
    {
        base.Init(client);


        #region Kafka 生产者

        _connectionFactory = new ConnectionFactory
        {
            HostName = _driverPropertys.IP,
            Port = _driverPropertys.Port,
            UserName = _driverPropertys.UserName,
            Password = _driverPropertys.Password,
            VirtualHost = _driverPropertys.VirtualHost,
        };

        #endregion

    }


    protected override async Task ProtectedExecuteAsync(CancellationToken cancellationToken)
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
            }
        }


        var cacheItems = new List<CacheItem>();

        try
        {
            var list = _collectVariableRunTimes.ToListWithDequeue();
            if (list?.Count != 0)
            {
                var data = list.ChunkBetter(_driverPropertys.SplitSize);
                foreach (var item in data)
                {
                    try
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            string json = item.GetSciptListValue(_driverPropertys.BigTextScriptVariableModel);
                            var result = Publish($"{_driverPropertys.VariableTopic}", json, _model?.CreateBasicProperties());
                            if (success != result.IsSuccess)
                            {
                                if (!result.IsSuccess)
                                    LogMessage.Warning(result.ToString());
                                success = result.IsSuccess;
                            }
                            if (!result.IsSuccess)
                            {
                                AddVarCahce(cacheItems, $"{_driverPropertys.VariableTopic}", json);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogMessage?.LogWarning(ex);
                    }

                }
            }

        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex);
        }

        try
        {
            var list = _collectDeviceRunTimes.ToListWithDequeue();
            if (list?.Count != 0)
            {
                var data = list.ChunkBetter(_driverPropertys.SplitSize);
                foreach (var item in data)
                {
                    try
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            string json = item.GetSciptListValue(_driverPropertys.BigTextScriptDeviceModel);
                            var result = Publish($"{_driverPropertys.DeviceTopic}", json, _model?.CreateBasicProperties());
                            if (success != result.IsSuccess)
                            {
                                if (!result.IsSuccess)
                                    LogMessage.Warning(result.ToString());
                                success = result.IsSuccess;
                            }
                            if (!result.IsSuccess)
                            {

                                AddDevCache(cacheItems, $"{_driverPropertys.DeviceTopic}", json);

                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogMessage?.LogWarning(ex);
                    }

                }
            }

        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex);
        }

        if (cacheItems.Count > 0)
            CacheDb.Cache.Insert(cacheItems);

        List<long> successIds = new();
        try
        {
            var varList = CacheDb.Cache.Find(a => a.Type == varType, 0, 100).ToList();
            {
                foreach (var item in varList)
                {
                    try
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            var result = Publish($"{_driverPropertys.VariableTopic}", item.Value, _model?.CreateBasicProperties());
                            if (success != result.IsSuccess)
                            {
                                if (!result.IsSuccess)
                                    LogMessage.Warning(result.ToString());
                                success = result.IsSuccess;
                            }
                            if (result.IsSuccess)
                                successIds.Add(item.Id);
                        }
                        else
                        {
                            break;
                        }

                    }
                    catch (Exception ex)
                    {
                        LogMessage?.LogWarning(ex);
                    }
                }

            }
        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex);
        }

        try
        {

            var devList = CacheDb.Cache.Find(a => a.Type == devType, 0, 100).ToList();
            {
                foreach (var item in devList)
                {
                    try
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            var result = Publish($"{_driverPropertys.DeviceTopic}", item.Value, _model?.CreateBasicProperties());
                            if (success != result.IsSuccess)
                            {
                                if (!result.IsSuccess)
                                    LogMessage.Warning(result.ToString());
                                success = result.IsSuccess;
                            }
                            if (result.IsSuccess)
                                successIds.Add(item.Id);
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogMessage?.LogWarning(ex);
                    }
                }
            }



        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex);
        }
        if (successIds.Count > 0)
            CacheDb.Cache.DeleteMany(a => successIds.Contains(a.Id));

        await Delay(_driverPropertys.CycleInterval, cancellationToken);
    }
}
