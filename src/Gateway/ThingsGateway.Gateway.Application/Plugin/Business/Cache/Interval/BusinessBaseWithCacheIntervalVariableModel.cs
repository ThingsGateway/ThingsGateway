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
/// 抽象类 <see cref="BusinessBaseWithCacheIntervalVariableModel{T}"/>，表示具有缓存间隔功能的业务基类，其中 T 代表变量模型。
/// </summary>
/// <typeparam name="T">变量模型类型</typeparam>
public abstract class BusinessBaseWithCacheIntervalVariableModel<T> : BusinessBaseWithCacheVariableModel<T>
{
    /// <summary>
    /// 用于定时触发的时间间隔。
    /// </summary>
    protected TimeTick _exTTimerTick;

    /// <summary>
    /// 获取具体业务属性的缓存设置。
    /// </summary>
    protected override BusinessPropertyWithCache _businessPropertyWithCache => _businessPropertyWithCacheInterval;

    /// <summary>
    /// 获取具体业务属性的缓存间隔设置。
    /// </summary>
    protected abstract BusinessPropertyWithCacheInterval _businessPropertyWithCacheInterval { get; }

    /// <summary>
    /// 初始化方法，用于初始化业务对象。
    /// </summary>
    /// <param name="channel">通道对象</param>
    internal protected override void Init(IChannel? channel = null)
    {
        // 如果业务属性指定了全部变量，则设置当前设备的变量运行时列表和采集设备列表
        if (_businessPropertyWithCacheInterval.IsAllVariable)
        {
            CurrentDevice.VariableRunTimes = GlobalData.Variables;
            CollectDevices = GlobalData.CollectDevices;
        }

        // 如果业务间隔小于等于100毫秒，则将业务间隔设置为100毫秒
        if (_businessPropertyWithCacheInterval.BusinessInterval <= 100)
            _businessPropertyWithCacheInterval.BusinessInterval = 100;

        // 初始化定时器
        _exTTimerTick = new TimeTick(_businessPropertyWithCacheInterval.BusinessInterval);

        // 注册变量值变化事件处理程序
        GlobalData.VariableValueChangeEvent -= VariableValueChange;
        if (!_businessPropertyWithCacheInterval.IsInterval)
        {
            GlobalData.VariableValueChangeEvent += VariableValueChange;
            // 触发一次变量值变化事件
            CurrentDevice.VariableRunTimes.ForEach(a =>
            {
                if (a.Value.IsOnline)
                    VariableValueChange(a.Value, a.Value.Adapt<VariableData>());
            });
        }
    }

    /// <summary>
    /// 释放资源的方法。
    /// </summary>
    /// <param name="disposing">是否释放托管资源</param>
    protected override void Dispose(bool disposing)
    {
        GlobalData.VariableValueChangeEvent -= VariableValueChange;
        _memoryVarModelQueue.Clear();
        base.Dispose(disposing);
    }

    /// <summary>
    /// 间隔插入操作，用于周期性地插入变量。
    /// </summary>
    /// <returns>表示异步操作的任务</returns>
    protected virtual async Task IntervalInsert()
    {
        var vardatas = CurrentDevice.VariableRunTimes.Values.ToList();
        while (!DisposedValue)
        {
            if (CurrentDevice?.KeepRun == false)
            {
                await Delay(default).ConfigureAwait(false);
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
            }


            await Delay(default).ConfigureAwait(false);
        }
    }

 

    /// <summary>
    /// 在启动前执行的异步操作。
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>表示异步操作的任务</returns>
    protected override Task ProtectedBeforStartAsync(CancellationToken cancellationToken)
    {
        // 启动间隔插入操作
        _ = IntervalInsert();
        return base.ProtectedBeforStartAsync(cancellationToken);
    }

    /// <summary>
    /// 当变量状态变化时发生，通常需要执行<see cref="BusinessBaseWithCacheVariableModel{T}.AddQueueVarModel(CacheDBItem{T})"/>。
    /// </summary>
    /// <param name="variableRunTime">变量运行时对象</param>
    /// <param name="variable">变量运行时对象</param>
    protected virtual void VariableChange(VariableRunTime variableRunTime, VariableData variable)
    {
    }

    /// <summary>
    /// 当变量值发生变化时调用的方法。
    /// </summary>
    /// <param name="variableRunTime">变量运行时对象</param>
    /// <param name="variable">变量数据</param>
    private void VariableValueChange(VariableRunTime variableRunTime, VariableData variable)
    {
        if (!CurrentDevice.KeepRun)
            return;
        if (_businessPropertyWithCacheInterval?.IsInterval != true)
        {
            //筛选
            if (CurrentDevice.VariableRunTimes.ContainsKey(variableRunTime.Name))
                VariableChange(variableRunTime, variable);
        }
    }
}
