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

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

using ThingsGateway.Gateway.Application.Extensions;
using ThingsGateway.NewLife.Extension;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 设备采集报警后台服务
/// </summary>
internal sealed class AlarmHostedService : BackgroundService, IAlarmHostedService
{
    private readonly ILogger _logger;
    /// <inheritdoc cref="AlarmHostedService"/>
    public AlarmHostedService(ILogger<AlarmHostedService> logger, IStringLocalizer<AlarmHostedService> localizer)
    {
        _logger = logger;
        Localizer = localizer;
    }

    private IStringLocalizer Localizer { get; }

    #region 核心实现

    /// <summary>
    /// 获取bool报警类型
    /// </summary>
    /// <param name="tag">要检查的变量</param>
    /// <param name="limit">报警限制值</param>
    /// <param name="expressions">报警约束表达式</param>
    /// <param name="text">报警文本</param>
    /// <returns>报警类型枚举</returns>
    private static AlarmTypeEnum? GetBoolAlarmCode(VariableRuntime tag, out string limit, out string expressions, out string text)
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
    /// 获取自定义报警类型
    /// </summary>
    /// <param name="tag">要检查的变量</param>
    /// <param name="limit">报警限制值</param>
    /// <param name="expressions">报警约束表达式</param>
    /// <param name="text">报警文本</param>
    /// <returns>报警类型枚举</returns>
    private static AlarmTypeEnum? GetCustomAlarmDegree(VariableRuntime tag, out string limit, out string expressions, out string text)
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
            var result = tag.CustomAlarmCode.GetExpressionsResult(tag.Value, tag.DeviceRuntime?.Driver?.LogMessage);

            if (result is bool boolResult) // 检查计算结果是否为布尔类型
            {
                if (boolResult) // 如果计算结果为true
                {
                    limit = tag.CustomAlarmCode; // 将报警限制值设置为自定义报警代码
                    expressions = tag.CustomRestrainExpressions!; // 获取自定义报警时的报警约束表达式
                    text = tag.CustomAlarmText!; // 获取自定义报警时的报警文本
                    return AlarmTypeEnum.Custom; // 返回自定义报警类型枚举
                }
            }
        }

        return null; // 如果不符合自定义报警条件，则返回null
    }

    /// <summary>
    /// 获取decimal类型的报警类型
    /// </summary>
    /// <param name="tag">要检查的变量</param>
    /// <param name="limit">报警限制值</param>
    /// <param name="expressions">报警约束表达式</param>
    /// <param name="text">报警文本</param>
    /// <returns>报警类型枚举</returns>
    private static AlarmTypeEnum? GetDecimalAlarmDegree(VariableRuntime tag, out string limit, out string expressions, out string text)
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


        // 检查是否启用了低低报警功能，并且变量的值小于低低报警的限制值
        if (tag.LLAlarmEnable && tag.Value.ToDecimal() < tag.LLAlarmCode.ToDecimal())
        {
            limit = tag.LLAlarmCode.ToString()!; // 将报警限制值设置为低低报警的限制值
            expressions = tag.LLRestrainExpressions!; // 获取低低报警的约束表达式
            text = tag.LLAlarmText!; // 获取低低报警时的报警文本
            return AlarmTypeEnum.LL; // 返回低低报警类型枚举
        }


        // 检查是否启用了低报警功能，并且变量的值小于低报警的限制值
        if (tag.LAlarmEnable && tag.Value.ToDecimal() < tag.LAlarmCode.ToDecimal())
        {
            limit = tag.LAlarmCode.ToString()!; // 将报警限制值设置为低报警的限制值
            expressions = tag.LRestrainExpressions!; // 获取低报警的约束表达式
            text = tag.LAlarmText!; // 获取低报警时的报警文本
            return AlarmTypeEnum.L; // 返回低报警类型枚举
        }

        return null; // 如果不符合任何报警条件，则返回null
    }

    /// <summary>
    /// 对变量进行报警分析，并根据需要触发相应的报警事件或恢复事件。
    /// </summary>
    /// <param name="item">要进行报警分析的变量</param>
    private static void AlarmAnalysis(VariableRuntime item)
    {
        string limit; // 报警限制值
        string ex; // 报警约束表达式
        string text; // 报警文本
        AlarmTypeEnum? alarmEnum; // 报警类型枚举
        int delay = item.AlarmDelay; // 获取报警延迟时间

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
            AlarmChange(item, null, text, EventTypeEnum.Finish, alarmEnum, delay);
        }
        else
        {
            // 如果获取到了报警类型，则需触发报警事件或更新报警状态

            if (!string.IsNullOrEmpty(ex))
            {
                // 如果存在报警约束表达式，则计算表达式结果，以确定是否触发报警事件
                var data = ex.GetExpressionsResult(item.Value, item.DeviceRuntime?.Driver?.LogMessage);
                if (data is bool result)
                {
                    if (result)
                    {
                        // 如果表达式结果为true，则触发报警事件
                        AlarmChange(item, limit, text, EventTypeEnum.Alarm, alarmEnum, delay);
                    }
                }
            }
            else
            {
                // 如果不存在报警约束表达式，则直接触发报警事件
                AlarmChange(item, limit, text, EventTypeEnum.Alarm, alarmEnum, delay);
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
    /// <param name="delay">报警延时</param>
    private static void AlarmChange(VariableRuntime item, object limit, string text, EventTypeEnum eventEnum, AlarmTypeEnum? alarmEnum, int delay)
    {
        bool changed = false;
        if (eventEnum == EventTypeEnum.Finish)
        {
            // 如果是需恢复报警事件
            // 如果实时报警列表中不存在该变量，则直接返回
            if (!GlobalData.RealAlarmVariables.ContainsKey(item.Name))
            {
                return;
            }
        }
        else if (eventEnum == EventTypeEnum.Alarm)
        {
            // 如果是触发报警事件
            // 在实时报警列表中查找该变量
            if (GlobalData.RealAlarmVariables.TryGetValue(item.Name, out var variable))
            {
                // 如果变量已经处于相同的报警类型，则直接返回
                if (item.AlarmType == alarmEnum)
                    return;
            }
        }

        // 更新变量的报警信息和事件时间
        if (eventEnum == EventTypeEnum.Alarm)
        {
            var now = DateTime.Now;
            //添加报警延时策略
            if (delay > 0)
            {
                if (item.EventType != EventTypeEnum.Alarm && item.EventType != EventTypeEnum.Prepare)
                {
                    item.EventType = EventTypeEnum.Prepare;//准备报警
                    item.PrepareEventTime = now;
                }
                else
                {
                    if (item.EventType == EventTypeEnum.Prepare)
                    {
                        if ((now - item.PrepareEventTime!.Value).TotalSeconds > delay)
                        {
                            //超过延时时间，触发报警
                            item.EventType = EventTypeEnum.Alarm;
                            item.AlarmTime = now;
                            item.EventTime = now;
                            item.AlarmType = alarmEnum;
                            item.AlarmLimit = limit.ToString();
                            item.AlarmCode = item.Value.ToString();
                            item.AlarmText = text;
                            item.PrepareEventTime = null;

                            changed = true;
                        }
                    }
                    else if (item.EventType == EventTypeEnum.Alarm && item.AlarmType != alarmEnum)
                    {
                        //报警类型改变，重新计时
                        if (item.PrepareEventTime == null)
                            item.PrepareEventTime = now;
                        if ((now - item.PrepareEventTime!.Value).TotalSeconds > delay)
                        {
                            //超过延时时间，触发报警
                            item.EventType = EventTypeEnum.Alarm;
                            item.AlarmTime = now;
                            item.EventTime = now;
                            item.AlarmType = alarmEnum;
                            item.AlarmLimit = limit.ToString();
                            item.AlarmCode = item.Value.ToString();
                            item.AlarmText = text;
                            item.PrepareEventTime = null;
                            changed = true;
                        }

                    }
                    else
                    {
                        return;
                    }
                }
            }
            else
            {
                // 如果是触发报警事件
                item.EventType = eventEnum;
                item.AlarmTime = now;
                item.EventTime = now;
                item.AlarmType = alarmEnum;
                item.AlarmLimit = limit.ToString();
                item.AlarmCode = item.Value.ToString();
                item.AlarmText = text;
                changed = true;
            }


        }
        else if (eventEnum == EventTypeEnum.Finish)
        {
            // 如果是需恢复报警事件
            // 获取旧的报警信息
            if (GlobalData.RealAlarmVariables.TryGetValue(item.Name, out var oldAlarm))
            {
                item.AlarmType = oldAlarm.AlarmType;
                item.EventType = eventEnum;
                item.AlarmLimit = oldAlarm.AlarmLimit;
                item.AlarmCode = item.Value.ToString();
                item.AlarmText = text;
                item.EventTime = DateTime.Now;
            }
            changed = true;
        }

        // 触发报警变化事件
        if (changed)
        {
            if (item.EventType == EventTypeEnum.Alarm)
            {
                // 如果是触发报警事件
                //lock (GlobalData. RealAlarmVariables)
                {
                    // 从实时报警列表中移除旧的报警信息，并添加新的报警信息
                    GlobalData.RealAlarmVariables.AddOrUpdate(item.Name, a => item, (a, b) => item);
                }
            }
            else if (item.EventType == EventTypeEnum.Finish)
            {
                // 如果是需恢复报警事件，则从实时报警列表中移除该变量
                GlobalData.RealAlarmVariables.TryRemove(item.Name, out _);
            }
            GlobalData.AlarmChange(item.Adapt<AlarmVariable>());
        }

    }

    public void ConfirmAlarm(string variableName)
    {
        // 如果是确认报警事件
        if (GlobalData.RealAlarmVariables.TryGetValue(variableName, out var variableRuntime))
        {
            variableRuntime.EventType = EventTypeEnum.Confirm;
            variableRuntime.EventTime = DateTime.Now;
            GlobalData.AlarmChange(variableRuntime.Adapt<AlarmVariable>());
        }
    }

    #endregion 核心实现

    /// <summary>
    /// 执行工作任务，对设备变量进行报警分析。
    /// </summary>
    /// <param name="cancellation">取消任务的 CancellationToken</param>
    private async Task DoWork(CancellationToken cancellation)
    {
        try
        {
            if (!GlobalData.StartCollectChannelEnable)
                return;

            //Stopwatch stopwatch = Stopwatch.StartNew();
            // 遍历设备变量列表

            foreach (var kv in GlobalData.AlarmEnableVariables)
            {
                // 如果取消请求已经被触发，则结束任务
                if (cancellation.IsCancellationRequested)
                    return;

                var item = kv.Value;

                // 如果该变量的报警功能未启用，则跳过该变量
                if (!item.AlarmEnable)
                    continue;

                // 如果该变量离线，则跳过该变量
                if (!item.IsOnline)
                    continue;

                // 对该变量进行报警分析
                AlarmAnalysis(item);


            }


            foreach (var item in GlobalData.RealAlarmVariables)
            {
                if (!GlobalData.AlarmEnableVariables.ContainsKey(item.Value.Id))
                {
                    if (GlobalData.RealAlarmVariables.TryRemove(item.Key, out var oldAlarm))
                    {
                        oldAlarm.EventType = EventTypeEnum.Finish;
                        oldAlarm.EventTime = DateTime.Now;
                        GlobalData.AlarmChange(item.Adapt<AlarmVariable>());
                    }
                }
            }

            //stopwatch.Stop();
            //_logger.LogInformation("报警分析耗时：" + stopwatch.ElapsedMilliseconds + "ms");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Alarm analysis fail");
        }
        finally
        {
            // 延迟一段时间，避免过于频繁地执行任务
            await Task.Delay(500, cancellation).ConfigureAwait(false);
        }
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(Localizer["RealAlarmTaskStart"]);
        while (!stoppingToken.IsCancellationRequested)
        {
            await DoWork(stoppingToken).ConfigureAwait(false);
        }
    }

}
