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

using Mapster;

using RabbitMQ.Client;

using System.Text;

using ThingsGateway.Cache;
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
        var alramData = WorkerUtil.GetWoker<AlarmWorker>().RealAlarmVariables.Adapt<List<AlarmVariable>>().ChunkBetter(_driverPropertys.SplitSize);
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

        foreach (var item in alramData)
        {
            if (!success)
                break;
            await UpdateT3(item, cancellationToken);
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
                LogMessage.Trace($"主题：{topic}{Environment.NewLine}负载：{payLoad}");
                return new();
            }
            else
            {
                return new("上传失败");
            }
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }
    }

    #endregion 方法
}