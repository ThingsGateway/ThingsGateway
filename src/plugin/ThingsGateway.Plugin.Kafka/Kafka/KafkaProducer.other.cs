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

using ThingsGateway.Cache;
using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Extension.Generic;

using TouchSocket.Core;

namespace ThingsGateway.Plugin.Kafka;

/// <summary>
/// Kafka消息生产
/// </summary>
public partial class KafkaProducer : BusinessBaseWithCacheIntervalScript<VariableData, DeviceData, AlarmVariable>
{
    private volatile bool producerSuccess = true;
    private IProducer<Null, string> _producer;
    private ProducerBuilder<Null, string> _producerBuilder;
    private ProducerConfig _producerconfig;

    protected override void VariableChange(VariableRunTime variableRunTime)
    {
        AddQueueT(new(variableRunTime.Adapt<VariableData>()));
        base.VariableChange(variableRunTime);
    }

    protected override void DeviceChange(DeviceRunTime deviceRunTime)
    {
        AddQueueT2(new(deviceRunTime.Adapt<DeviceData>()));
        base.DeviceChange(deviceRunTime);
    }

    protected override void AlarmChange(AlarmVariable alarmVariable)
    {
        AddQueueT3(new(alarmVariable.Adapt<AlarmVariable>()));
        base.AlarmChange(alarmVariable);
    }

    protected override Task<OperResult> UpdateT3(IEnumerable<LiteDBDefalutCacheItem<AlarmVariable>> item, CancellationToken cancellationToken)
    {
        return UpdateT3(item.Select(a => a.Data), cancellationToken);
    }

    protected override Task<OperResult> UpdateT2(IEnumerable<LiteDBDefalutCacheItem<DeviceData>> item, CancellationToken cancellationToken)
    {
        return UpdateT2(item.Select(a => a.Data), cancellationToken);
    }

    protected override Task<OperResult> UpdateT(IEnumerable<LiteDBDefalutCacheItem<VariableData>> item, CancellationToken cancellationToken)
    {
        return UpdateT(item.Select(a => a.Data), cancellationToken);
    }

    #region private

    private async Task<OperResult> UpdateT3(IEnumerable<AlarmVariable> item, CancellationToken cancellationToken)
    {
        List<TopicJson> topicJsonList = GetAlarms(item);
        return await Update(topicJsonList, cancellationToken);
    }

    private async Task<OperResult> Update(List<TopicJson> topicJsonList, CancellationToken cancellationToken)
    {
        foreach (var topicJson in topicJsonList)
        {
            var result = await KafKaUpAsync(topicJson.Topic, topicJson.Json, cancellationToken);
            if (success != result.IsSuccess)
            {
                if (!result.IsSuccess)
                {
                    LogMessage.LogWarning(result.ToString());
                }
                success = result.IsSuccess;
            }
            if (!result.IsSuccess)
            {
                return result;
            }
        }
        return new();
    }

    private async Task<OperResult> UpdateT2(IEnumerable<DeviceData> item, CancellationToken cancellationToken)
    {
        List<TopicJson> topicJsonList = GetDeviceData(item);
        return await Update(topicJsonList, cancellationToken);
    }

    private async Task<OperResult> UpdateT(IEnumerable<VariableData> item, CancellationToken cancellationToken)
    {
        List<TopicJson> topicJsonList = GetVariable(item);
        return await Update(topicJsonList, cancellationToken);
    }

    #endregion private

    #region 方法

    private async Task AllPublishAsync(CancellationToken cancellationToken)
    {
        //保留消息
        //分解List，避免超出字节大小限制
        var varData = CurrentDevice.VariableRunTimes.Adapt<List<VariableData>>().ChunkBetter(_driverPropertys.SplitSize);
        var devData = CollectDevices.Adapt<List<DeviceData>>().ChunkBetter(_driverPropertys.SplitSize);
        foreach (var item in varData)
        {
            if (!success)
                break;
            await UpdateT(item, cancellationToken);
        }

        foreach (var item in devData)
        {
            if (!success)
                break;
            await UpdateT2(item, cancellationToken);
        }
    }

    /// <summary>
    /// kafka上传，返回上传结果
    /// </summary>
    private async Task<OperResult> KafKaUpAsync(string topic, string payLoad, CancellationToken cancellationToken)
    {
        try
        {
            using CancellationTokenSource cancellationTokenSource = new();
            using CancellationTokenSource stoppingToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token, cancellationToken);
            Task<DeliveryResult<Null, string>> resultTask = _producer.ProduceAsync(topic, new Message<Null, string> { Value = payLoad }, stoppingToken.Token);
            var timeOutResult = await Task.WhenAny(resultTask, Task.Delay(_driverPropertys.TimeOut, stoppingToken.Token));
            if (timeOutResult == resultTask)
            {
                var result = (timeOutResult as Task<DeliveryResult<Null, string>>).Result;
                if (result.Status != PersistenceStatus.Persisted)
                {
                    return new("上传失败");
                }
                else
                {
                    LogMessage.Trace($"主题：{topic}{Environment.NewLine}负载：{payLoad}");
                    return new();
                }
            }
            else
            {
                stoppingToken.Cancel();
                return new("上传超时");
            }
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }
    }

    #endregion 方法
}