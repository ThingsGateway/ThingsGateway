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

namespace ThingsGateway.TimeCrontab;

/// <summary>
/// Cron 字段值含 {0}L 字符解析器
/// </summary>
/// <remarks>
/// <para>表示月中最后一个星期{0}，仅在 <see cref="CrontabFieldKind.DayOfWeek"/> 字段域中使用</para>
/// </remarks>
internal sealed class LastDayOfWeekInMonthParser : ICronParser
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="dayOfWeek">星期，0 = 星期天，7 = 星期六</param>
    /// <param name="kind">Cron 字段种类</param>
    /// <exception cref="TimeCrontabException"></exception>
    public LastDayOfWeekInMonthParser(int dayOfWeek, CrontabFieldKind kind)
    {
        // 验证 {0}L 字符是否在 DayOfWeek 字段域中使用
        if (kind != CrontabFieldKind.DayOfWeek)
        {
            throw new TimeCrontabException(string.Format("The <{0}L> parser can only be used in the Day of Week field.", dayOfWeek));
        }

        DayOfWeek = dayOfWeek;
        DateTimeDayOfWeek = dayOfWeek.ToDayOfWeek();
        Kind = kind;
    }

    /// <summary>
    /// Cron 字段种类
    /// </summary>
    public CrontabFieldKind Kind { get; }

    /// <summary>
    /// 星期
    /// </summary>
    public int DayOfWeek { get; }

    /// <summary>
    /// <see cref="DayOfWeek"/> 类型星期
    /// </summary>
    private DayOfWeek DateTimeDayOfWeek { get; }

    /// <summary>
    /// 判断当前时间是否符合 Cron 字段种类解析规则
    /// </summary>
    /// <param name="datetime">当前时间</param>
    /// <returns><see cref="bool"/></returns>
    public bool IsMatch(DateTime datetime)
    {
        return datetime.Day == DateTimeDayOfWeek.LastDayOfMonth(datetime.Year, datetime.Month);
    }

    /// <summary>
    /// 将解析器转换成字符串输出
    /// </summary>
    /// <returns><see cref="string"/></returns>
    public override string ToString()
    {
        return string.Format("{0}L", DayOfWeek);
    }
}