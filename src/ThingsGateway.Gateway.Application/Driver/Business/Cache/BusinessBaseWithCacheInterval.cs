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
/// 业务插件，额外实现变量、设备、变量间隔上传
/// </summary>
public abstract class BusinessBaseWithCacheInterval : BusinessBaseWithCache
{
    /// <summary>
    /// 获取具体业务属性的缓存设置。
    /// </summary>
    protected sealed override BusinessPropertyWithCache _businessPropertyWithCache => _businessPropertyWithCacheInterval;

    /// <summary>
    /// 获取具体业务属性的缓存间隔设置。
    /// </summary>
    protected abstract BusinessPropertyWithCacheInterval _businessPropertyWithCacheInterval { get; }



    protected internal override async Task InitChannelAsync(ChannelObject channelObject, CancellationToken cancellationToken)
    {
        if (AlarmModelEnable)
        {
            GlobalData.AlarmChangedEvent -= AlarmValueChange;
            GlobalData.ReadOnlyRealAlarmIdVariables?.ForEach(a => AlarmValueChange(a.Value));

            GlobalData.AlarmChangedEvent += AlarmValueChange;

        }
        if (PluginEventDataModelEnable)
        {
            GlobalData.PluginEventHandler -= PluginEventChange;
            GlobalData.PluginEventHandler += PluginEventChange;

        }
        if (DevModelEnable)
        {
            // 如果不是间隔上传，则订阅全局变量值改变事件和设备状态改变事件，并触发一次事件处理
            if (_businessPropertyWithCacheInterval.BusinessUpdateEnum != BusinessUpdateEnum.Interval)
            {
                GlobalData.DeviceStatusChangeEvent += DeviceStatusChange;
            }
        }

        if (VarModelEnable)
        {
            // 注册变量值变化事件处理程序
            if (_businessPropertyWithCacheInterval.BusinessUpdateEnum != BusinessUpdateEnum.Interval)
            {
                GlobalData.VariableValueChangeEvent += VariableValueChange;
            }
        }

        await base.InitChannelAsync(channelObject, cancellationToken).ConfigureAwait(false);
    }
    public override async Task AfterVariablesChangedAsync(CancellationToken cancellationToken)
    {
        if (AlarmModelEnable || DevModelEnable || VarModelEnable || PluginEventDataModelEnable)
        {
            // 如果业务属性指定了全部变量，则设置当前设备的变量运行时列表和采集设备列表
            if (_businessPropertyWithCacheInterval.IsAllVariable)
            {
                LogMessage?.LogInformation("Refresh variable");
                IdVariableRuntimes = GlobalData.GetEnableVariables().ToFrozenDictionary(a => a.Id);
                CollectDevices = IdVariableRuntimes.Select(a => a.Value.DeviceRuntime).Where(a => !a.IsMemory && a.IsCollect == true).DistinctBy(a => a.Id).ToFrozenDictionary(a => a.Id, a => a);


                VariableRuntimeGroups = IdVariableRuntimes.GroupBy(a => a.Value.BusinessGroup ?? string.Empty).ToFrozenDictionary(a => a.Key, a => a.Select(a => a.Value).ToList());
            }
            else
            {
                await base.AfterVariablesChangedAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        if (DevModelEnable)
        {
            CollectDevices?.ForEach(a =>
            {
                if (_businessPropertyWithCacheInterval.BusinessUpdateEnum != BusinessUpdateEnum.Interval)
                    DeviceStatusChange(a.Value, a.Value.AdaptDeviceBasicData());
            });
        }

        if (VarModelEnable)
        {
            // 触发一次变量值变化事件
            IdVariableRuntimes.ForEach(a =>
            {
                if (((!_businessPropertyWithCacheInterval.OnlineFilter) || a.Value.IsOnline) && _businessPropertyWithCacheInterval.BusinessUpdateEnum != BusinessUpdateEnum.Interval)
                    VariableValueInit(a.Value);
            });
        }
    }

    /// <summary>
    /// 当报警状态变化时触发此方法。如果不需要进行报警上传，则可以忽略此方法。通常情况下，需要在此方法中执行 <see cref="BusinessBaseWithCache.AddQueueAlarmModel(CacheDBItem{AlarmVariable})"/> 方法。
    /// </summary>
    /// <param name="alarmVariable">报警变量</param>
    protected virtual void AlarmChange(AlarmVariable alarmVariable)
    {
        // 在报警状态变化时执行的自定义逻辑
    }

    /// <summary>
    /// 当设备状态变化时触发此方法。如果不需要进行设备上传，则可以忽略此方法。通常情况下，需要在此方法中执行 <see cref="BusinessBaseWithCache.AddQueueDevModel(CacheDBItem{DeviceBasicData})"/> 方法。
    /// </summary>
    /// <param name="deviceRuntime">设备运行时信息</param>
    /// <param name="deviceData">设备数据</param>
    protected virtual void DeviceChange(DeviceRuntime deviceRuntime, DeviceBasicData deviceData)
    {
        // 在设备状态变化时执行的自定义逻辑
    }
    /// <summary>
    /// 当设备状态定时变化时触发此方法。如果不需要进行设备上传，则可以忽略此方法。通常情况下，需要在此方法中执行 <see cref="BusinessBaseWithCache.AddQueueDevModels(CacheDBItem{List{DeviceBasicData}})"/> 方法。
    /// </summary>
    /// <param name="deviceRuntime">设备运行时信息</param>
    /// <param name="deviceData">设备数据</param>
    protected virtual void DeviceTimeInterval(IEnumerable<DeviceRuntime> deviceRuntime, List<DeviceBasicData> deviceData)
    {
        // 在设备状态变化时执行的自定义逻辑
    }

    /// <summary>
    /// 释放资源方法
    /// </summary>
    protected override Task DisposeAsync(bool disposing)
    {
        // 解绑事件
        GlobalData.AlarmChangedEvent -= AlarmValueChange;
        GlobalData.PluginEventHandler -= PluginEventChange;

        GlobalData.VariableValueChangeEvent -= VariableValueChange;
        GlobalData.DeviceStatusChangeEvent -= DeviceStatusChange;


        return base.DisposeAsync(disposing);
    }

    /// <summary>
    /// 间隔上传数据的方法
    /// </summary>
    protected void IntervalInsert(object? state, CancellationToken cancellationToken)
    {
        if (CurrentDevice?.Pause != false)
        {
            return;
        }

        // 如果业务属性的缓存为间隔上传，则根据定时器间隔执行相应操作
        if (_businessPropertyWithCacheInterval.BusinessUpdateEnum != BusinessUpdateEnum.Change)
        {
            if (VarModelEnable)
            {
                try
                {
                    if (LogMessage?.LogLevel <= TouchSocket.Core.LogLevel.Trace)
                        LogMessage?.LogTrace($"Interval {typeof(VariableBasicData).Name} data, count {IdVariableRuntimes.Count}");
                    // 间隔推送全部变量
                    var variableRuntimes = IdVariableRuntimes.Select(a => a.Value);
                    VariableTimeInterval(variableRuntimes, variableRuntimes.AdaptListVariableBasicData());
                }
                catch (Exception ex)
                {
                    LogMessage?.LogWarning(ex, AppResource.IntervalInsertVariableFail);
                }
            }
            if (DevModelEnable)
            {
                try
                {
                    if (CollectDevices != null)
                    {
                        if (LogMessage?.LogLevel <= TouchSocket.Core.LogLevel.Trace)
                            LogMessage?.LogTrace($"Interval {typeof(DeviceBasicData).Name} data, count {CollectDevices.Count}");

                        var deviceRuntimes = CollectDevices.Select(a => a.Value);
                        // 间隔推送全部设备
                        DeviceTimeInterval(deviceRuntimes, deviceRuntimes.AdaptListDeviceBasicData());
                    }
                }
                catch (Exception ex)
                {
                    LogMessage?.LogWarning(ex, AppResource.IntervalInsertDeviceFail);
                }
            }
        }
    }

    protected override List<IScheduledTask> ProtectedGetTasks(CancellationToken cancellationToken)
    {
        var list = base.ProtectedGetTasks(cancellationToken);
        list.Add(ScheduledTaskHelper.GetTask(_businessPropertyWithCacheInterval.BusinessInterval, IntervalInsert, null, LogMessage, cancellationToken));
        return list;
    }

    /// <summary>
    /// 当变量状态变化时触发此方法。如果不需要进行变量上传，则可以忽略此方法。通常情况下，需要在此方法中执行 <see cref="BusinessBaseWithCache.AddQueueVarModel(CacheDBItem{VariableBasicData})"/> 方法。
    /// </summary>
    /// <param name="variableRuntime">变量运行时信息</param>
    /// <param name="variable">变量数据</param>
    protected virtual void VariableChange(VariableRuntime variableRuntime, VariableBasicData variable)
    {
        // 在变量状态变化时执行的自定义逻辑
    }

    /// <summary>
    /// 当变量定时变化时触发此方法。如果不需要进行变量上传，则可以忽略此方法。通常情况下，需要在此方法中执行 <see cref="BusinessBaseWithCache.AddQueueVarModels(CacheDBItem{List{VariableBasicData}})"/> 方法。
    /// </summary>
    /// <param name="variableRuntimes">变量运行时信息</param>
    /// <param name="variables">变量数据</param>
    protected virtual void VariableTimeInterval(IEnumerable<VariableRuntime> variableRuntimes, List<VariableBasicData> variables)
    {
        // 在变量状态变化时执行的自定义逻辑
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

    public override void PauseThread(bool pause)
    {
        lock (pauseLock)
        {
            var oldV = CurrentDevice.Pause;
            base.PauseThread(pause);
            if (!pause && oldV != pause)
            {
                if (AlarmModelEnable)
                {
                    GlobalData.ReadOnlyRealAlarmIdVariables?.ForEach(a => AlarmChange(a.Value));
                }
                if (DevModelEnable)
                {
                    CollectDevices?.ForEach(a =>
                {
                    if (_businessPropertyWithCacheInterval.BusinessUpdateEnum != BusinessUpdateEnum.Interval)
                        DeviceStatusChange(a.Value, a.Value.AdaptDeviceBasicData());
                });
                }
                if (VarModelEnable)
                {
                    IdVariableRuntimes.ForEach(a =>
                {
                    if (((!_businessPropertyWithCacheInterval.OnlineFilter) || a.Value.IsOnline) && _businessPropertyWithCacheInterval.BusinessUpdateEnum != BusinessUpdateEnum.Interval)
                        VariableValueInit(a.Value);
                });
                }
            }
        }
    }

    /// <summary>
    /// 当设备状态发生变化时触发此事件处理方法。该方法内部会检查是否需要进行设备上传，如果需要，则调用 <see cref="DeviceChange(DeviceRuntime, DeviceBasicData)"/> 方法。
    /// </summary>
    /// <param name="deviceRuntime">设备运行时信息</param>
    /// <param name="deviceData">设备数据</param>
    protected void DeviceStatusChange(DeviceRuntime deviceRuntime, DeviceBasicData deviceData)
    {
        if (CurrentDevice?.Pause != false)
            return;
        if (TaskSchedulerLoop?.Stoped == true) return;
        if (!DevModelEnable) return;
        // 如果业务属性的缓存为间隔上传，则不执行后续操作
        //if (_businessPropertyWithCacheInterval?.IsInterval != true)
        {
            // 检查当前设备的设备列表是否包含此设备，如果包含，则触发设备的状态变化处理方法
            if (CollectDevices?.ContainsKey(deviceData.Id) == true)
                DeviceChange(deviceRuntime, deviceData);
        }
    }

    /// <summary>
    /// 当变量值发生变化时触发此事件处理方法。该方法内部会检查是否需要进行变量上传，如果需要，则调用 <see cref="VariableChange(VariableRuntime, VariableBasicData)"/> 方法。
    /// </summary>
    /// <param name="variableRuntime">变量运行时信息</param>
    /// <param name="variable">变量数据</param>
    protected void VariableValueChange(VariableRuntime variableRuntime, VariableBasicData variable)
    {
        if (CurrentDevice?.Pause != false)
            return;
        if (!VarModelEnable) return;
        if (TaskSchedulerLoop?.Stoped == true) return;

        // 如果业务属性的缓存为间隔上传，则不执行后续操作
        //if (_businessPropertyWithCacheInterval?.IsInterval != true)
        {
            // 检查当前设备的变量是否包含此变量，如果包含，则触发变量的变化处理方法
            if (IdVariableRuntimes.ContainsKey(variable.Id))
                VariableChange(variableRuntime, variable);
        }
    }

    /// <summary>
    /// 当初始化时触发此事件处理方法。该方法内部会检查是否需要进行变量上传，如果需要，则调用 <see cref="VariableChange(VariableRuntime, VariableBasicData)"/> 方法。
    /// </summary>
    /// <param name="variableRuntime">变量运行时信息</param>
    protected void VariableValueInit(VariableRuntime variableRuntime)
    {
        if (CurrentDevice?.Pause != false)
            return;
        if (!VarModelEnable) return;
        if (TaskSchedulerLoop?.Stoped == true) return;

        // 如果业务属性的缓存为间隔上传，则不执行后续操作
        //if (_businessPropertyWithCacheInterval?.IsInterval != true)
        {
            // 检查当前设备的变量是否包含此变量，如果包含，则触发变量的变化处理方法
            if (IdVariableRuntimes.ContainsKey(variableRuntime.Id))
            {
                var data = variableRuntime.AdaptVariableBasicData();
                data.ValueInited = false;
                VariableChange(variableRuntime, data);
            }
        }
    }
}
