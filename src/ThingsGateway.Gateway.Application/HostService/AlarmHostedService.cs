
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

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

using System.Collections.Concurrent;

using ThingsGateway.Core.Extension;
using ThingsGateway.Gateway.Application.Extensions;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 变量报警事件委托
/// </summary>
public delegate void VariableAlarmEventHandler(AlarmVariable alarmVariable);

/// <summary>
/// 设备采集报警后台服务
/// </summary>
public class AlarmHostedService : BackgroundService
{
    private readonly ILogger _logger;

    /// <summary>
    /// 重启锁
    /// </summary>
    private EasyLock RestartLock { get; } = new();

    private ConcurrentQueue<VariableRunTime> _deviceVariables = new();

    private DoTask RealAlarmTask { get; set; }
    private IStringLocalizer Localizer { get; }

    /// <inheritdoc cref="AlarmHostedService"/>
    public AlarmHostedService(ILogger<AlarmHostedService> logger, IStringLocalizer<AlarmHostedService> localizer)
    {
        _logger = logger;
        Localizer = localizer;
    }

    /// <summary>
    /// 报警变化事件
    /// </summary>
    public event VariableAlarmEventHandler OnAlarmChanged;

    /// <summary>
    /// 实时报警列表
    /// </summary>
    internal ConcurrentList<VariableRunTime> RealAlarmVariables { get; } = new();

    #region 核心实现

    /// <summary>
    /// 获取bool报警类型
    /// </summary>
    /// <param name="tag">要检查的变量</param>
    /// <param name="limit">报警限制值</param>
    /// <param name="expressions">报警约束表达式</param>
    /// <param name="text">报警文本</param>
    /// <returns>报警类型枚举</returns>
    private static AlarmTypeEnum? GetBoolAlarmCode(VariableRunTime tag, out string limit, out string expressions, out string text)
    {
        limit = string.Empty; // 初始化报警限制值为空字符串
        expressions = string.Empty; // 初始化报警约束表达式为空字符串
        text = string.Empty; // 初始化报警文本为空字符串

        if (tag?.Value == null) // 检查变量是否为null或其值为null
        {
            return null; // 如果是，则返回null
        }

        if (tag.BoolCloseAlarmEnable && !tag.Value.ToBoolean(true)) // 检查是否启用了关闭报警功能，并且变量的布尔值为false
        {
            limit = false.ToString(); // 将报警限制值设置为"false"
            expressions = tag.BoolCloseRestrainExpressions!; // 获取关闭报警的约束表达式
            text = tag.BoolCloseAlarmText!; // 获取关闭报警时的报警文本
            return AlarmTypeEnum.Close; // 返回关闭报警类型枚举
        }

        if (tag.BoolOpenAlarmEnable && tag.Value.ToBoolean(false)) // 检查是否启用了开启报警功能，并且变量的布尔值为true
        {
            limit = true.ToString(); // 将报警限制值设置为"true"
            expressions = tag.BoolOpenRestrainExpressions!; // 获取开启报警的约束表达式
            text = tag.BoolOpenAlarmText!; // 获取开启报警时的报警文本
            return AlarmTypeEnum.Open; // 返回开启报警类型枚举
        }

        return null; // 如果不符合任何报警条件，则返回null
    }

    /// <summary>
    /// 获取decimal类型的报警类型
    /// </summary>
    /// <param name="tag">要检查的变量</param>
    /// <param name="limit">报警限制值</param>
    /// <param name="expressions">报警约束表达式</param>
    /// <param name="text">报警文本</param>
    /// <returns>报警类型枚举</returns>
    private static AlarmTypeEnum? GetDecimalAlarmDegree(VariableRunTime tag, out string limit, out string expressions, out string text)
    {
        limit = string.Empty; // 初始化报警限制值为空字符串
        expressions = string.Empty; // 初始化报警约束表达式为空字符串
        text = string.Empty; // 初始化报警文本为空字符串

        if (tag?.Value == null) // 检查变量是否为null或其值为null
        {
            return null; // 如果是，则返回null
        }

        // 检查是否启用了高高报警功能，并且变量的值大于高高报警的限制值
        if (tag.HHAlarmEnable && tag.Value.ToDecimal() > tag.HHAlarmCode.ToDecimal())
        {
            limit = tag.HHAlarmCode.ToString()!; // 将报警限制值设置为高高报警的限制值
            expressions = tag.HHRestrainExpressions!; // 获取高高报警的约束表达式
            text = tag.HHAlarmText!; // 获取高高报警时的报警文本
            return AlarmTypeEnum.HH; // 返回高高报警类型枚举
        }

        // 检查是否启用了高报警功能，并且变量的值大于高报警的限制值
        if (tag.HAlarmEnable && tag.Value.ToDecimal() > tag.HAlarmCode.ToDecimal())
        {
            limit = tag.HAlarmCode.ToString()!; // 将报警限制值设置为高报警的限制值
            expressions = tag.HRestrainExpressions!; // 获取高报警的约束表达式
            text = tag.HAlarmText!; // 获取高报警时的报警文本
            return AlarmTypeEnum.H; // 返回高报警类型枚举
        }

        // 检查是否启用了低报警功能，并且变量的值小于低报警的限制值
        if (tag.LAlarmEnable && tag.Value.ToDecimal() < tag.LAlarmCode.ToDecimal())
        {
            limit = tag.LAlarmCode.ToString()!; // 将报警限制值设置为低报警的限制值
            expressions = tag.LRestrainExpressions!; // 获取低报警的约束表达式
            text = tag.LAlarmText!; // 获取低报警时的报警文本
            return AlarmTypeEnum.L; // 返回低报警类型枚举
        }

        // 检查是否启用了低低报警功能，并且变量的值小于低低报警的限制值
        if (tag.LLAlarmEnable && tag.Value.ToDecimal() < tag.LLAlarmCode.ToDecimal())
        {
            limit = tag.LLAlarmCode.ToString()!; // 将报警限制值设置为低低报警的限制值
            expressions = tag.LLRestrainExpressions!; // 获取低低报警的约束表达式
            text = tag.LLAlarmText!; // 获取低低报警时的报警文本
            return AlarmTypeEnum.LL; // 返回低低报警类型枚举
        }

        return null; // 如果不符合任何报警条件，则返回null
    }

    /// <summary>
    /// 获取自定义报警类型
    /// </summary>
    /// <param name="tag">要检查的变量</param>
    /// <param name="limit">报警限制值</param>
    /// <param name="expressions">报警约束表达式</param>
    /// <param name="text">报警文本</param>
    /// <returns>报警类型枚举</returns>
    private static AlarmTypeEnum? GetCustomAlarmDegree(VariableRunTime tag, out string limit, out string expressions, out string text)
    {
        limit = string.Empty; // 初始化报警限制值为空字符串
        expressions = string.Empty; // 初始化报警约束表达式为空字符串
        text = string.Empty; // 初始化报警文本为空字符串

        if (tag?.Value == null) // 检查变量是否为null或其值为null
        {
            return null; // 如果是，则返回null
        }

        if (tag.CustomAlarmEnable) // 检查是否启用了自定义报警功能
        {
            // 调用变量的CustomAlarmCode属性的GetExpressionsResult方法，传入变量的值，获取报警表达式的计算结果
            var result = tag.CustomAlarmCode.GetExpressionsResult(tag.Value);

            if (result is bool boolResult) // 检查计算结果是否为布尔类型
            {
                if (boolResult) // 如果计算结果为true
                {
                    limit = tag.CustomAlarmCode; // 将报警限制值设置为自定义报警代码
                    expressions = tag.CustomAlarmText!; // 获取自定义报警时的报警约束表达式
                    text = tag.CustomAlarmText!; // 获取自定义报警时的报警文本
                    return AlarmTypeEnum.Custom; // 返回自定义报警类型枚举
                }
            }
        }

        return null; // 如果不符合自定义报警条件，则返回null
    }

    /// <summary>
    /// 对变量进行报警分析，并根据需要触发相应的报警事件或恢复事件。
    /// </summary>
    /// <param name="item">要进行报警分析的变量</param>
    private void AlarmAnalysis(VariableRunTime item)
    {
        string limit; // 报警限制值
        string ex; // 报警约束表达式
        string text; // 报警文本
        AlarmTypeEnum? alarmEnum; // 报警类型枚举

        // 检查变量的数据类型
        if (item.DataType.GetSystemType() == typeof(bool))
        {
            // 如果数据类型为布尔型，则调用GetBoolAlarmCode方法获取布尔型报警类型及相关信息
            alarmEnum = GetBoolAlarmCode(item, out limit, out ex, out text);
        }
        else
        {
            // 如果数据类型为非布尔型，则调用GetDecimalAlarmDegree方法获取数值型报警类型及相关信息
            alarmEnum = GetDecimalAlarmDegree(item, out limit, out ex, out text);
        }

        // 如果未获取到报警类型，则尝试获取自定义报警类型
        if (alarmEnum == null)
        {
            alarmEnum = GetCustomAlarmDegree(item, out limit, out ex, out text);
        }

        if (alarmEnum == null)
        {
            // 如果仍未获取到报警类型，则触发需恢复报警事件（如果存在）
            AlarmChange(item, null, text, EventTypeEnum.Finish, alarmEnum);
        }
        else
        {
            // 如果获取到了报警类型，则需触发报警事件或更新报警状态

            if (!string.IsNullOrEmpty(ex))
            {
                // 如果存在报警约束表达式，则计算表达式结果，以确定是否触发报警事件
                var data = ex.GetExpressionsResult(item.Value);
                if (data is bool result)
                {
                    if (result)
                    {
                        // 如果表达式结果为true，则触发报警事件
                        AlarmChange(item, limit, text, EventTypeEnum.Alarm, alarmEnum);
                    }
                }
            }
            else
            {
                // 如果不存在报警约束表达式，则直接触发报警事件
                AlarmChange(item, limit, text, EventTypeEnum.Alarm, alarmEnum);
            }
        }
    }

    /// <summary>
    /// 根据报警事件类型进行相应的处理操作，包括触发报警事件或更新报警状态。
    /// </summary>
    /// <param name="item">要处理的变量</param>
    /// <param name="limit">报警限制值</param>
    /// <param name="text">报警文本</param>
    /// <param name="eventEnum">报警事件类型枚举</param>
    /// <param name="alarmEnum">报警类型枚举</param>
    private void AlarmChange(VariableRunTime item, object limit, string text, EventTypeEnum eventEnum, AlarmTypeEnum? alarmEnum)
    {
        if (eventEnum == EventTypeEnum.Finish)
        {
            // 如果是需恢复报警事件
            // 如果实时报警列表中不存在该变量，则直接返回
            if (!RealAlarmVariables.Any(it => it.Id == item.Id))
            {
                return;
            }
        }
        else if (eventEnum == EventTypeEnum.Alarm)
        {
            // 如果是触发报警事件
            // 在实时报警列表中查找该变量
            var variable = RealAlarmVariables.FirstOrDefault(it => it.Id == item.Id);
            if (variable != null)
            {
                // 如果变量已经处于相同的报警类型，则直接返回
                if (item.AlarmType == alarmEnum)
                    return;
            }
        }

        // 更新变量的报警信息和事件时间
        if (eventEnum == EventTypeEnum.Alarm)
        {
            // 如果是触发报警事件
            item.AlarmType = alarmEnum;
            item.EventType = eventEnum;
            item.AlarmLimit = limit.ToString();
            item.AlarmCode = item.Value.ToString();
            item.AlarmText = text;
            item.AlarmTime = DateTime.Now;
            item.EventTime = DateTime.Now;
        }
        else if (eventEnum == EventTypeEnum.Finish)
        {
            // 如果是需恢复报警事件
            // 获取旧的报警信息
            var oldAlarm = RealAlarmVariables.FirstOrDefault(it => it.Id == item.Id);
            item.AlarmType = oldAlarm.AlarmType;
            item.EventType = eventEnum;
            item.AlarmLimit = oldAlarm.AlarmLimit;
            item.AlarmCode = item.Value.ToString();
            item.AlarmText = text;
            item.EventTime = DateTime.Now;
        }
        else if (eventEnum == EventTypeEnum.Check)
        {
            // 如果是确认报警事件
            item.AlarmType = alarmEnum;
            item.EventType = eventEnum;
            item.AlarmLimit = limit.ToString();
            item.AlarmCode = item.Value.ToString();
            item.AlarmText = text;
            item.EventTime = DateTime.Now;
        }

        // 触发报警变化事件
        OnAlarmChanged?.Invoke(item.Adapt<AlarmVariable>());

        if (eventEnum == EventTypeEnum.Alarm)
        {
            // 如果是触发报警事件
            //lock (RealAlarmVariables)
            {
                // 从实时报警列表中移除旧的报警信息，并添加新的报警信息
                RealAlarmVariables.RemoveWhere(it => it.Id == item.Id);
                RealAlarmVariables.Add(item);
            }
        }
        else
        {
            // 如果是需恢复报警事件或检查报警事件，则从实时报警列表中移除该变量
            RealAlarmVariables.RemoveWhere(it => it.Id == item.Id);
        }
    }

    private void DeviceVariableChange(VariableRunTime variable)
    {
        //这里不能序列化变量，报警服务需改变同一个变量指向的属性
        _deviceVariables.Enqueue(variable);
    }

    #endregion 核心实现

    #region 线程任务

    /// <summary>
    /// 执行工作任务，对设备变量进行报警分析。
    /// </summary>
    /// <param name="cancellation">取消任务的 CancellationToken</param>
    private async Task DoWork(CancellationToken cancellation)
    {
        // 延迟一段时间，避免过于频繁地执行任务
        await Task.Delay(500, cancellation).ConfigureAwait(false);

        // 获取设备变量列表
        var list = _deviceVariables.ToListWithDequeue();

        // 遍历设备变量列表
        foreach (var item in list)
        {
            // 如果取消请求已经被触发，则结束任务
            if (cancellation.IsCancellationRequested)
                return;

            // 如果该变量的报警功能未启用，则跳过该变量
            if (!item.AlarmEnable)
                continue;

            // 如果该变量离线，则跳过该变量
            if (!item.IsOnline)
                continue;

            // 对该变量进行报警分析
            AlarmAnalysis(item);
        }
    }

    #endregion 线程任务

    #region worker服务

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        HostedServiceUtil.CollectDeviceHostedService.Started += CollectDeviceHostedService_Started;
        HostedServiceUtil.CollectDeviceHostedService.Stoping += CollectDeviceHostedService_Stoping;
        GlobalData.VariableCollectChangeEvent += DeviceVariableChange;
        return base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    private Task CollectDeviceHostedService_Started()
    {
        return StartAsync();
    }

    private Task CollectDeviceHostedService_Stoping()
    {
        return StopAsync();
    }

    internal async Task StartAsync()
    {
        try
        {
            await RestartLock.WaitAsync().ConfigureAwait(false); // 等待获取锁，以确保只有一个线程可以执行以下代码

            if (RealAlarmTask != null)
            {
                await RealAlarmTask.StopAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false); // 停止现有任务，等待最多10秒钟
            }

            RealAlarmTask = new DoTask(DoWork, _logger, Localizer["RealAlarmTask"]); // 创建新的任务
            RealAlarmTask.Start(); // 启动任务
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Start"); // 记录错误日志
        }
        finally
        {
            RestartLock.Release(); // 释放锁
        }
    }

    internal async Task StopAsync()
    {
        try
        {
            await RestartLock.WaitAsync().ConfigureAwait(false); // 等待获取锁，以确保只有一个线程可以执行以下代码

            if (RealAlarmTask != null)
            {
                await RealAlarmTask.StopAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false); // 停止任务，等待最多10秒钟
            }
            RealAlarmTask = null;
            RealAlarmVariables.Clear(); // 清空任务相关的变量
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stop"); // 记录错误日志
        }
        finally
        {
            RestartLock.Release(); // 释放锁
        }
    }

    #endregion worker服务
}
