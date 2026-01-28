//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Collections.Frozen;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 业务插件
/// </summary>
public abstract class BusinessBaseWithCacheAlarm : BusinessBaseWithCache
{
    protected override bool PluginEventDataModelEnable => true;
    protected override bool AlarmModelEnable => true;

    protected override bool DevModelEnable => false;

    protected override bool VarModelEnable => false;

    protected override ValueTask<OperResult> UpdateDevModel(List<CacheDBItem<DeviceBasicData>> item, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
    protected override ValueTask<OperResult> UpdateVarModel(List<CacheDBItem<VariableBasicData>> item, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override ValueTask<OperResult> UpdateVarModels(List<VariableBasicData> item, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public override async Task AfterVariablesChangedAsync(CancellationToken cancellationToken)
    {
        await base.AfterVariablesChangedAsync(cancellationToken).ConfigureAwait(false);

        IdVariableRuntimes = GlobalData.AlarmEnableIdVariables;

        CollectDevices = IdVariableRuntimes.Select(a => a.Value.DeviceRuntime).Where(a => !a.IsMemory && a.IsCollect == true).DistinctBy(a => a.Id).ToFrozenDictionary(a => a.Id, a => a);
        VariableRuntimeGroups = IdVariableRuntimes.Where(a => !a.Value.BusinessGroup.IsNullOrEmpty()).GroupBy(a => a.Value.BusinessGroup ?? string.Empty).ToFrozenDictionary(a => a.Key, a => a.Select(a => a.Value).ToList());

    }
    protected internal override async Task InitChannelAsync(ChannelObject channelObject, CancellationToken cancellationToken)
    {
        GlobalData.AlarmChangedEvent -= AlarmValueChange;
        GlobalData.ReadOnlyRealAlarmIdVariables?.ForEach(a => AlarmValueChange(a.Value));
        GlobalData.AlarmChangedEvent += AlarmValueChange;
        GlobalData.PluginEventHandler -= PluginEventChange;
        GlobalData.PluginEventHandler += PluginEventChange;

        await base.InitChannelAsync(channelObject, cancellationToken).ConfigureAwait(false);
    }
    protected override Task DisposeAsync(bool disposing)
    {
        GlobalData.AlarmChangedEvent -= AlarmValueChange;
        GlobalData.PluginEventHandler -= PluginEventChange;
        return base.DisposeAsync(disposing);
    }


    private void PluginEventChange(PluginEventData value)
    {
        if (CurrentDevice?.Pause != false)
            return;
        if (TaskSchedulerLoop?.Stoped == true) return;

        if (!PluginEventDataModelEnable) return;
        // 如果业务属性的缓存为间隔上传，则不执行后续操作
        //if (_businessPropertyWithCacheInterval?.IsInterval != true)
        {
            PluginChange(value);
        }
    }
    /// <summary>
    /// 当报警状态变化时触发此方法。如果不需要进行报警上传，则可以忽略此方法。通常情况下，需要在此方法中执行 <see cref="BusinessBaseWithCache.AddQueuePluginDataModel(CacheDBItem{PluginEventData})"/> 方法。
    /// </summary>
    protected virtual void PluginChange(PluginEventData value)
    {
        // 在报警状态变化时执行的自定义逻辑
    }

    /// <summary>
    /// 当报警值发生变化时触发此事件处理方法。该方法内部会检查是否需要进行报警上传，如果需要，则调用 <see cref="AlarmChange(AlarmVariable)"/> 方法。
    /// </summary>
    /// <param name="alarmVariable">报警变量</param>
    protected void AlarmValueChange(AlarmVariable alarmVariable)
    {
        if (CurrentDevice?.Pause != false)
            return;
        if (TaskSchedulerLoop?.Stoped == true) return;

        if (!AlarmModelEnable) return;
        // 如果业务属性的缓存为间隔上传，则不执行后续操作
        //if (_businessPropertyWithCacheInterval?.IsInterval != true)
        {
            // 检查当前设备的变量是否包含此报警变量，如果包含，则触发报警变量的变化处理方法
            if (IdVariableRuntimes.ContainsKey(alarmVariable.Id))
                AlarmChange(alarmVariable);
        }
    }
    /// <summary>
    /// 当报警状态变化时触发此方法。如果不需要进行报警上传，则可以忽略此方法。通常情况下，需要在此方法中执行 <see cref="BusinessBaseWithCache.AddQueueAlarmModel"/> 方法。
    /// </summary>
    /// <param name="alarmVariable">报警变量</param>
    protected virtual void AlarmChange(AlarmVariable alarmVariable)
    {
        // 在报警状态变化时执行的自定义逻辑
    }

    public override void PauseThread(bool pause)
    {
        lock (pauseLock)
        {
            var oldV = CurrentDevice.Pause;
            base.PauseThread(pause);
            if (!pause && oldV != pause)
            {
                GlobalData.ReadOnlyRealAlarmIdVariables?.ForEach(a => AlarmChange(a.Value));
            }
        }
    }

}
