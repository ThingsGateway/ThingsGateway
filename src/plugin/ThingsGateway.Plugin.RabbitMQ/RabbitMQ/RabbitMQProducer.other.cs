//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Mapster;

using RabbitMQ.Client;

using System.Text;

using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Extension.Generic;

using TouchSocket.Core;

namespace ThingsGateway.Plugin.RabbitMQ;

/// <summary>
/// RabbitMQProducer
/// </summary>
public partial class RabbitMQProducer : BusinessBaseWithCacheIntervalScript<VariableData, DeviceData, AlarmVariable>
{
    private IConnection _connection;
    private ConnectionFactory _connectionFactory;
    private IModel _model;

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

    private ValueTask<OperResult> Update(List<TopicJson> topicJsonList, CancellationToken cancellationToken)
    {
        foreach (var topicJson in topicJsonList)
        {
            var result = Publish(topicJson.Topic, topicJson.Json, _model.CreateBasicProperties());
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
                return ValueTask.FromResult(result);
            }
        }
        OperResult operResult = OperResult.Success;
        return ValueTask.FromResult(operResult);
    }

    private ValueTask<OperResult> UpdateAlarmModel(IEnumerable<AlarmVariable> item, CancellationToken cancellationToken)
    {
        List<TopicJson> topicJsonList = GetAlarms(item);
        return Update(topicJsonList, cancellationToken);
    }

    private ValueTask<OperResult> UpdateDevModel(IEnumerable<DeviceData> item, CancellationToken cancellationToken)
    {
        List<TopicJson> topicJsonList = GetDeviceData(item);
        return Update(topicJsonList, cancellationToken);
    }

    private ValueTask<OperResult> UpdateVarModel(IEnumerable<VariableData> item, CancellationToken cancellationToken)
    {
        List<TopicJson> topicJsonList = GetVariable(item);
        return Update(topicJsonList, cancellationToken);
    }

    #endregion private

    #region 方法

    private async ValueTask AllPublishAsync(CancellationToken cancellationToken)
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
    /// 上传，返回上传结果
    /// </summary>
    private OperResult Publish(string topic, string payLoad, IBasicProperties properties)
    {
        try
        {
            if (properties != null)
                properties.Persistent = true;
            if (_model != null)
            {
                _model.BasicPublish(_driverPropertys.ExchangeName, topic, properties, Encoding.UTF8.GetBytes(payLoad));
                LogMessage.Trace($"Topic：{topic}{Environment.NewLine}PayLoad：{payLoad}");
                return OperResult.Success;
            }
            else
            {
                return new OperResult("Upload fail");
            }
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }
    }

    #endregion 方法
}
