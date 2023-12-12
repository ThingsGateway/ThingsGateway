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

using Yitter.IdGenerator;

namespace ThingsGateway.Plugin.Kafka;

/// <summary>
/// MqttClient
/// </summary>
public partial class KafkaProducer : UploadBaseWithCacheT<DeviceData, VariableData>
{
    private const string devType = "dev";
    private const string varType = "var";
    private readonly KafkaProducerProperty _driverPropertys = new();
    private readonly KafkaProducerVariableProperty _variablePropertys = new();
    private readonly EasyLock easyLock = new();
    private IProducer<Null, string> producer;
    private ProducerBuilder<Null, string> producerBuilder;
    private ProducerConfig producerconfig;
    private volatile bool success = true;

    protected override void AddCache(List<CacheItem> cacheItems, IEnumerable<VariableData> dev)
    {
        AddVarCahce(cacheItems, $"{_driverPropertys.VariableTopic}", dev.GetSciptListValue(_driverPropertys.BigTextScriptVariableModel));
    }

    private void AddVarCahce(List<CacheItem> cacheItems, string key, string data)
    {
        var cacheItem = new CacheItem()
        {
            Id = YitIdHelper.NextId(),
            Key = key,
            Type = varType,
            Value = data
        };
        cacheItems.Add(cacheItem);
    }

    protected override void AddCache(List<CacheItem> cacheItems, IEnumerable<DeviceData> dev)
    {
        AddDevCache(cacheItems, $"{_driverPropertys.DeviceTopic}", dev.GetSciptListValue(_driverPropertys.BigTextScriptDeviceModel));
    }

    private void AddDevCache(List<CacheItem> cacheItems, string key, string data)
    {
        var cacheItem = new CacheItem()
        {
            Id = YitIdHelper.NextId(),
            Key = key,
            Type = devType,
            Value = data
        };
        cacheItems.Add(cacheItem);
    }

    /// <summary>
    /// kafka上传，返回上传结果
    /// </summary>
    private async Task<OperResult> KafKaUp(string topic, string payLoad, CancellationToken cancellationToken)
    {
        try
        {
            using CancellationTokenSource cancellationTokenSource = new();
            using CancellationTokenSource stoppingToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token, cancellationToken);
            Task<DeliveryResult<Null, string>> resultTask = producer.ProduceAsync(topic, new Message<Null, string> { Value = payLoad }, stoppingToken.Token);
            var timeOutResult = await Task.WhenAny(resultTask, Task.Delay(_driverPropertys.TimeOut, stoppingToken.Token));
            if (timeOutResult == resultTask)
            {
                var result = (timeOutResult as Task<DeliveryResult<Null, string>>).Result;
                if (result.Status != PersistenceStatus.Persisted)
                {
                    CurrentDevice.SetDeviceStatus(DateTimeExtensions.CurrentDateTime, CurrentDevice.ErrorCount + 1);
                    return new("上传失败");
                }
                else
                {
                    LogMessage.Trace($"{FoundationConst.LogMessageHeader}主题：{topic}{Environment.NewLine}负载：{payLoad}");
                    CurrentDevice.SetDeviceStatus(DateTimeExtensions.CurrentDateTime, 0);
                    return OperResult.CreateSuccessResult();
                }
            }
            else
            {
                stoppingToken.Cancel();
                CurrentDevice.SetDeviceStatus(DateTimeExtensions.CurrentDateTime, CurrentDevice.ErrorCount + 1);
                return new("上传超时");
            }
        }
        catch (Exception ex)
        {
            CurrentDevice.SetDeviceStatus(DateTimeExtensions.CurrentDateTime, CurrentDevice.ErrorCount + 1);
            return new OperResult(ex);
        }
    }
}