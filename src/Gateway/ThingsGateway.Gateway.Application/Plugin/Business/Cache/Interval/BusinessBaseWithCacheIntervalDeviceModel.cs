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
/// 业务插件的抽象基类，用于实现设备和变量之间的间隔上传功能。
/// </summary>
/// <typeparam name="VarModel">变量数据类型</typeparam>
/// <typeparam name="DevModel">设备数据类型</typeparam>
public abstract class BusinessBaseWithCacheIntervalDeviceModel<VarModel, DevModel> : BusinessBaseWithCacheDeviceModel<VarModel, DevModel>
{
    // 用于控制设备上传的定时器
    protected TimeTick _exT2TimerTick;

    // 用于控制变量上传的定时器
    protected TimeTick _exTTimerTick;

    /// <summary>
    /// 获取具体业务属性的缓存设置。
    /// </summary>
    protected override BusinessPropertyWithCache _businessPropertyWithCache => _businessPropertyWithCacheInterval;

    /// <summary>
    /// 获取业务属性与缓存间隔的抽象属性。
    /// </summary>
    protected abstract BusinessPropertyWithCacheInterval _businessPropertyWithCacheInterval { get; }

    /// <summary>
    /// 初始化方法，初始化插件。
    /// </summary>
    /// <param name="channel">通道对象</param>
    internal protected override void Init(IChannel? channel = null)
    {
        // 如果业务属性要求上传所有变量，则更新当前设备的变量运行次数和采集设备信息
        if (_businessPropertyWithCacheInterval.IsAllVariable)
        {
            CurrentDevice.VariableRunTimes = GlobalData.Variables;
            CollectDevices = GlobalData.CollectDevices;
        }

        // 设置业务间隔时间的最小值为100毫秒
        if (_businessPropertyWithCacheInterval.BusinessInterval <= 100)
            _businessPropertyWithCacheInterval.BusinessInterval = 100;

        // 初始化设备和变量上传的定时器
        _exTTimerTick = new(_businessPropertyWithCacheInterval.BusinessInterval);
        _exT2TimerTick = new(_businessPropertyWithCacheInterval.BusinessInterval);

        // 注销全局变量值改变事件和设备状态改变事件的订阅，以防止重复订阅
        GlobalData.VariableValueChangeEvent -= VariableValueChange;
        GlobalData.DeviceStatusChangeEvent -= DeviceStatusChange;

        // 如果不是间隔上传，则订阅全局变量值改变事件和设备状态改变事件，并触发一次事件处理
        if (!_businessPropertyWithCacheInterval.IsInterval)
        {
            GlobalData.DeviceStatusChangeEvent += DeviceStatusChange;
            GlobalData.VariableValueChangeEvent += VariableValueChange;
            CollectDevices.ForEach(a =>
            {
                if (a.Value.DeviceStatus == DeviceStatusEnum.OnLine)
                    DeviceStatusChange(a.Value, a.Value.Adapt<DeviceBasicData>());
            });
            CurrentDevice.VariableRunTimes.ForEach(a =>
            {
                if (a.Value.IsOnline)
                    VariableValueChange(a.Value, a.Value.Adapt<VariableBasicData>());
            });
        }
    }

    /// <summary>
    /// 设备状态变化时发生的虚拟方法，用于处理设备状态变化事件。
    /// </summary>
    /// <param name="deviceRunTime">设备运行时对象</param>
    /// <param name="deviceData">设备数据对象</param>
    protected virtual void DeviceChange(DeviceRunTime deviceRunTime, DeviceBasicData deviceData)
    {
    }

    /// <summary>
    /// 释放资源的方法，释放插件相关资源。
    /// </summary>
    /// <param name="disposing">是否释放托管资源</param>
    protected override void Dispose(bool disposing)
    {
        // 注销全局变量值改变事件和设备状态改变事件的订阅
        GlobalData.VariableValueChangeEvent -= VariableValueChange;
        GlobalData.DeviceStatusChangeEvent -= DeviceStatusChange;

        // 清空内存队列
        _memoryDevModelQueue.Clear();
        _memoryVarModelQueue.Clear();
        base.Dispose(disposing);
    }

    /// <summary>
    /// 执行间隔插入任务的方法，用于定期上传设备和变量信息。
    /// </summary>
    /// <returns>异步任务</returns>
    protected virtual async Task IntervalInsert()
    {
        var vardatas = CurrentDevice.VariableRunTimes.Values.ToList();
        var devdatas = CollectDevices.Values.ToList();
        while (!DisposedValue)
        {
            if (CurrentDevice?.KeepRun == false)
            {
                await Delay(default).ConfigureAwait(false);
                continue;
            }

            // 如果是间隔上传，根据定时器触发事件上传设备和变量信息
            if (_businessPropertyWithCacheInterval.IsInterval)
            {
                try
                {
                    if (_exTTimerTick.IsTickHappen())
                    {
                        // 上传所有变量信息
                        foreach (var variableRuntime in vardatas)
                        {
                            VariableChange(variableRuntime, variableRuntime.Adapt<VariableData>());
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
                        // 上传所有设备信息
                        foreach (var deviceRuntime in devdatas)
                        {
                            DeviceChange(deviceRuntime, deviceRuntime.Adapt<DeviceBasicData>());
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogMessage.LogWarning(ex, BusinessBaseLocalizer["IntervalInsertDeviceFail"]);
                }
            }

            await Delay(default).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 在开始前的保护方法，异步执行间隔插入任务。
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>异步任务</returns>
    protected override Task ProtectedBeforStartAsync(CancellationToken cancellationToken)
    {
        _ = IntervalInsert();
        return base.ProtectedBeforStartAsync(cancellationToken);
    }

    /// <summary>
    /// 变量状态变化时发生的虚拟方法，用于处理变量状态变化事件。
    /// </summary>
    /// <param name="variableRunTime">变量运行时对象</param>
    /// <param name="variable">变量数据对象</param>
    protected virtual void VariableChange(VariableRunTime variableRunTime, VariableData variable)
    {
    }

    /// <summary>
    /// 设备状态改变时的事件处理方法。
    /// </summary>
    /// <param name="deviceRunTime">设备运行时对象</param>
    /// <param name="deviceData">设备数据对象</param>
    private void DeviceStatusChange(DeviceRunTime deviceRunTime, DeviceBasicData deviceData)
    {
        // 如果当前设备已停止运行，则直接返回，不进行处理
        if (!CurrentDevice.KeepRun)
            return;

        // 如果业务属性不是间隔上传，则执行设备状态改变的处理逻辑
        if (_businessPropertyWithCacheInterval?.IsInterval != true)
        {
            // 检查当前设备集合中是否包含该设备，并进行相应处理
            if (CollectDevices.ContainsKey(deviceRunTime.Name))
                DeviceChange(deviceRunTime, deviceData);
        }
    }

    /// <summary>
    /// 变量值改变时的事件处理方法。
    /// </summary>
    /// <param name="variableRunTime">变量运行时对象</param>
    /// <param name="variable">变量数据对象</param>
    private void VariableValueChange(VariableRunTime variableRunTime, VariableBasicData variable)
    {
        // 如果当前设备已停止运行，则直接返回，不进行处理
        if (!CurrentDevice.KeepRun)
            return;

        // 如果业务属性不是间隔上传，则执行变量状态改变的处理逻辑
        if (_businessPropertyWithCacheInterval?.IsInterval != true)
        {
            // 检查当前设备是否包含该变量，并进行相应处理
            if (CurrentDevice.VariableRunTimes.ContainsKey(variableRunTime.Name))
                VariableChange(variableRunTime, variable);
        }
    }
}
