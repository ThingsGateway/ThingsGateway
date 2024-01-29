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

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 业务插件，额外实现变量、设备、变量间隔上传
/// </summary>
public abstract class BusinessBaseWithCacheInterval<T, T2, T3> : BusinessBaseWithCacheTTT<T, T2, T3>
{
    protected TimeTick _exT2TimerTick;
    protected TimeTick _exTTimerTick;

    /// <summary>
    /// <inheritdoc cref="DriverPropertys"/>
    /// </summary>
    protected override BusinessPropertyWithCache _businessPropertyWithCache => _businessPropertyWithCacheInterval;

    protected abstract BusinessPropertyWithCacheInterval _businessPropertyWithCacheInterval { get; }

    public override void Init(IChannel? channel = null)
    {
        base.Init(channel);

        if (_businessPropertyWithCacheInterval.IsAllVariable)
        {
            CurrentDevice.VariableRunTimes = GlobalData.AllVariables.ToList();
            CollectDevices = GlobalData.CollectDevices.ToList();
        }

        if (_businessPropertyWithCacheInterval.BusinessInterval <= 100) _businessPropertyWithCacheInterval.BusinessInterval = 100;
        _exTTimerTick = new(_businessPropertyWithCacheInterval.BusinessInterval);
        _exT2TimerTick = new(_businessPropertyWithCacheInterval.BusinessInterval);

        if (!_businessPropertyWithCacheInterval.IsInterval)
        {
            CurrentDevice?.VariableRunTimes?.ForEach(a => a.VariableValueChange -= VariableValueChange);
            var alarmWorker = WorkerUtil.GetWoker<AlarmWorker>();
            CollectDevices?.ForEach(a =>
            {
                a.DeviceStatusChange -= DeviceStatusChange;
            });
            alarmWorker.OnAlarmChanged -= AlarmValueChange;
            CollectDevices.ForEach(a => { a.DeviceStatusChange += DeviceStatusChange; DeviceStatusChange(a); });
            CurrentDevice.VariableRunTimes.ForEach(a => { a.VariableValueChange += VariableValueChange; VariableValueChange(a); });
            alarmWorker.OnAlarmChanged += AlarmValueChange;
            alarmWorker.RealAlarmVariables.Adapt<List<AlarmVariable>>().ToList().ForEach(a => AlarmValueChange(a));
        }
    }

    protected override Task ProtectedBeforStartAsync(CancellationToken cancellationToken)
    {
        _ = IntervalInsert(cancellationToken);
        return base.ProtectedBeforStartAsync(cancellationToken);
    }

    protected override void Dispose(bool disposing)
    {
        var alarmWorker = WorkerUtil.GetWoker<AlarmWorker>();
        alarmWorker.OnAlarmChanged -= AlarmValueChange;
        CurrentDevice?.VariableRunTimes?.ForEach(a => a.VariableValueChange -= VariableValueChange);

        CollectDevices?.ForEach(a =>
        {
            a.DeviceStatusChange -= DeviceStatusChange;
        });

        _memoryT2Queue.Clear();
        _memoryTQueue.Clear();
        base.Dispose(disposing);
    }

    protected virtual async Task IntervalInsert(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (CurrentDevice?.KeepRun == false)
            {
                await Delay(cancellationToken);
                continue;
            }
            //间隔上传
            if (_businessPropertyWithCacheInterval.IsInterval)
            {
                try
                {
                    if (_exTTimerTick.IsTickHappen())
                    {
                        //间隔推送全部变量
                        foreach (var variableData in CurrentDevice.VariableRunTimes)
                        {
                            VariableChange(variableData);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogMessage.LogWarning(ex, "添加队列失败");
                }
                try
                {
                    if (_exT2TimerTick.IsTickHappen())
                    {
                        foreach (var devData in CollectDevices)
                        {
                            DeviceChange(devData);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogMessage.LogWarning(ex, "添加队列失败");
                }
            }

            await Delay(cancellationToken);
        }
    }

    /// <summary>
    /// 报警状态变化时发生，如不需要报警上传，忽略此方法，通常需要执行<see cref="BusinessBaseWithCacheTTT{T,T2,T3}.AddQueueT3(T3)"/>
    /// </summary>
    /// <param name="deviceRunTime"></param>
    protected virtual void AlarmChange(AlarmVariable alarmVariable)
    {
    }

    /// <summary>
    /// 设备状态变化时发生，如不需要设备上传，忽略此方法，通常需要执行<see cref="BusinessBaseWithCacheTT{T,T2}.AddQueueT2(T2)"/>
    /// </summary>
    /// <param name="deviceRunTime"></param>
    protected virtual void DeviceChange(DeviceRunTime deviceRunTime)
    {
    }

    /// <summary>
    /// 变量状态变化时发生，如不需要变量上传，忽略此方法，通常需要执行<see cref="BusinessBaseWithCacheT{T}.AddQueueT(T)"/>
    /// </summary>
    /// <param name="variableRunTime"></param>
    protected virtual void VariableChange(VariableRunTime variableRunTime)
    {
    }

    private void AlarmValueChange(AlarmVariable alarmVariable)
    {
        if (!CurrentDevice.KeepRun)
            return;
        if (_businessPropertyWithCacheInterval?.IsInterval != true)
        {
            AlarmChange(alarmVariable);
        }
    }

    private void VariableValueChange(VariableRunTime variableRunTime)
    {
        if (!CurrentDevice.KeepRun)
            return;
        if (_businessPropertyWithCacheInterval?.IsInterval != true)
        {
            VariableChange(variableRunTime);
        }
    }

    private void DeviceStatusChange(DeviceRunTime deviceRunTime)
    {
        if (!CurrentDevice.KeepRun)
            return;
        if (_businessPropertyWithCacheInterval?.IsInterval != true)
        {
            DeviceChange(deviceRunTime);
        }
    }
}

/// <summary>
/// 业务插件，额外实现设备、变量间隔上传
/// </summary>
public abstract class BusinessBaseWithCacheInterval<T, T2> : BusinessBaseWithCacheTT<T, T2>
{
    protected TimeTick _exT2TimerTick;
    protected TimeTick _exTTimerTick;

    /// <summary>
    /// <inheritdoc cref="DriverPropertys"/>
    /// </summary>
    protected override BusinessPropertyWithCache _businessPropertyWithCache => _businessPropertyWithCacheInterval;

    protected abstract BusinessPropertyWithCacheInterval _businessPropertyWithCacheInterval { get; }

    public override void Init(IChannel? channel = null)
    {
        base.Init(channel);

        if (_businessPropertyWithCacheInterval.IsAllVariable)
        {
            CurrentDevice.VariableRunTimes = GlobalData.AllVariables.ToList();
            CollectDevices = GlobalData.CollectDevices.ToList();
        }

        if (_businessPropertyWithCacheInterval.BusinessInterval <= 100) _businessPropertyWithCacheInterval.BusinessInterval = 100;
        _exTTimerTick = new(_businessPropertyWithCacheInterval.BusinessInterval);
        _exT2TimerTick = new(_businessPropertyWithCacheInterval.BusinessInterval);

        if (!_businessPropertyWithCacheInterval.IsInterval)
        {
            CurrentDevice?.VariableRunTimes?.ForEach(a => a.VariableValueChange -= VariableValueChange);
            CollectDevices?.ForEach(a =>
            {
                a.DeviceStatusChange -= DeviceStatusChange;
            });
            CollectDevices.ForEach(a => { a.DeviceStatusChange += DeviceStatusChange; });
            CurrentDevice.VariableRunTimes.ForEach(a => { a.VariableValueChange += VariableValueChange; });
        }
    }

    protected override Task ProtectedBeforStartAsync(CancellationToken cancellationToken)
    {
        _ = IntervalInsert(cancellationToken);
        return base.ProtectedBeforStartAsync(cancellationToken);
    }

    protected override void Dispose(bool disposing)
    {
        CurrentDevice?.VariableRunTimes?.ForEach(a => a.VariableValueChange -= VariableValueChange);

        CollectDevices?.ForEach(a =>
        {
            a.DeviceStatusChange -= DeviceStatusChange;
        });

        _memoryT2Queue.Clear();
        _memoryTQueue.Clear();
        base.Dispose(disposing);
    }

    protected virtual async Task IntervalInsert(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (CurrentDevice?.KeepRun == false)
            {
                await Delay(cancellationToken);
                continue;
            }
            //间隔上传
            if (_businessPropertyWithCacheInterval.IsInterval)
            {
                try
                {
                    if (_exTTimerTick.IsTickHappen())
                    {
                        //间隔推送全部变量
                        foreach (var variableData in CurrentDevice.VariableRunTimes)
                        {
                            VariableChange(variableData);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogMessage.LogWarning(ex, "添加队列失败");
                }
                try
                {
                    if (_exT2TimerTick.IsTickHappen())
                    {
                        foreach (var devData in CollectDevices)
                        {
                            DeviceChange(devData);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogMessage.LogWarning(ex, "添加队列失败");
                }
            }

            await Delay(cancellationToken);
        }
    }

    /// <summary>
    /// 设备状态变化时发生，如不需要设备上传，忽略此方法，通常需要执行<see cref="BusinessBaseWithCacheTT{T,T2}.AddQueueT2(T2)"/>
    /// </summary>
    /// <param name="deviceRunTime"></param>
    protected virtual void DeviceChange(DeviceRunTime deviceRunTime)
    {
    }

    /// <summary>
    /// 变量状态变化时发生，如不需要变量上传，忽略此方法，通常需要执行<see cref="BusinessBaseWithCacheT{T}.AddQueueT(T)"/>
    /// </summary>
    /// <param name="variableRunTime"></param>
    protected virtual void VariableChange(VariableRunTime variableRunTime)
    {
    }

    private void VariableValueChange(VariableRunTime variableRunTime)
    {
        if (!CurrentDevice.KeepRun)
            return;
        if (_businessPropertyWithCacheInterval?.IsInterval != true)
        {
            VariableChange(variableRunTime);
        }
    }

    private void DeviceStatusChange(DeviceRunTime deviceRunTime)
    {
        if (!CurrentDevice.KeepRun)
            return;
        if (_businessPropertyWithCacheInterval?.IsInterval != true)
        {
            DeviceChange(deviceRunTime);
        }
    }
}

/// <summary>
/// 业务插件，额外实现变量间隔上传
/// </summary>
public abstract class BusinessBaseWithCacheInterval<T> : BusinessBaseWithCacheT<T>
{
    protected TimeTick _exTTimerTick;

    /// <summary>
    /// <inheritdoc cref="DriverPropertys"/>
    /// </summary>
    protected override BusinessPropertyWithCache _businessPropertyWithCache => _businessPropertyWithCacheInterval;

    protected abstract BusinessPropertyWithCacheInterval _businessPropertyWithCacheInterval { get; }

    public override void Init(IChannel? channel = null)
    {
        base.Init(channel);

        if (_businessPropertyWithCacheInterval.IsAllVariable)
        {
            CurrentDevice.VariableRunTimes = GlobalData.AllVariables.ToList();
            CollectDevices = GlobalData.CollectDevices.ToList();
        }

        if (_businessPropertyWithCacheInterval.BusinessInterval <= 100) _businessPropertyWithCacheInterval.BusinessInterval = 100;
        _exTTimerTick = new(_businessPropertyWithCacheInterval.BusinessInterval);

        if (!_businessPropertyWithCacheInterval.IsInterval)
        {
            CurrentDevice?.VariableRunTimes?.ForEach(a => a.VariableValueChange -= VariableValueChange);
            CurrentDevice.VariableRunTimes.ForEach(a => { a.VariableValueChange += VariableValueChange; });
        }
    }

    protected override Task ProtectedBeforStartAsync(CancellationToken cancellationToken)
    {
        _ = IntervalInsert(cancellationToken);
        return base.ProtectedBeforStartAsync(cancellationToken);
    }

    protected override void Dispose(bool disposing)
    {
        CurrentDevice?.VariableRunTimes?.ForEach(a => a.VariableValueChange -= VariableValueChange);

        _memoryTQueue.Clear();
        base.Dispose(disposing);
    }

    protected virtual async Task IntervalInsert(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (CurrentDevice?.KeepRun == false)
            {
                await Delay(cancellationToken);
                continue;
            }
            //间隔上传
            IntervalInsertVariable();

            await Delay(cancellationToken);
        }
    }

    protected virtual void IntervalInsertVariable()
    {
        if (_businessPropertyWithCacheInterval.IsInterval)
        {
            try
            {
                if (_exTTimerTick.IsTickHappen())
                {
                    //间隔推送全部变量
                    foreach (var variableData in CurrentDevice.VariableRunTimes)
                    {
                        VariableChange(variableData);
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage.LogWarning(ex, "添加队列失败");
            }
        }
    }

    /// <summary>
    /// 变量状态变化时发生，如不需要变量上传，忽略此方法，通常需要执行<see cref="BusinessBaseWithCacheT{T}.AddQueueT(T)"/>
    /// </summary>
    /// <param name="variableRunTime"></param>
    protected virtual void VariableChange(VariableRunTime variableRunTime)
    {
    }

    private void VariableValueChange(VariableRunTime variableRunTime)
    {
        if (!CurrentDevice.KeepRun)
            return;
        if (_businessPropertyWithCacheInterval?.IsInterval != true)
        {
            VariableChange(variableRunTime);
        }
    }
}