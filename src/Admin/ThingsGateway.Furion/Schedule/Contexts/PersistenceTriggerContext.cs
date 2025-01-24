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

namespace ThingsGateway.Schedule;

/// <summary>
/// 作业触发器持久化上下文
/// </summary>
[SuppressSniffer]
public sealed class PersistenceTriggerContext : PersistenceContext
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="jobDetail">作业信息</param>
    /// <param name="trigger">作业触发器</param>
    /// <param name="behavior">作业持久化行为</param>
    internal PersistenceTriggerContext(JobDetail jobDetail
        , Trigger trigger
        , PersistenceBehavior behavior)
        : base(jobDetail, behavior)
    {
        TriggerId = trigger.TriggerId;
        Trigger = trigger;
    }

    /// <summary>
    /// 作业触发器 Id
    /// </summary>
    public string TriggerId { get; }

    /// <summary>
    /// 作业触发器
    /// </summary>
    public Trigger Trigger { get; }

    /// <summary>
    /// 触发模式
    /// </summary>
    /// <remarks>默认为定时触发</remarks>
    public int Mode { get; internal set; }

    /// <summary>
    /// 转换成 Sql 语句
    /// </summary>
    /// <param name="tableName">数据库表名</param>
    /// <param name="naming">命名法</param>
    /// <returns><see cref="string"/></returns>
    public new string ConvertToSQL(string tableName, NamingConventions naming = NamingConventions.CamelCase)
    {
        return Trigger.ConvertToSQL(tableName, Behavior, naming);
    }

    /// <summary>
    /// 转换成 JSON 语句
    /// </summary>
    /// <param name="naming">命名法</param>
    /// <returns><see cref="string"/></returns>
    public new string ConvertToJSON(NamingConventions naming = NamingConventions.CamelCase)
    {
        return Trigger.ConvertToJSON(naming);
    }

    /// <summary>
    /// 转换作业计划成 JSON 语句
    /// </summary>
    /// <param name="naming">命名法</param>
    /// <returns><see cref="string"/></returns>
    public string ConvertAllToJSON(NamingConventions naming = NamingConventions.CamelCase)
    {
        return Penetrates.Write(writer =>
        {
            writer.WriteStartObject();

            // 输出 JobDetail
            writer.WritePropertyName(Penetrates.GetNaming(nameof(JobDetail), naming));
            writer.WriteRawValue(JobDetail.ConvertToJSON(naming));

            // 输出 Trigger
            writer.WritePropertyName(Penetrates.GetNaming(nameof(Trigger), naming));
            writer.WriteRawValue(Trigger.ConvertToJSON(naming));

            writer.WriteEndObject();
        });
    }

    /// <summary>
    /// 转换成 Monitor 字符串
    /// </summary>
    /// <param name="naming">命名法</param>
    /// <returns><see cref="string"/></returns>
    public new string ConvertToMonitor(NamingConventions naming = NamingConventions.CamelCase)
    {
        return Trigger.ConvertToMonitor(naming);
    }

    /// <summary>
    /// 作业触发器持久化上下文转字符串输出
    /// </summary>
    /// <returns><see cref="String"/></returns>
    public override string ToString()
    {
        return $"{JobDetail} {Trigger} [{Behavior}]{(Trigger.NextRunTime == null ? $" [{Trigger.Status}]" : $" -> {Trigger.NextRunTime.ToFormatString()}")}";
    }
}