#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

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
    public override async Task BeforStartAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
    protected override void Dispose(bool disposing)
    {
        producer.Dispose();
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

        var serviceScope = _scopeFactory.CreateScope();
        _globalCollectDeviceData = serviceScope.ServiceProvider.GetService<GlobalCollectDeviceData>();
        _rpcCore = serviceScope.ServiceProvider.GetService<RpcSingletonService>();
        collectDeviceHostService = serviceScope.GetBackgroundService<CollectDeviceWorker>();

        var tags = _globalCollectDeviceData.CollectVariables.Where(a => a.VariablePropertys.ContainsKey(device.Id))
           .Where(b => GetPropertyValue(b, nameof(variablePropertys.Enable)).GetBoolValue()).ToList();

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