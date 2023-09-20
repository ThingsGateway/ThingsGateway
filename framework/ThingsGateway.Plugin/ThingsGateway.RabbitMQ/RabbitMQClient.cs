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

using Furion;

using Mapster;

using Microsoft.Extensions.Logging;

using RabbitMQ.Client;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ThingsGateway.Application;
using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Extension.ConcurrentQueue;
using ThingsGateway.Foundation.Extension.Generic;

using TouchSocket.Core;

namespace ThingsGateway.RabbitMQ;
/// <summary>
/// RabbitMQ
/// </summary>
public class RabbitMQClient : UpLoadBase
{
    private readonly RabbitMQClientProperty driverPropertys = new();
    private readonly RabbitMQClientVariableProperty variablePropertys = new();
    private ConcurrentQueue<DeviceData> _collectDeviceRunTimes = new();
    private ConcurrentQueue<VariableData> _collectVariableRunTimes = new();
    private IConnection _connection;
    private ConnectionFactory _connectionFactory;
    private GlobalDeviceData _globalDeviceData;
    private IModel _model;
    private RpcSingletonService _rpcCore;
    private List<DeviceVariableRunTime> _uploadVariables = new();
    /// <inheritdoc/>
    public override Type DriverDebugUIType => null;
    /// <inheritdoc/>
    public override UpDriverPropertyBase DriverPropertys => driverPropertys;

    /// <inheritdoc/>
    public override List<DeviceVariableRunTime> UploadVariables => _uploadVariables;

    /// <inheritdoc/>
    public override VariablePropertyBase VariablePropertys => variablePropertys;


    /// <inheritdoc/>
    public override Task AfterStopAsync()
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public override Task BeforStartAsync(CancellationToken token)
    {
        return Task.CompletedTask;
    }

    private TimerTick exDeviceTimerTick;

    private TimerTick exVariableTimerTick;
    /// <inheritdoc/>
    public override async Task ExecuteAsync(CancellationToken token)
    {
        try
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
                    if (driverPropertys.IsQueueDeclare)
                    {
                        _model?.QueueDeclare(driverPropertys.VariableQueueName, true, false, false);
                        _model?.QueueDeclare(driverPropertys.DeviceQueueName, true, false, false);
                    }


                }
                catch (Exception ex)
                {
                    LogMessage?.LogWarning(ex, ToString());
                }
            }

            if (!driverPropertys.IsInterval)
            {
                ////变化推送
                var varList = _collectVariableRunTimes.ToListWithDequeue();
                if (varList?.Count != 0)
                {
                    if (driverPropertys.IsList)
                    {
                        var listChunk = varList.ChunkTrivialBetter(driverPropertys.SplitSize);
                        foreach (var variables in listChunk)
                        {
                            try
                            {
                                if (!token.IsCancellationRequested)
                                {
                                    var data = variables.GetSciptListValue(driverPropertys.BigTextScriptVariableModel);
                                    // 设置消息持久化
                                    IBasicProperties properties = _model?.CreateBasicProperties();
                                    await Publish(driverPropertys.VariableQueueName, data, properties);
                                }
                                else
                                {
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                LogMessage?.LogWarning(ex, ToString());
                            }

                        }
                    }
                    else
                    {
                        foreach (var variable in varList)
                        {
                            try
                            {
                                if (!token.IsCancellationRequested)
                                {
                                    var data = variable.GetSciptListValue(driverPropertys.BigTextScriptVariableModel);
                                    // 设置消息持久化
                                    IBasicProperties properties = _model?.CreateBasicProperties();
                                    await Publish(driverPropertys.VariableQueueName, data, properties);
                                }
                                else
                                {
                                    break;
                                }

                            }
                            catch (Exception ex)
                            {
                                LogMessage?.LogWarning(ex, ToString());
                            }
                        }

                    }

                }
            }
            else
            {
                if (exVariableTimerTick.IsTickHappen())
                {
                    try
                    {
                        var varList = _uploadVariables.Adapt<List<VariableData>>();
                        if (varList?.Count != 0)
                        {
                            if (driverPropertys.IsList)
                            {
                                var listChunk = varList.ChunkTrivialBetter(driverPropertys.SplitSize);
                                foreach (var variables in listChunk)
                                {
                                    try
                                    {
                                        if (!token.IsCancellationRequested)
                                        {
                                            var data = variables.GetSciptListValue(driverPropertys.BigTextScriptVariableModel);
                                            // 设置消息持久化
                                            IBasicProperties properties = _model?.CreateBasicProperties();
                                            await Publish(driverPropertys.VariableQueueName, data, properties);
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        LogMessage?.LogWarning(ex, ToString());
                                    }

                                }
                            }
                            else
                            {
                                foreach (var variable in varList)
                                {
                                    try
                                    {
                                        if (!token.IsCancellationRequested)
                                        {
                                            var data = variable.GetSciptListValue(driverPropertys.BigTextScriptVariableModel);
                                            // 设置消息持久化
                                            IBasicProperties properties = _model?.CreateBasicProperties();
                                            await Publish(driverPropertys.VariableQueueName, data, properties);
                                        }
                                        else
                                        {
                                            break;
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        LogMessage?.LogWarning(ex, ToString());
                                    }
                                }

                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        LogMessage?.LogWarning(ex, ToString());
                    }


                }
            }
        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex, ToString());
        }
        try
        {
            if (!driverPropertys.IsInterval)
            {
                ////变化推送
                var devList = _collectDeviceRunTimes.ToListWithDequeue();
                if (devList?.Count != 0)
                {
                    if (driverPropertys.IsList)
                    {
                        var listChunk = devList.ChunkTrivialBetter(driverPropertys.SplitSize);
                        foreach (var devices in listChunk)
                        {
                            try
                            {
                                var data = devices.GetSciptListValue(driverPropertys.BigTextScriptDeviceModel);
                                // 设置消息持久化
                                IBasicProperties properties = _model?.CreateBasicProperties();
                                await Publish(driverPropertys.DeviceQueueName, data, properties);
                            }
                            catch (Exception ex)
                            {
                                LogMessage?.LogWarning(ex, ToString());
                            }

                        }
                    }
                    else
                    {
                        foreach (var devices in devList)
                        {
                            try
                            {
                                var data = devices.GetSciptListValue(driverPropertys.BigTextScriptDeviceModel);
                                // 设置消息持久化
                                IBasicProperties properties = _model?.CreateBasicProperties();
                                await Publish(driverPropertys.DeviceQueueName, data, properties);
                            }
                            catch (Exception ex)
                            {
                                LogMessage?.LogWarning(ex, ToString());
                            }
                        }

                    }

                }

            }
            else
            {
                if (exDeviceTimerTick.IsTickHappen())
                {
                    var devList = _collectDevice.Adapt<List<DeviceData>>();
                    if (devList?.Count != 0)
                    {
                        if (driverPropertys.IsList)
                        {
                            var listChunk = devList.ChunkTrivialBetter(driverPropertys.SplitSize);
                            foreach (var devices in listChunk)
                            {
                                try
                                {
                                    var data = devices.GetSciptListValue(driverPropertys.BigTextScriptDeviceModel);
                                    // 设置消息持久化
                                    IBasicProperties properties = _model?.CreateBasicProperties();
                                    await Publish(driverPropertys.DeviceQueueName, data, properties);
                                }
                                catch (Exception ex)
                                {
                                    LogMessage?.LogWarning(ex, ToString());
                                }

                            }
                        }
                        else
                        {
                            foreach (var devices in devList)
                            {
                                try
                                {
                                    var data = devices.GetSciptListValue(driverPropertys.BigTextScriptDeviceModel);
                                    // 设置消息持久化
                                    IBasicProperties properties = _model?.CreateBasicProperties();
                                    await Publish(driverPropertys.DeviceQueueName, data, properties);
                                }
                                catch (Exception ex)
                                {
                                    LogMessage?.LogWarning(ex, ToString());
                                }
                            }

                        }

                    }

                }

            }

        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex, ToString());
        }

        if (driverPropertys.CycleInterval > UploadDeviceThread.CycleInterval + 50)
        {
            try
            {
                await Task.Delay(driverPropertys.CycleInterval - UploadDeviceThread.CycleInterval, token);
            }
            catch
            {
            }
        }
        else
        {

        }

    }

    /// <inheritdoc/>
    public override bool IsConnected() => _connection?.IsOpen == true;


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $" {nameof(RabbitMQClient)} IP:{driverPropertys.IP} Port:{driverPropertys.Port}";
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        _globalDeviceData?.AllVariables.ForEach(a => a.VariableValueChange -= VariableValueChange);

        _collectDevice?.ForEach(a =>
        {
            a.DeviceStatusChange -= DeviceStatusChange;
        });
        _model?.SafeDispose();
        _connection?.SafeDispose();
        _uploadVariables = null;
        _collectDeviceRunTimes.Clear();
        _collectVariableRunTimes.Clear();
        _collectDeviceRunTimes = null;
        _collectVariableRunTimes = null;
    }
    private List<CollectDeviceRunTime> _collectDevice;

    /// <inheritdoc/>
    protected override void Init(UploadDeviceRunTime device)
    {
        _connectionFactory = new ConnectionFactory
        {
            HostName = driverPropertys.IP,
            Port = driverPropertys.Port,
            UserName = driverPropertys.UserName,
            Password = driverPropertys.Password,
            VirtualHost = driverPropertys.VirtualHost,
        };



        _globalDeviceData = App.GetService<GlobalDeviceData>();
        _rpcCore = App.GetService<RpcSingletonService>();


        var tags = _globalDeviceData.AllVariables.Where(a => a.VariablePropertys.ContainsKey(device.Id))
       .Where(b => b.VariablePropertys[device.Id].Any(c =>
       {
           if (c.PropertyName == nameof(variablePropertys.Enable))
           {
               if (c.Value?.GetBoolValue() == true)
                   return true;
               else
                   return false;
           }
           else
               return false;
       }))
       .ToList();

        _uploadVariables = tags;

        _collectDevice = _globalDeviceData.CollectDevices.Where(a => _uploadVariables.Select(b => b.DeviceId).Contains(a.Id)).ToList();

        _collectDevice.ForEach(a =>
        {
            a.DeviceStatusChange += DeviceStatusChange;
            DeviceStatusChange(a);
        });
        _uploadVariables.ForEach(a =>
        {
            a.VariableValueChange += VariableValueChange;
            VariableValueChange(a);
        });

        if (driverPropertys.UploadInterval <= 1000) driverPropertys.UploadInterval = 1000;
        exVariableTimerTick = new(driverPropertys.UploadInterval);
        exDeviceTimerTick = new(driverPropertys.UploadInterval);

    }

    private void DeviceStatusChange(CollectDeviceRunTime collectDeviceRunTime)
    {
        if (driverPropertys?.IsInterval != true)
            _collectDeviceRunTimes.Enqueue(collectDeviceRunTime.Adapt<DeviceData>());
    }

    private async Task Publish(string queueName, string data, IBasicProperties properties)
    {
        try
        {
            if (properties != null)
                properties.Persistent = true;
            if (_model != null)
                _model.BasicPublish(driverPropertys.ExchangeName, queueName, properties, Encoding.UTF8.GetBytes(data));
            else
            {
                await CacheDb.AddCacheData(queueName, data, driverPropertys.CacheMaxCount);
                return;
            }

            //连接成功时补发缓存数据
            var cacheData = await CacheDb.GetCacheData(10);
            foreach (var item in cacheData)
            {
                try
                {
                    _model?.BasicPublish(driverPropertys.ExchangeName, item.Topic, properties, Encoding.UTF8.GetBytes(item.CacheStr));
                    LogMessage.Trace(FoundationConst.LogMessageHeader + $"主题：{item.Topic}{Environment.NewLine}负载：{item.CacheStr}");

                    await CacheDb.DeleteCacheData(item.Id);
                }
                catch
                {

                }
            }
            LogMessage.Trace(FoundationConst.LogMessageHeader + $"主题：{queueName}{Environment.NewLine}负载：{data}");

        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex, ToString());
            await CacheDb.AddCacheData(queueName, data, driverPropertys.CacheMaxCount);
        }

    }
    private void VariableValueChange(DeviceVariableRunTime collectVariableRunTime)
    {
        if (driverPropertys?.IsInterval != true)
            _collectVariableRunTimes.Enqueue(collectVariableRunTime.Adapt<VariableData>());
    }
}
