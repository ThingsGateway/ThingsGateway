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
/// Cron 字段值含 {0}#{1} 字符解析器
/// </summary>
/// <remarks>
/// <para>表示月中第{0}个星期{1}，仅在 <see cref="CrontabFieldKind.DayOfWeek"/> 字段域中使用</para>
/// </remarks>
internal sealed class SpecificDayOfWeekInMonthParser : ICronParser
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="dayOfWeek">星期，0 = 星期天，7 = 星期六</param>
    /// <param name="weekNumber">月中第几个星期</param>
    /// <param name="kind">Cron 字段种类</param>
    /// <exception cref="TimeCrontabException"></exception>
    public SpecificDayOfWeekInMonthParser(int dayOfWeek, int weekNumber, CrontabFieldKind kind)
    {
        // 验证星期数有效性
        if (weekNumber <= 0 || weekNumber > 5)
        {
            throw new TimeCrontabException(string.Format("Week number = {0} is out of bounds.", weekNumber));
        }

        // 验证 L 字符是否在 DayOfWeek 字段域中使用
        if (kind != CrontabFieldKind.DayOfWeek)
        {
            throw new TimeCrontabException(string.Format("The <{0}#{1}> parser can only be used in the Day of Week field.", dayOfWeek, weekNumber));
        }

        DayOfWeek = dayOfWeek;
        DateTimeDayOfWeek = dayOfWeek.ToDayOfWeek();
        WeekNumber = weekNumber;
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
    /// 月中第几个星期
    /// </summary>
    public int WeekNumber { get; }

    /// <summary>
    /// 判断当前时间是否符合 Cron 字段种类解析规则
    /// </summary>
    /// <param name="datetime">当前时间</param>
    /// <returns><see cref="bool"/></returns>
    public bool IsMatch(DateTime datetime)
    {
        // 获取当前时间所在月第一天
        var currentDay = new DateTime(datetime.Year, datetime.Month, 1);

        // 第几个星期计数器
        var weekCount = 0;

        // 限制当前循环仅在本月
        while (currentDay.Month == datetime.Month)
        {
            // 首先确认星期是否相等，如果相等，则计数器 + 1
            if (currentDay.DayOfWeek == DateTimeDayOfWeek)
            {
                weekCount++;

                // 如果计算器和指定 WeekNumber 一致，则退出循环
                if (weekCount == WeekNumber)
                {
                    break;
                }

                // 否则，则追加一周（即7天）进入下一次循环
                currentDay = currentDay.AddDays(7);
            }
            // 如果星期不相等，则追加一天i将纳入下一次循环
            else
            {
                currentDay = currentDay.AddDays(1);
            }
        }

        // 如果最后计算出现跨月份情况，则不匹配
        if (currentDay.Month != datetime.Month)
        {
            return false;
        }

        return datetime.Day == currentDay.Day;
    }

    /// <summary>
    /// 将解析器转换成字符串输出
    /// </summary>
    /// <returns><see cref="string"/></returns>
    public override string ToString()
    {
        return string.Format("{0}#{1}", DayOfWeek, WeekNumber);
    }
}