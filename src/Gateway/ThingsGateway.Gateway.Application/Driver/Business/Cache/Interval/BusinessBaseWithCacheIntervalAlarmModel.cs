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

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 业务插件，额外实现变量、设备、变量间隔上传
/// </summary>
public abstract class BusinessBaseWithCacheIntervalAlarmModel<VarModel, DevModel, AlarmModel> : BusinessBaseWithCacheAlarmModel<VarModel, DevModel, AlarmModel>
{
    protected TimeTick _exT2TimerTick;  // 用于设备上传的时间间隔定时器
    protected TimeTick _exTTimerTick;   // 用于变量上传的时间间隔定时器

    /// <summary>
    /// 业务属性
    /// </summary>
    protected sealed override BusinessPropertyWithCache _businessPropertyWithCache => _businessPropertyWithCacheInterval;

    /// <summary>
    /// 业务属性
    /// </summary>
    protected abstract BusinessPropertyWithCacheInterval _businessPropertyWithCacheInterval { get; }

    protected internal override void InitChannel(IChannel? channel = null)
    {
        // 初始化
        _exTTimerTick = new(_businessPropertyWithCacheInterval.BusinessInterval);
        _exT2TimerTick = new(_businessPropertyWithCacheInterval.BusinessInterval);

        GlobalData.AlarmChangedEvent -= AlarmValueChange;
        GlobalData.AlarmChangedEvent += AlarmValueChange;
        // 解绑全局数据的事件
        GlobalData.VariableValueChangeEvent -= VariableValueChange;
        GlobalData.DeviceStatusChangeEvent -= DeviceStatusChange;

        // 根据业务属性的缓存是否为间隔上传来决定事件绑定
        if (_businessPropertyWithCacheInterval.BusinessUpdateEnum != BusinessUpdateEnum.Interval)
        {
            // 绑定全局数据的事件
            GlobalData.DeviceStatusChangeEvent += DeviceStatusChange;
            GlobalData.VariableValueChangeEvent += VariableValueChange;

        }

        base.InitChannel(channel);
    }
    public override void AfterVariablesChanged()
    {
        // 如果业务属性指定了全部变量，则设置当前设备的变量运行时列表和采集设备列表
        if (_businessPropertyWithCacheInterval.IsAllVariable)
        {
            VariableRuntimes = new(GlobalData.GetEnableVariables());

            CollectDevices = GlobalData.GetEnableDevices().ToDictionary();
        }
        else
        {
            base.AfterVariablesChanged();
        }


        // 触发一次设备状态变化和变量值变化事件
        CollectDevices.ForEach(a =>
        {
            if (a.Value.DeviceStatus == DeviceStatusEnum.OnLine)
                DeviceStatusChange(a.Value, a.Value.Adapt<DeviceBasicData>());
        });
        VariableRuntimes.ForEach(a =>
        {
            if (a.Value.IsOnline)
                VariableValueChange(a.Value, a.Value.Adapt<VariableBasicData>());
        });
    }

    /// <summary>
    /// 当报警状态变化时触发此方法。如果不需要进行报警上传，则可以忽略此方法。通常情况下，需要在此方法中执行 <see cref="BusinessBaseWithCacheAlarmModel{T,T2,T3}.AddQueueAlarmModel(CacheDBItem{T3})"/> 方法。
    /// </summary>
    /// <param name="alarmVariable">报警变量</param>
    protected virtual void AlarmChange(AlarmVariable alarmVariable)
    {
        // 在报警状态变化时执行的自定义逻辑
    }

    /// <summary>
    /// 当设备状态变化时触发此方法。如果不需要进行设备上传，则可以忽略此方法。通常情况下，需要在此方法中执行 <see cref="BusinessBaseWithCacheDeviceModel{T,T2}.AddQueueDevModel(CacheDBItem{T2})"/> 方法。
    /// </summary>
    /// <param name="deviceRuntime">设备运行时信息</param>
    /// <param name="deviceData">设备数据</param>
    protected virtual void DeviceChange(DeviceRuntime deviceRuntime, DeviceBasicData deviceData)
    {
        // 在设备状态变化时执行的自定义逻辑
    }
    /// <summary>
    /// 当设备状态定时变化时触发此方法。如果不需要进行设备上传，则可以忽略此方法。通常情况下，需要在此方法中执行 <see cref="BusinessBaseWithCacheDeviceModel{T,T2}.AddQueueDevModel(CacheDBItem{T2})"/> 方法。
    /// </summary>
    /// <param name="deviceRuntime">设备运行时信息</param>
    /// <param name="deviceData">设备数据</param>
    protected virtual void DeviceTimeInterval(DeviceRuntime deviceRuntime, DeviceBasicData deviceData)
    {
        // 在设备状态变化时执行的自定义逻辑
    }
    /// <summary>
    /// 释放资源方法
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        // 解绑事件
        GlobalData.AlarmChangedEvent -= AlarmValueChange;
        GlobalData.VariableValueChangeEvent -= VariableValueChange;
        GlobalData.DeviceStatusChangeEvent -= DeviceStatusChange;

        // 清空内存队列
        _memoryAlarmModelQueue.Clear();
        _memoryDevModelQueue.Clear();
        _memoryVarModelQueue.Clear();
        base.Dispose(disposing);
    }

    /// <summary>
    /// 间隔上传数据的方法
    /// </summary>
    protected virtual async Task IntervalInsert(CancellationToken cancellationToken)
    {
        while (!DisposedValue)
        {
            if (CurrentDevice.Pause == true)
            {
                await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
                continue;
            }

            // 如果业务属性的缓存为间隔上传，则根据定时器间隔执行相应操作
            if (_businessPropertyWithCacheInterval.BusinessUpdateEnum != BusinessUpdateEnum.Change)
            {
                try
                {
                    if (_exTTimerTick.IsTickHappen())
                    {
                        // 间隔推送全部变量
                        foreach (var variableRuntime in VariableRuntimes.Select(a => a.Value))
                        {
                            VariableTimeInterval(variableRuntime, variableRuntime.Adapt<VariableBasicData>());
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogMessage.LogWarning(ex, BusinessBaseLocalizer["IntervalInsertVariableFail"]);
                }
                try
                {
                    if (_exT2TimerTick.IsTickHappen())
                    {
                        // 间隔推送全部设备
                        foreach (var deviceRuntime in CollectDevices.Select(a => a.Value))
                        {
                            DeviceTimeInterval(deviceRuntime, deviceRuntime.Adapt<DeviceBasicData>());
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogMessage.LogWarning(ex, BusinessBaseLocalizer["IntervalInsertDeviceFail"]);
                }
            }

            await Task.Delay(100, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 启动前异步方法
    /// </summary>
    protected override Task ProtectedStartAsync(CancellationToken cancellationToken)
    {
        // 启动间隔上传的数据获取线程
        _ = IntervalInsert(cancellationToken);
        return base.ProtectedStartAsync(cancellationToken);
    }

    /// <summary>
    /// 当变量状态变化时触发此方法。如果不需要进行变量上传，则可以忽略此方法。通常情况下，需要在此方法中执行 <see cref="BusinessBaseWithCacheVariableModel{T}.AddQueueVarModel(CacheDBItem{T})"/> 方法。
    /// </summary>
    /// <param name="variableRuntime">变量运行时信息</param>
    /// <param name="variable">变量数据</param>
    protected virtual void VariableChange(VariableRuntime variableRuntime, VariableBasicData variable)
    {
        // 在变量状态变化时执行的自定义逻辑
    }
    /// <summary>
    /// 当变量定时变化时触发此方法。如果不需要进行变量上传，则可以忽略此方法。通常情况下，需要在此方法中执行 <see cref="BusinessBaseWithCacheVariableModel{T}.AddQueueVarModel(CacheDBItem{T})"/> 方法。
    /// </summary>
    /// <param name="variableRuntime">变量运行时信息</param>
    /// <param name="variable">变量数据</param>
    protected virtual void VariableTimeInterval(VariableRuntime variableRuntime, VariableBasicData variable)
    {
        // 在变量状态变化时执行的自定义逻辑
    }
    /// <summary>
    /// 当报警值发生变化时触发此事件处理方法。该方法内部会检查是否需要进行报警上传，如果需要，则调用 <see cref="AlarmChange(AlarmVariable)"/> 方法。
    /// </summary>
    /// <param name="alarmVariable">报警变量</param>
    private void AlarmValueChange(AlarmVariable alarmVariable)
    {
        if (CurrentDevice.Pause)
            return;
        // 如果业务属性的缓存为间隔上传，则不执行后续操作
        //if (_businessPropertyWithCacheInterval?.IsInterval != true)
        {
            // 检查当前设备的变量是否包含此报警变量，如果包含，则触发报警变量的变化处理方法
            if (VariableRuntimes.ContainsKey(alarmVariable.Name))
                AlarmChange(alarmVariable);
        }
    }

    /// <summary>
    /// 当设备状态发生变化时触发此事件处理方法。该方法内部会检查是否需要进行设备上传，如果需要，则调用 <see cref="DeviceChange(DeviceRuntime, DeviceBasicData)"/> 方法。
    /// </summary>
    /// <param name="deviceRuntime">设备运行时信息</param>
    /// <param name="deviceData">设备数据</param>
    private void DeviceStatusChange(DeviceRuntime deviceRuntime, DeviceBasicData deviceData)
    {
        if (CurrentDevice.Pause == true)
            return;
        // 如果业务属性的缓存为间隔上传，则不执行后续操作
        //if (_businessPropertyWithCacheInterval?.IsInterval != true)
        {
            // 检查当前设备的设备列表是否包含此设备，如果包含，则触发设备的状态变化处理方法
            if (CollectDevices.ContainsKey(deviceData.Id))
                DeviceChange(deviceRuntime, deviceData);
        }
    }

    /// <summary>
    /// 当变量值发生变化时触发此事件处理方法。该方法内部会检查是否需要进行变量上传，如果需要，则调用 <see cref="VariableChange(VariableRuntime, VariableBasicData)"/> 方法。
    /// </summary>
    /// <param name="variableRuntime">变量运行时信息</param>
    /// <param name="variable">变量数据</param>
    private void VariableValueChange(VariableRuntime variableRuntime, VariableBasicData variable)
    {
        if (CurrentDevice.Pause == true)
            return;
        // 如果业务属性的缓存为间隔上传，则不执行后续操作
        //if (_businessPropertyWithCacheInterval?.IsInterval != true)
        {
            // 检查当前设备的变量是否包含此变量，如果包含，则触发变量的变化处理方法
            if (VariableRuntimes.ContainsKey(variable.Name))
                VariableChange(variableRuntime, variable);
        }
    }
}
