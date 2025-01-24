﻿// ------------------------------------------------------------------------
// 版权信息
// 版权归百小僧及百签科技（广东）有限公司所有。
// 所有权利保留。
// 官方网站：https://baiqian.com
//
// 许可证信息
// 项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。
// 许可证的完整文本可以在源代码树根目录中的 LICENSE-APACHE 和 LICENSE-MIT 文件中找到。
// ------------------------------------------------------------------------

using ThingsGateway.TimeCrontab;

namespace ThingsGateway.Schedule;

/// <summary>
/// 作业触发器静态类
/// </summary>
[SuppressSniffer]
public static class Triggers
{
    /// <summary>
    /// 创建毫秒周期（间隔）作业触发器构建器
    /// </summary>
    /// <param name="interval">间隔（毫秒）</param>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public static TriggerBuilder Period(long interval)
    {
        return TriggerBuilder.Period(interval);
    }

    /// <summary>
    /// 创建秒周期（间隔）作业触发器构建器
    /// </summary>
    /// <param name="interval">间隔（秒）</param>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public static TriggerBuilder PeriodSeconds(long interval)
    {
        return Period(interval * 1000);
    }

    /// <summary>
    /// 创建分钟周期（间隔）作业触发器构建器
    /// </summary>
    /// <param name="interval">间隔（分钟）</param>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public static TriggerBuilder PeriodMinutes(long interval)
    {
        return Period(interval * 1000 * 60);
    }

    /// <summary>
    /// 创建小时周期（间隔）作业触发器构建器
    /// </summary>
    /// <param name="interval">间隔（小时）</param>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public static TriggerBuilder PeriodHours(long interval)
    {
        return Period(interval * 1000 * 60 * 60);
    }

    /// <summary>
    /// 创建 Cron 表达式作业触发器构建器
    /// </summary>
    /// <param name="schedule">Cron 表达式</param>
    /// <param name="format">Cron 表达式格式化类型，默认 <see cref="CronStringFormat.Default"/></param>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public static TriggerBuilder Cron(string schedule, CronStringFormat format = CronStringFormat.Default)
    {
        return TriggerBuilder.Cron(schedule, format);
    }

    /// <summary>
    /// 创建 Cron 表达式作业触发器构建器
    /// </summary>
    /// <param name="schedule">Cron 表达式</param>
    /// <param name="args">动态参数类型，支持 <see cref="int"/> 和 object[]</param>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public static TriggerBuilder Cron(string schedule, object args)
    {
        return TriggerBuilder.Cron(schedule, args);
    }

    /// <summary>
    /// 创建作业触发器构建器
    /// </summary>
    /// <param name="triggerId">作业触发器 Id</param>
    /// <returns><see cref="JobBuilder"/></returns>
    public static TriggerBuilder Create(string triggerId)
    {
        return TriggerBuilder.Create(triggerId);
    }

    /// <summary>
    /// 创建作业触发器构建器
    /// </summary>
    /// <typeparam name="TTrigger"><see cref="Trigger"/> 派生类</typeparam>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public static TriggerBuilder Create<TTrigger>()
        where TTrigger : Trigger
    {
        return TriggerBuilder.Create<TTrigger>();
    }

    /// <summary>
    /// 创建作业触发器构建器
    /// </summary>
    /// <typeparam name="TTrigger"><see cref="Trigger"/> 派生类</typeparam>
    /// <param name="args">作业触发器参数</param>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public static TriggerBuilder Create<TTrigger>(params object[] args)
        where TTrigger : Trigger
    {
        return TriggerBuilder.Create<TTrigger>(args);
    }

    /// <summary>
    /// 创建作业触发器构建器
    /// </summary>
    /// <param name="assemblyName">作业触发器类型所在程序集 Name</param>
    /// <param name="triggerTypeFullName">作业触发器类型 FullName</param>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public static TriggerBuilder Create(string assemblyName, string triggerTypeFullName)
    {
        return TriggerBuilder.Create(assemblyName, triggerTypeFullName);
    }

    /// <summary>
    /// 创建作业触发器构建器
    /// </summary>
    /// <param name="assemblyName">作业触发器类型所在程序集 Name</param>
    /// <param name="triggerTypeFullName">作业触发器类型 FullName</param>
    /// <param name="args">作业触发器参数</param>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public static TriggerBuilder Create(string assemblyName, string triggerTypeFullName, params object[] args)
    {
        return TriggerBuilder.Create(assemblyName, triggerTypeFullName, args);
    }

    /// <summary>
    /// 创建作业触发器构建器
    /// </summary>
    /// <param name="triggerType"><see cref="Trigger"/> 派生类</param>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public static TriggerBuilder Create(Type triggerType)
    {
        return TriggerBuilder.Create(triggerType);
    }

    /// <summary>
    /// 创建作业触发器构建器
    /// </summary>
    /// <param name="triggerType"><see cref="Trigger"/> 派生类</param>
    /// <param name="args">作业触发器参数</param>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public static TriggerBuilder Create(Type triggerType, params object[] args)
    {
        return TriggerBuilder.Create(triggerType, args);
    }

    /// <summary>
    /// 将 <see cref="Trigger"/> 转换成 <see cref="TriggerBuilder"/>
    /// </summary>
    /// <param name="trigger">作业触发器</param>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public static TriggerBuilder From(Trigger trigger)
    {
        return TriggerBuilder.From(trigger);
    }

    /// <summary>
    /// 将 JSON 字符串转换成 <see cref="TriggerBuilder"/>
    /// </summary>
    /// <param name="json">JSON 字符串</param>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public static TriggerBuilder From(string json)
    {
        return TriggerBuilder.From(json);
    }

    /// <summary>
    /// 克隆作业触发器构建器
    /// </summary>
    /// <param name="fromTriggerBuilder">被克隆的作业触发器构建器</param>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public static TriggerBuilder Clone(TriggerBuilder fromTriggerBuilder)
    {
        return TriggerBuilder.Clone(fromTriggerBuilder);
    }

    /// <summary>
    /// 创建每秒开始作业触发器构建器
    /// </summary>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public static TriggerBuilder Secondly()
    {
        return Cron("@secondly");
    }

    /// <summary>
    /// 创建指定特定秒开始作业触发器构建器
    /// </summary>
    /// <param name="fields">字段值</param>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public static TriggerBuilder SecondlyAt(params object[] fields)
    {
        return Cron("@secondly", fields);
    }

    /// <summary>
    /// 创建每分钟开始作业触发器构建器
    /// </summary>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public static TriggerBuilder Minutely()
    {
        return Cron("@minutely");
    }

    /// <summary>
    /// 创建每分钟特定秒开始作业触发器构建器
    /// </summary>
    /// <param name="fields">字段值</param>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public static TriggerBuilder MinutelyAt(params object[] fields)
    {
        return Cron("@minutely", fields);
    }

    /// <summary>
    /// 创建每小时开始作业触发器构建器
    /// </summary>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public static TriggerBuilder Hourly()
    {
        return Cron("@hourly");
    }

    /// <summary>
    /// 创建每小时特定分钟开始作业触发器构建器
    /// </summary>
    /// <param name="fields">字段值</param>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public static TriggerBuilder HourlyAt(params object[] fields)
    {
        return Cron("@hourly", fields);
    }

    /// <summary>
    /// 创建每天（午夜）开始作业触发器构建器
    /// </summary>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public static TriggerBuilder Daily()
    {
        return Cron("@daily");
    }

    /// <summary>
    /// 创建每天特定小时开始作业触发器构建器
    /// </summary>
    /// <param name="fields">字段值</param>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public static TriggerBuilder DailyAt(params object[] fields)
    {
        return Cron("@daily", fields);
    }

    /// <summary>
    /// 创建每月1号（午夜）开始作业触发器构建器
    /// </summary>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public static TriggerBuilder Monthly()
    {
        return Cron("@monthly");
    }

    /// <summary>
    /// 创建每月特定天（午夜）开始作业触发器构建器
    /// </summary>
    /// <param name="fields">字段值</param>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public static TriggerBuilder MonthlyAt(params object[] fields)
    {
        return Cron("@monthly", fields);
    }

    /// <summary>
    /// 创建每周日（午夜）开始作业触发器构建器
    /// </summary>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public static TriggerBuilder Weekly()
    {
        return Cron("@weekly");
    }

    /// <summary>
    /// 创建每周特定星期几（午夜）开始作业触发器构建器
    /// </summary>
    /// <param name="fields">字段值</param>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public static TriggerBuilder WeeklyAt(params object[] fields)
    {
        return Cron("@weekly", fields);
    }

    /// <summary>
    /// 创建每年1月1号（午夜）开始作业触发器构建器
    /// </summary>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public static TriggerBuilder Yearly()
    {
        return Cron("@yearly");
    }

    /// <summary>
    /// 创建每年特定月1号（午夜）开始作业触发器构建器
    /// </summary>
    /// <param name="fields">字段值</param>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public static TriggerBuilder YearlyAt(params object[] fields)
    {
        return Cron("@yearly", fields);
    }

    /// <summary>
    /// 创建每周一至周五（午夜）开始作业触发器构建器
    /// </summary>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public static TriggerBuilder Workday()
    {
        return Cron("@workday");
    }
}