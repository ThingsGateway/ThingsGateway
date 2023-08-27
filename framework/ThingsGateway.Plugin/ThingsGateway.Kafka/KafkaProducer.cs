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

using Mapster;



using ThingsGateway.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.Kafka;

using Furion;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using ThingsGateway.Application;
using ThingsGateway.Foundation.Extension.ConcurrentQueue;
using ThingsGateway.Foundation.Extension.Generic;

/// <summary>
/// Kafka消息生产
/// </summary>
public class KafkaProducer : UpLoadBase
{
    private readonly ConcurrentQueue<DeviceData> _collectDeviceRunTimes = new();
    private readonly ConcurrentQueue<VariableData> _collectVariableRunTimes = new();
    private readonly KafkaProducerProperty driverPropertys = new();
    private readonly KafkaProducerVariableProperty variablePropertys = new();
    private GlobalDeviceData _globalDeviceData;
    private List<DeviceVariableRunTime> _uploadVariables = new();
    private bool isSuccess = true;
    private IProducer<Null, string> producer;
    private ProducerBuilder<Null, string> producerBuilder;
    private ProducerConfig producerconfig;
    /// <inheritdoc/>
    public override Type DriverDebugUIType => null;

    /// <inheritdoc/>
    public override DriverPropertyBase DriverPropertys => driverPropertys;


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
    /// <inheritdoc/>
    public override async Task ExecuteAsync(CancellationToken token)
    {
        try
        {
            ////变化推送
            var varList = _collectVariableRunTimes.ToListWithDequeue();
            if (varList?.Count != 0)
            {
                //分解List，避免超出mqtt字节大小限制
                var varData = varList.ChunkTrivialBetter(driverPropertys.SplitSize);
                foreach (var item in varData)
                {
                    try
                    {
                        if (!token.IsCancellationRequested)
                        {
                            await KafKaUp(driverPropertys.VariableTopic, item.ToJsonString(), token);
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogMessage.LogWarning(ex, ToString());
                    }

                }
                if (isSuccess)
                    producer.Flush(token);

            }
        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex, ToString());
        }
        try
        {
            ////变化推送
            var devList = _collectDeviceRunTimes.ToListWithDequeue();
            if (devList?.Count != 0)
            {
                //分解List，避免超出mqtt字节大小限制
                var devData = devList.ChunkTrivialBetter(driverPropertys.SplitSize);
                foreach (var item in devData)
                {
                    try
                    {
                        if (!token.IsCancellationRequested)
                        {
                            await KafKaUp(driverPropertys.DeviceTopic, item.ToJsonString(), token);
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
                if (isSuccess)
                    producer.Flush(token);

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

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    public override bool IsConnected() => isSuccess;

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        _globalDeviceData?.CollectDevices.Where(a => _uploadVariables.Select(b => b.DeviceId).Contains(a.Id)).ForEach(a =>
        {
            a.DeviceStatusChange -= DeviceStatusChange;
        });
        _uploadVariables?.ForEach(a =>
        {
            a.VariableValueChange -= VariableValueChange;
        });
        producer?.Dispose();
        _uploadVariables = null;
        _collectDeviceRunTimes.Clear();
        _collectVariableRunTimes.Clear();
        base.Dispose(disposing);
    }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="device"></param>
    protected override void Init(UploadDeviceRunTime device)
    {
        #region Kafka 生产者
        //1、生产者配置
        producerconfig = new ProducerConfig
        {
            BootstrapServers = driverPropertys.BootStrapServers,
            ClientId = driverPropertys.ClientId,
        };
        //2、创建生产者
        producerBuilder = new ProducerBuilder<Null, string>(producerconfig);
        //3、错误日志监视
        producerBuilder.SetErrorHandler((p, msg) =>
        {
            isSuccess = false;
            LogMessage?.LogWarning(msg.Reason);
        });
        //kafka
        try
        {
            producer = producerBuilder.Build();
        }
        catch (DllNotFoundException)
        {
            if (!Library.IsLoaded)
            {
                string fileEx = ".dll";
                string osStr = "win-";
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    osStr = "win-";
                    fileEx = ".dll";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    osStr = "linux-";
                    fileEx = ".so";
                }
                else
                {

                    osStr = "osx-";
                    fileEx = ".dylib";
                }
                osStr += RuntimeInformation.ProcessArchitecture.ToString().ToLower();

                var pathToLibrd = System.IO.Path.Combine(AppContext.BaseDirectory, "Plugins", "ThingsGateway.Kafka", "runtimes", osStr, "native", $"librdkafka{fileEx}");
                Library.Load(pathToLibrd);
            }
            producer = producerBuilder.Build();
        }
        #endregion

        _globalDeviceData = App.GetService<GlobalDeviceData>();

        var tags = _globalDeviceData.AllVariables.Where(a => a.VariablePropertys.ContainsKey(device.Id))
           .Where(b => GetPropertyValue(b, nameof(variablePropertys.Enable)).GetBoolValue()).ToList();

        _uploadVariables = tags;

        _globalDeviceData.CollectDevices.Where(a => _uploadVariables.Select(b => b.DeviceId).Contains(a.Id)).ForEach(a =>
        {
            a.DeviceStatusChange += DeviceStatusChange;
        });
        _uploadVariables.ForEach(a =>
        {
            a.VariableValueChange += VariableValueChange;
        });


    }

    private void DeviceStatusChange(CollectDeviceRunTime collectDeviceRunTime)
    {
        _collectDeviceRunTimes.Enqueue(collectDeviceRunTime.Adapt<DeviceData>());
    }

    private async Task KafKaUp(string topic, string payLoad, CancellationToken token)
    {
        try
        {
            using CancellationTokenSource cancellationTokenSource = new();
            using CancellationTokenSource stoppingToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token, token);
            Task<DeliveryResult<Null, string>> resultTask = producer.ProduceAsync(topic, new Message<Null, string> { Value = payLoad }, stoppingToken.Token);
            var timeOutResult = await Task.WhenAny(resultTask, Task.Delay(driverPropertys.TimeOut, stoppingToken.Token));
            if (timeOutResult == resultTask)
            {
                var result = (timeOutResult as Task<DeliveryResult<Null, string>>).Result;
                if (result.Status != PersistenceStatus.Persisted)
                {
                    isSuccess = false;
                    await CacheDb.AddCacheData(topic, payLoad, driverPropertys.CacheMaxCount);
                }
                else
                {
                    isSuccess = true;
                    //连接成功时补发缓存数据
                    var cacheData = await CacheDb.GetCacheData();
                    foreach (var item in cacheData)
                    {
                        var cacheResult = await producer.ProduceAsync(item.Topic, new Message<Null, string> { Value = item.CacheStr }, stoppingToken.Token);

                        if (cacheResult.Status == PersistenceStatus.Persisted)
                        {
                            LogMessage.Trace(FoundationConst.LogMessageHeader + $"主题：{item.Topic}{Environment.NewLine}负载：{item.CacheStr}");

                            await CacheDb.DeleteCacheData(item.Id);
                        }
                    }
                    LogMessage.Trace(FoundationConst.LogMessageHeader + $"主题：{topic}{Environment.NewLine}负载：{payLoad}");

                }
            }
            else
            {
                isSuccess = false;
                stoppingToken.Cancel();
            }
        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex, ToString());
            await CacheDb.AddCacheData(topic, payLoad, driverPropertys.CacheMaxCount);
        }
    }
    private void VariableValueChange(DeviceVariableRunTime collectVariableRunTime)
    {
        _collectVariableRunTimes.Enqueue(collectVariableRunTime.Adapt<VariableData>());
    }
}