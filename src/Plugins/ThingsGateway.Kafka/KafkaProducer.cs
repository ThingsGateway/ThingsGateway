using Confluent.Kafka;

using Mapster;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NewLife.Serialization;

using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Extension;
using ThingsGateway.Web.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.Kafka;

using System.Collections.Concurrent;
using System.Runtime.InteropServices;

/// <summary>
/// kafka 生产者属性
/// </summary>
public class KafkaProducerProperty : DriverPropertyBase
{
    [DeviceProperty("服务地址", "")] public string BootStrapServers { get; set; } = "127.0.0.1";
    [DeviceProperty("设备主题", "")] public string DeviceTopic { get; set; } = "test1";
    [DeviceProperty("变量主题", "")] public string VariableTopic { get; set; } = "test2";
    [DeviceProperty("分组ID", "")] public string GroupId { get; set; } = "test-consumer-group";
    [DeviceProperty("客户端ID", "")] public string ClientId { get; set; } = "test-consumer";
    [DeviceProperty("线程循环间隔", "最小500ms")] public int CycleInterval { get; set; } = 1000;

}
/// <summary>
/// kafka 生产者可变属性
/// </summary>
public class KafkaProducerVariableProperty : VariablePropertyBase
{
    [VariableProperty("启用", "")]
    public bool Enable { get; set; } = true;
    [VariableProperty("允许写入", "")]
    public bool VariableRpcEnable { get; set; } = true;
}
public class KafkaProducer : UpLoadBase
{
    private UploadDevice _curDevice;
    private GlobalCollectDeviceData _globalCollectDeviceData;

    private ProducerConfig producerconfig;
    private IProducer<Null, string> producer;
    private ProducerBuilder<Null, string> producerBuilder;

    private RpcSingletonService _rpcCore;
    private List<CollectVariableRunTime> _uploadVariables = new();

    private CollectDeviceWorker collectDeviceHostService;

    private ConcurrentQueue<DeviceData> CollectDeviceRunTimes = new();
    private ConcurrentQueue<VariableData> CollectVariableRunTimes = new();
    private KafkaProducerProperty driverPropertys = new();

    private EasyLock lockobj = new();
    private KafkaProducerVariableProperty variablePropertys = new();

    public KafkaProducer(IServiceScopeFactory scopeFactory) : base(scopeFactory)
    {
    }


    public override DriverPropertyBase DriverPropertys => driverPropertys;


    public override List<CollectVariableRunTime> UploadVariables => _uploadVariables;


    public override VariablePropertyBase VariablePropertys => variablePropertys;
    public override async Task BeforStartAsync()
    {
        await Task.CompletedTask;
    }

    public override void Dispose()
    {

    }
    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="device"></param>
    protected override void Init(UploadDevice device)
    {
        _curDevice = device;
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
                string osStr = "win-";
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    osStr = "win-";
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    osStr = "linux-";
                else
                    osStr = "osx-";
                osStr += RuntimeInformation.ProcessArchitecture.ToString().ToLower();
                var pathToLibrd = System.IO.Path.Combine(AppContext.BaseDirectory, "Plugins", "ThingsGateway.Kafka", "runtimes", osStr, "native", "librdkafka");
                Library.Load(pathToLibrd);
            }
            producer = producerBuilder.Build();
        }
        #endregion

        using var serviceScope = _scopeFactory.CreateScope();
        _globalCollectDeviceData = serviceScope.ServiceProvider.GetService<GlobalCollectDeviceData>();
        _rpcCore = serviceScope.ServiceProvider.GetService<RpcSingletonService>();
        collectDeviceHostService = serviceScope.GetBackgroundService<CollectDeviceWorker>();

        var tags = _globalCollectDeviceData.CollectVariables.Where(a => a.VariablePropertys.ContainsKey(device.Id))
           .Where(b => b.VariablePropertys[device.Id].Any(c =>
           {
               if (c.PropertyName == nameof(variablePropertys.Enable)) { if (c.Value?.GetBoolValue() == true) return true; else return false; }
               else return false;
           })).ToList();

        _uploadVariables = tags;

        _globalCollectDeviceData.CollectDevices.Where(a => _uploadVariables.Select(b => b.DeviceId).Contains(a.Id)).ForEach(a =>
        {
            a.DeviceStatusCahnge += DeviceStatusCahnge;
        });
        _uploadVariables.ForEach(a =>
        {
            a.VariableValueChange += VariableValueChange;
        });


    }

    private void handler(DeliveryReport<Null, string> r)
    {
        if (!r.Error.IsError)
        {
            _logger.LogTrace($"Delivered message to {r.TopicPartitionOffset}");
        }
        else
        {
            _logger.LogWarning($"Delivery Error: {r.Error.Reason}");
        }
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
            var varList = CollectVariableRunTimes.ToListWithDequeue();
            if (varList?.Count != 0)
            {
                //分解List，避免超出mqtt字节大小限制
                var varData = varList.ChunkTrivialBetter(500);
                foreach (var item in varData)
                {
                    try
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            producer.Produce(driverPropertys.VariableTopic, new Message<Null, string> { Value = item.ToJson() }, handler);
                            producer.Flush(TimeSpan.FromSeconds(3));
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
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, ToString());
        }
        try
        {
            ////变化推送
            var devList = CollectDeviceRunTimes.ToListWithDequeue();
            if (devList?.Count != 0)
            {
                //分解List，避免超出mqtt字节大小限制
                var devData = devList.ChunkTrivialBetter(500);
                foreach (var item in devData)
                {
                    try
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            producer.Produce(driverPropertys.DeviceTopic, new Message<Null, string> { Value = item.ToJson() }, handler);
                            producer.Flush(TimeSpan.FromSeconds(3));
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

            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, ToString());
        }

        if (driverPropertys.CycleInterval > UploadDeviceThread.CycleInterval + 50)
        {
            await Task.Delay(driverPropertys.CycleInterval - UploadDeviceThread.CycleInterval);
        }
        else
        {

        }

    }

    public override OperResult IsConnected()
    {
        return producer == null ? new("初始化失败") : OperResult.CreateSuccessResult();
    }

    private void DeviceStatusCahnge(CollectDeviceRunTime collectDeviceRunTime)
    {
        CollectDeviceRunTimes.Enqueue(collectDeviceRunTime.Adapt<DeviceData>());
    }

    private void VariableValueChange(CollectVariableRunTime collectVariableRunTime)
    {
        CollectVariableRunTimes.Enqueue(collectVariableRunTime.Adapt<VariableData>());
    }
}