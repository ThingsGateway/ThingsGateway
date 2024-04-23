//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Mapster;

using RabbitMQ.Client;

using System.Text;

using ThingsGateway.Admin.Application;
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

    protected override void VariableChange(VariableRunTime variableRunTime, VariableData variable)
    {
        AddQueueVarModel(new(variable));
        base.VariableChange(variableRunTime, variable);
    }

    protected override void DeviceChange(DeviceRunTime deviceRunTime, DeviceData deviceData)
    {
        AddQueueDevModel(new(deviceData));
        base.DeviceChange(deviceRunTime, deviceData);
    }

    protected override void AlarmChange(AlarmVariable alarmVariable)
    {
        AddQueueAlarmModel(new(alarmVariable));
        base.AlarmChange(alarmVariable);
    }

    protected override Task<OperResult> UpdateAlarmModel(IEnumerable<CacheDBItem<AlarmVariable>> item, CancellationToken cancellationToken)
    {
        return UpdateAlarmModel(item.Select(a => a.Value), cancellationToken);
    }

    protected override Task<OperResult> UpdateDevModel(IEnumerable<CacheDBItem<DeviceData>> item, CancellationToken cancellationToken)
    {
        return UpdateDevModel(item.Select(a => a.Value), cancellationToken);
    }

    protected override Task<OperResult> UpdateVarModel(IEnumerable<CacheDBItem<VariableData>> item, CancellationToken cancellationToken)
    {
        return UpdateVarModel(item.Select(a => a.Value), cancellationToken);
    }

    #region private

    private Task<OperResult> UpdateAlarmModel(IEnumerable<AlarmVariable> item, CancellationToken cancellationToken)
    {
        List<TopicJson> topicJsonList = GetAlarms(item);
        return Update(topicJsonList, cancellationToken);
    }

    private Task<OperResult> Update(List<TopicJson> topicJsonList, CancellationToken cancellationToken)
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
                return Task.FromResult(result);
            }
        }
        return Task.FromResult(new OperResult());
    }

    private Task<OperResult> UpdateDevModel(IEnumerable<DeviceData> item, CancellationToken cancellationToken)
    {
        List<TopicJson> topicJsonList = GetDeviceData(item);
        return Update(topicJsonList, cancellationToken);
    }

    private Task<OperResult> UpdateVarModel(IEnumerable<VariableData> item, CancellationToken cancellationToken)
    {
        List<TopicJson> topicJsonList = GetVariable(item);
        return Update(topicJsonList, cancellationToken);
    }

    #endregion private

    #region 方法

    private async Task AllPublishAsync(CancellationToken cancellationToken)
    {
        //保留消息
        //分解List，避免超出字节大小限制
        var varData = CurrentDevice.VariableRunTimes.Values.Adapt<List<VariableData>>().ChunkBetter(_driverPropertys.SplitSize);
        var devData = CollectDevices.Values.Adapt<List<DeviceData>>().ChunkBetter(_driverPropertys.SplitSize);
        var alramData = GlobalData.ReadOnlyRealAlarmVariables.Adapt<List<AlarmVariable>>().ChunkBetter(_driverPropertys.SplitSize);
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
                return new();
            }
            else
            {
                return new("Upload fail");
            }
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }
    }

    #endregion 方法
}
