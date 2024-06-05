//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
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
    protected override BusinessPropertyWithCache _businessPropertyWithCache => _businessPropertyWithCacheInterval;

    /// <summary>
    /// 业务属性
    /// </summary>
    protected abstract BusinessPropertyWithCacheInterval _businessPropertyWithCacheInterval { get; }

    /// <summary>
    /// 初始化方法，可传入通道参数
    /// </summary>
    public override void Init(IChannel? channel = null)
    {
        // 如果业务属性的缓存要求上传所有变量，则设置当前设备的变量运行时间和采集设备列表
        if (_businessPropertyWithCacheInterval.IsAllVariable)
        {
            CurrentDevice.VariableRunTimes = GlobalData.Variables;
            CollectDevices = GlobalData.CollectDevices;
        }

        // 如果业务属性的缓存要求的业务间隔小于等于100，则设置为100
        if (_businessPropertyWithCacheInterval.BusinessInterval <= 100)
            _businessPropertyWithCacheInterval.BusinessInterval = 100;

        // 初始化
        _exTTimerTick = new(_businessPropertyWithCacheInterval.BusinessInterval);
        _exT2TimerTick = new(_businessPropertyWithCacheInterval.BusinessInterval);

        HostedServiceUtil.AlarmHostedService.OnAlarmChanged -= AlarmValueChange;
        HostedServiceUtil.AlarmHostedService.OnAlarmChanged += AlarmValueChange;

        // 根据业务属性的缓存是否为间隔上传来决定事件绑定
        if (!_businessPropertyWithCacheInterval.IsInterval)
        {
            // 解绑全局数据的事件
            GlobalData.VariableValueChangeEvent -= VariableValueChange;
            GlobalData.DeviceStatusChangeEvent -= DeviceStatusChange;

            // 绑定全局数据的事件
            GlobalData.DeviceStatusChangeEvent += DeviceStatusChange;
            GlobalData.VariableValueChangeEvent += VariableValueChange;

            // 触发一次设备状态变化和变量值变化事件
            CollectDevices.ForEach(a => { DeviceStatusChange(a.Value, a.Value.Adapt<DeviceData>()); });
            CurrentDevice.VariableRunTimes.ForEach(a => { VariableValueChange(a.Value, a.Value.Adapt<VariableData>()); });
        }
    }

    /// <summary>
    /// 启动前异步方法
    /// </summary>
    protected override Task ProtectedBeforStartAsync(CancellationToken cancellationToken)
    {
        // 启动间隔上传的数据获取线程
        _ = IntervalInsert();
        return base.ProtectedBeforStartAsync(cancellationToken);
    }

    /// <summary>
    /// 释放资源方法
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        // 解绑事件
        HostedServiceUtil.AlarmHostedService.OnAlarmChanged -= AlarmValueChange;
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
    protected virtual async Task IntervalInsert(CancellationToken cancellationToken = default)
    {
        while (!DisposedValue)
        {
            if (CurrentDevice?.KeepRun == false)
            {
                await Delay(cancellationToken).ConfigureAwait(false);
                continue;
            }

            // 如果业务属性的缓存为间隔上传，则根据定时器间隔执行相应操作
            if (_businessPropertyWithCacheInterval.IsInterval)
            {
                try
                {
                    if (_exTTimerTick.IsTickHappen())
                    {
                        // 间隔推送全部变量
                        foreach (var variableRuntime in CurrentDevice.VariableRunTimes)
                        {
                            VariableChange(variableRuntime.Value, variableRuntime.Value.Adapt<VariableData>());
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
                        foreach (var deviceRuntime in CollectDevices)
                        {
                            DeviceChange(deviceRuntime.Value, deviceRuntime.Value.Adapt<DeviceData>());
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogMessage.LogWarning(ex, BusinessBaseLocalizer["IntervalInsertDeviceFail"]);
                }
            }

            await Delay(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 当报警状态变化时触发此方法。如果不需要进行报警上传，则可以忽略此方法。通常情况下，需要在此方法中执行 <see cref="BusinessBaseWithCacheAlarmModel{T,T2,T3}.AddQueueT3(T3)"/> 方法。
    /// </summary>
    /// <param name="alarmVariable">报警变量</param>
    protected virtual void AlarmChange(AlarmVariable alarmVariable)
    {
        // 在报警状态变化时执行的自定义逻辑
    }

    /// <summary>
    /// 当设备状态变化时触发此方法。如果不需要进行设备上传，则可以忽略此方法。通常情况下，需要在此方法中执行 <see cref="BusinessBaseWithCacheDevModel{T,T2}.AddQueueDevModel(T2)"/> 方法。
    /// </summary>
    /// <param name="deviceRunTime">设备运行时信息</param>
    /// <param name="deviceData">设备数据</param>
    protected virtual void DeviceChange(DeviceRunTime deviceRunTime, DeviceData deviceData)
    {
        // 在设备状态变化时执行的自定义逻辑
    }

    /// <summary>
    /// 当变量状态变化时触发此方法。如果不需要进行变量上传，则可以忽略此方法。通常情况下，需要在此方法中执行 <see cref="BusinessBaseWithCacheVarModel{T}.AddQueueVarModel(T)"/> 方法。
    /// </summary>
    /// <param name="variableRunTime">变量运行时信息</param>
    /// <param name="variable">变量数据</param>
    protected virtual void VariableChange(VariableRunTime variableRunTime, VariableData variable)
    {
        // 在变量状态变化时执行的自定义逻辑
    }

    /// <summary>
    /// 当报警值发生变化时触发此事件处理方法。该方法内部会检查是否需要进行报警上传，如果需要，则调用 <see cref="AlarmChange(AlarmVariable)"/> 方法。
    /// </summary>
    /// <param name="alarmVariable">报警变量</param>
    private void AlarmValueChange(AlarmVariable alarmVariable)
    {
        if (!CurrentDevice.KeepRun)
            return;
        // 如果业务属性的缓存为间隔上传，则不执行后续操作
        if (_businessPropertyWithCacheInterval?.IsInterval != true)
        {
            // 检查当前设备的变量是否包含此报警变量，如果包含，则触发报警变量的变化处理方法
            if (CurrentDevice.VariableRunTimes.ContainsKey(alarmVariable.Name))
                AlarmChange(alarmVariable);
        }
    }

    /// <summary>
    /// 当变量值发生变化时触发此事件处理方法。该方法内部会检查是否需要进行变量上传，如果需要，则调用 <see cref="VariableChange(VariableRunTime, VariableData)"/> 方法。
    /// </summary>
    /// <param name="variableRunTime">变量运行时信息</param>
    /// <param name="variable">变量数据</param>
    private void VariableValueChange(VariableRunTime variableRunTime, VariableData variable)
    {
        if (!CurrentDevice.KeepRun)
            return;
        // 如果业务属性的缓存为间隔上传，则不执行后续操作
        if (_businessPropertyWithCacheInterval?.IsInterval != true)
        {
            // 检查当前设备的变量是否包含此变量，如果包含，则触发变量的变化处理方法
            if (CurrentDevice.VariableRunTimes.ContainsKey(variable.Name))
                VariableChange(variableRunTime, variable);
        }
    }

    /// <summary>
    /// 当设备状态发生变化时触发此事件处理方法。该方法内部会检查是否需要进行设备上传，如果需要，则调用 <see cref="DeviceChange(DeviceRunTime, DeviceData)"/> 方法。
    /// </summary>
    /// <param name="deviceRunTime">设备运行时信息</param>
    /// <param name="deviceData">设备数据</param>
    private void DeviceStatusChange(DeviceRunTime deviceRunTime, DeviceData deviceData)
    {
        if (!CurrentDevice.KeepRun)
            return;
        // 如果业务属性的缓存为间隔上传，则不执行后续操作
        if (_businessPropertyWithCacheInterval?.IsInterval != true)
        {
            // 检查当前设备的设备列表是否包含此设备，如果包含，则触发设备的状态变化处理方法
            if (CollectDevices.ContainsKey(deviceData.Name))
                DeviceChange(deviceRunTime, deviceData);
        }
    }
}
