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

using Mapster;

using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Extension.Generic;

using TouchSocket.Core;

namespace ThingsGateway.Plugin.Kafka;

/// <summary>
/// Kafka消息生产
/// </summary>
public partial class KafkaProducer : BusinessBaseWithCacheIntervalScript<VariableData, DeviceData, AlarmVariable>
{
    private IProducer<Null, string> _producer;
    private ProducerBuilder<Null, string> _producerBuilder;
    private ProducerConfig _producerconfig;
    private volatile bool producerSuccess = true;

    protected override void AlarmChange(AlarmVariable alarmVariable)
    {
        if (!_businessPropertyWithCacheIntervalScript.AlarmTopic.IsNullOrWhiteSpace())
            AddQueueAlarmModel(new(alarmVariable));
        base.AlarmChange(alarmVariable);
    }

    protected override void DeviceChange(DeviceRunTime deviceRunTime, DeviceBasicData deviceData)
    {
        if (!_businessPropertyWithCacheIntervalScript.DeviceTopic.IsNullOrWhiteSpace())
            AddQueueDevModel(new(deviceData));
        base.DeviceChange(deviceRunTime, deviceData);
    }

    protected override ValueTask<OperResult> UpdateAlarmModel(IEnumerable<CacheDBItem<AlarmVariable>> item, CancellationToken cancellationToken)
    {
        return UpdateAlarmModel(item.Select(a => a.Value), cancellationToken);
    }

    protected override ValueTask<OperResult> UpdateDevModel(IEnumerable<CacheDBItem<DeviceData>> item, CancellationToken cancellationToken)
    {
        return UpdateDevModel(item.Select(a => a.Value), cancellationToken);
    }

    protected override ValueTask<OperResult> UpdateVarModel(IEnumerable<CacheDBItem<VariableData>> item, CancellationToken cancellationToken)
    {
        return UpdateVarModel(item.Select(a => a.Value), cancellationToken);
    }

    protected override void VariableChange(VariableRunTime variableRunTime, VariableBasicData variable)
    {
        if (!_businessPropertyWithCacheIntervalScript.VariableTopic.IsNullOrWhiteSpace())
            AddQueueVarModel(new(variable));
        base.VariableChange(variableRunTime, variable);
    }

    #region private

    private async ValueTask<OperResult> Update(List<TopicJson> topicJsonList, CancellationToken cancellationToken)
    {
        foreach (var topicJson in topicJsonList)
        {
            var result = await KafKaUpAsync(topicJson.Topic, topicJson.Json, cancellationToken).ConfigureAwait(false);
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
        return OperResult.Success;
    }

    private async ValueTask<OperResult> UpdateAlarmModel(IEnumerable<AlarmVariable> item, CancellationToken cancellationToken)
    {
        List<TopicJson> topicJsonList = GetAlarms(item);
        return await Update(topicJsonList, cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask<OperResult> UpdateDevModel(IEnumerable<DeviceData> item, CancellationToken cancellationToken)
    {
        List<TopicJson> topicJsonList = GetDeviceData(item);
        return await Update(topicJsonList, cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask<OperResult> UpdateVarModel(IEnumerable<VariableData> item, CancellationToken cancellationToken)
    {
        List<TopicJson> topicJsonList = GetVariable(item);
        return await Update(topicJsonList, cancellationToken).ConfigureAwait(false);
    }

    #endregion private

    #region 方法

    private async Task AllPublishAsync(CancellationToken cancellationToken)
    {
        //保留消息
        //分解List，避免超出字节大小限制
        var varData = CurrentDevice.VariableRunTimes.Select(a => a.Value).Adapt<List<VariableData>>().ChunkBetter(_driverPropertys.SplitSize);
        var devData = CollectDevices.Select(a => a.Value).Adapt<List<DeviceData>>().ChunkBetter(_driverPropertys.SplitSize);
        var alramData = GlobalData.ReadOnlyRealAlarmVariables.Select(a => a.Value).Adapt<List<AlarmVariable>>().ChunkBetter(_driverPropertys.SplitSize);
        foreach (var item in varData)
        {
            if (!success)
                break;
            await UpdateVarModel(item, cancellationToken).ConfigureAwait(false);
        }

        foreach (var item in devData)
        {
            if (!success)
                break;
            await UpdateDevModel(item, cancellationToken).ConfigureAwait(false);
        }
        foreach (var item in alramData)
        {
            if (!success)
                break;
            await UpdateAlarmModel(item, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// kafka上传，返回上传结果
    /// </summary>
    private async ValueTask<OperResult> KafKaUpAsync(string topic, string payLoad, CancellationToken cancellationToken)
    {
        try
        {
            using CancellationTokenSource cancellationTokenSource = new();
            using CancellationTokenSource stoppingToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token, cancellationToken);
            Task<DeliveryResult<Null, string>> resultTask = _producer.ProduceAsync(topic, new Message<Null, string> { Value = payLoad }, stoppingToken.Token);
            var timeOutResult = await Task.WhenAny(resultTask, Task.Delay(_driverPropertys.Timeout, stoppingToken.Token)).ConfigureAwait(false);
            if (timeOutResult == resultTask)
            {
                var result = (timeOutResult as Task<DeliveryResult<Null, string>>).Result;
                if (result.Status != PersistenceStatus.Persisted)
                {
                    return new OperResult("Upload fail");
                }
                else
                {
                    LogMessage.Trace($"Topic：{topic}{Environment.NewLine}PayLoad：{payLoad}");
                    return OperResult.Success;
                }
            }
            else
            {
                stoppingToken.Cancel();
                return new OperResult("Upload timeout");
            }
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }
    }

    #endregion 方法
}
