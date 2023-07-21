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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;



using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Extension;
using ThingsGateway.Web.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.Kafka;

using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;

using ThingsGateway.Foundation.Extension.Enumerator;
using ThingsGateway.Foundation.Extension.Generic;

public class KafkaProducer : UpLoadBase
{
    private GlobalDeviceData _globalDeviceData;
    public override Type DriverDebugUIType => null;

    private RpcSingletonService _rpcCore;
    private List<DeviceVariableRunTime> _uploadVariables = new();
    private CollectDeviceWorker collectDeviceHostService;
    private ConcurrentQueue<DeviceData> _collectDeviceRunTimes = new();
    private ConcurrentQueue<VariableData> _collectVariableRunTimes = new();
    private KafkaProducerProperty driverPropertys = new();
    private EasyLock lockobj = new();
    private IProducer<Null, string> producer;
    private ProducerBuilder<Null, string> producerBuilder;
    private ProducerConfig producerconfig;
    private KafkaProducerVariableProperty variablePropertys = new();

    public KafkaProducer(IServiceScopeFactory scopeFactory) : base(scopeFactory)
    {
    }


    public override DriverPropertyBase DriverPropertys => driverPropertys;


    public override List<DeviceVariableRunTime> UploadVariables => _uploadVariables;


    public override VariablePropertyBase VariablePropertys => variablePropertys;
    public override async Task BeforStartAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
    /// <summary>
    /// 异步执行
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task ExecuteAsync(CancellationToken cancellationToken)
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
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            await KafKaUp(driverPropertys.VariableTopic, item.ToJson(), cancellationToken);
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, ToString());
                    }

                }
                producer.Flush(cancellationToken);

            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, ToString());
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
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            await KafKaUp(driverPropertys.DeviceTopic, item.ToJson(), cancellationToken);
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, ToString());
                    }
                }
                producer.Flush(cancellationToken);

            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, ToString());
        }

        if (driverPropertys.CycleInterval > UploadDeviceThread.CycleInterval + 50)
        {
            try
            {
                await Task.Delay(driverPropertys.CycleInterval - UploadDeviceThread.CycleInterval, cancellationToken);
            }
            catch
            {
            }
        }
        else
        {

        }

    }

    private async Task KafKaUp(string topic, string payLoad, CancellationToken cancellationToken)
    {
        try
        {
            using CancellationTokenSource cancellationTokenSource = new();
            using CancellationTokenSource stoppingToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token, cancellationToken);
            Task<DeliveryResult<Null, string>> resultTask = producer.ProduceAsync(topic, new Message<Null, string> { Value = payLoad }, stoppingToken.Token);
            var timeOutResult = await Task.WhenAny(resultTask, Task.Delay(driverPropertys.TimeOut, stoppingToken.Token));
            if (timeOutResult == resultTask)
            {
                var result = (timeOutResult as Task<DeliveryResult<Null, string>>).Result;
                if (result.Status != PersistenceStatus.Persisted)
                {
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
                            logMessage.Trace(LogMessageHeader + $"主题：{item.Topic}{Environment.NewLine}负载：{item.CacheStr}");

                            await CacheDb.DeleteCacheData(item.Id);
                        }
                    }
                    logMessage.Trace(LogMessageHeader + $"主题：{topic}{Environment.NewLine}负载：{payLoad}");

                }
            }
            else
            {
                stoppingToken.Cancel();
                isSuccess = false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, ToString());
            await CacheDb.AddCacheData(topic, payLoad, driverPropertys.CacheMaxCount);
            CurDevice.LastErrorMessage = ex.Message;
        }


    }
    private bool isSuccess = true;
    public override OperResult IsConnected()
    {
        return isSuccess ? new() : OperResult.CreateSuccessResult();
    }

    protected override void Dispose(bool disposing)
    {
        _globalDeviceData?.CollectDevices.Where(a => _uploadVariables.Select(b => b.DeviceId).Contains(a.Id)).ForEach(a =>
        {
            a.DeviceStatusCahnge -= DeviceStatusCahnge;
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
            CurDevice.LastErrorMessage = msg.Reason;
            _logger.LogWarning($"Producer_Erro信息：Code：{msg.Code}；Reason：{msg.Reason}；IsError：{msg.IsError}");
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
                _logger.LogInformation($"路径：{pathToLibrd}");
                Library.Load(pathToLibrd);
            }
            producer = producerBuilder.Build();
        }
        #endregion

        var serviceScope = _scopeFactory.CreateScope();
        _globalDeviceData = serviceScope.ServiceProvider.GetService<GlobalDeviceData>();
        _rpcCore = serviceScope.ServiceProvider.GetService<RpcSingletonService>();
        collectDeviceHostService = serviceScope.GetBackgroundService<CollectDeviceWorker>();

        var tags = _globalDeviceData.AllVariables.Where(a => a.VariablePropertys.ContainsKey(device.Id))
           .Where(b => GetPropertyValue(b, nameof(variablePropertys.Enable)).GetBoolValue()).ToList();

        _uploadVariables = tags;

        _globalDeviceData.CollectDevices.Where(a => _uploadVariables.Select(b => b.DeviceId).Contains(a.Id)).ForEach(a =>
        {
            a.DeviceStatusCahnge += DeviceStatusCahnge;
        });
        _uploadVariables.ForEach(a =>
        {
            a.VariableValueChange += VariableValueChange;
        });


    }

    private void DeviceStatusCahnge(CollectDeviceRunTime collectDeviceRunTime)
    {
        _collectDeviceRunTimes.Enqueue(collectDeviceRunTime.Adapt<DeviceData>());
    }


    private void VariableValueChange(DeviceVariableRunTime collectVariableRunTime)
    {
        _collectVariableRunTimes.Enqueue(collectVariableRunTime.Adapt<VariableData>());
    }
}