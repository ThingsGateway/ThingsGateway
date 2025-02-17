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
/// Cron 表达式抽象类
/// </summary>
/// <remarks>主要将 Cron 表达式转换成 OOP 类进行操作</remarks>
public sealed partial class Crontab
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <remarks>禁止外部 new 实例化</remarks>
    private Crontab()
    {
        Parsers = new Dictionary<CrontabFieldKind, List<ICronParser>>();
        Format = CronStringFormat.Default;
    }

    /// <summary>
    /// Cron 字段解析器字典集合
    /// </summary>
    private Dictionary<CrontabFieldKind, List<ICronParser>> Parsers { get; set; }

    /// <summary>
    /// Cron 表达式格式化类型
    /// </summary>
    /// <remarks>禁止运行时更改</remarks>
    public CronStringFormat Format { get; private set; }

    /// <summary>
    /// 解析 Cron 表达式并转换成 <see cref="Crontab"/> 对象
    /// </summary>
    /// <param name="expression">Cron 表达式</param>
    /// <param name="format">Cron 表达式格式化类型</param>
    /// <returns><see cref="Crontab"/></returns>
    /// <exception cref="TimeCrontabException"></exception>
    public static Crontab Parse(string expression, CronStringFormat format = CronStringFormat.Default)
    {
        // 处理 Macro 表达式
        if (expression.StartsWith('@'))
        {
            return expression switch
            {
                "@secondly" => Secondly,
                "@minutely" => Minutely,
                "@hourly" => Hourly,
                "@daily" => Daily,
                "@monthly" => Monthly,
                "@weekly" => Weekly,
                "@yearly" => Yearly,
                "@workday" => Workday,
                _ => throw new NotImplementedException(),
            };
        }

        return new Crontab
        {
            Format = format,
            Parsers = ParseToDictionary(expression, format)
        };
    }

    /// <summary>
    /// 解析 Cron Macro 符号并转换成 <see cref="Crontab"/> 对象
    /// </summary>
    /// <param name="macro">Macro 符号</param>
    /// <param name="fields">字段值</param>
    /// <returns><see cref="Crontab"/></returns>
    /// <exception cref="TimeCrontabException"></exception>
    public static Crontab ParseAt(string macro, params object[] fields)
    {
        // 空检查
        if (string.IsNullOrWhiteSpace(macro)) throw new ArgumentNullException(nameof(macro));

        return macro switch
        {
            "@secondly" => SecondlyAt(fields),
            "@minutely" => MinutelyAt(fields),
            "@hourly" => HourlyAt(fields),
            "@daily" => DailyAt(fields),
            "@monthly" => MonthlyAt(fields),
            "@weekly" => WeeklyAt(fields),
            "@yearly" => YearlyAt(fields),
            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    /// 解析 Cron 表达式并转换成 <see cref="Crontab"/> 对象
    /// </summary>
    /// <remarks>解析失败返回 default</remarks>
    /// <param name="expression">Cron 表达式</param>
    /// <param name="format">Cron 表达式格式化类型</param>
    /// <returns><see cref="Crontab"/></returns>
    public static Crontab TryParse(string expression, CronStringFormat format = CronStringFormat.Default)
    {
        try
        {
            return Parse(expression, format);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 判断 Cron 表达式是否有效
    /// </summary>
    /// <param name="expression">Cron 表达式</param>
    /// <param name="format">Cron 表达式格式化类型</param>
    /// <returns><see cref="Crontab"/></returns>
    public static bool IsValid(string expression, CronStringFormat format = CronStringFormat.Default)
    {
        return TryParse(expression, format) != null;
    }

    /// <summary>
    /// 获取起始时间下一个发生时间
    /// </summary>
    /// <param name="baseTime">起始时间</param>
    /// <returns><see cref="DateTime"/></returns>
    public DateTime GetNextOccurrence(DateTime baseTime)
    {
        return GetNextOccurrence(baseTime, DateTime.MaxValue);
    }

    /// <summary>
    /// 获取起始时间上一个发生时间
    /// </summary>
    /// <param name="baseTime">起始时间</param>
    /// <returns><see cref="DateTime"/></returns>
    public DateTime GetPreviousOccurrence(DateTime baseTime)
    {
        return GetPreviousOccurrence(baseTime, DateTime.MinValue);
    }

    /// <summary>
    /// 获取特定时间范围下一个发生时间
    /// </summary>
    /// <param name="baseTime">起始时间</param>
    /// <param name="endTime">结束时间</param>
    /// <returns><see cref="DateTime"/></returns>
    public DateTime GetNextOccurrence(DateTime baseTime, DateTime endTime)
    {
        return InternalGetNextOccurence(baseTime, endTime);
    }

    /// <summary>
    /// 获取特定时间范围上一个发生时间
    /// </summary>
    /// <param name="baseTime">起始时间</param>
    /// <param name="endTime">结束时间</param>
    public DateTime GetPreviousOccurrence(DateTime baseTime, DateTime endTime)
    {
        return InternalGetPreviousOccurence(baseTime, endTime);
    }

    /// <summary>
    /// 获取特定时间范围所有发生时间
    /// </summary>
    /// <param name="baseTime">起始时间</param>
    /// <param name="endTime">结束时间</param>
    /// <returns><see cref="IEnumerable{T}"/></returns>
    public IEnumerable<DateTime> GetNextOccurrences(DateTime baseTime, DateTime endTime)
    {
        for (var occurrence = GetNextOccurrence(baseTime, endTime);
             occurrence < endTime;
             occurrence = GetNextOccurrence(occurrence, endTime))
        {
            yield return occurrence;
        }
    }

    /// <summary>
    /// 获取特定时间范围所有发生时间
    /// </summary>
    /// <param name="baseTime">起始时间</param>
    /// <param name="endTime">结束时间</param>
    /// <returns><see cref="IEnumerable{T}"/></returns>
    public IEnumerable<DateTime> GetPreviousOccurrences(DateTime baseTime, DateTime endTime)
    {
        for (var occurrence = GetPreviousOccurrence(baseTime, endTime);
            occurrence > endTime;
            occurrence = GetPreviousOccurrence(occurrence, endTime))
        {
            yield return occurrence;
        }
    }

    /// <summary>
    /// 计算距离下一个发生时间相差毫秒数
    /// </summary>
    /// <param name="baseTime">起始时间</param>
    /// <returns><see cref="double"/></returns>
    public double GetSleepMilliseconds(DateTime baseTime)
    {
        // 采用 DateTimeKind.Unspecified 转换当前时间并忽略毫秒之后部分
        var startAt = new DateTime(baseTime.Year
            , baseTime.Month
            , baseTime.Day
            , baseTime.Hour
            , baseTime.Minute
            , baseTime.Second
            , baseTime.Millisecond);

        // 计算总休眠时间
        return (GetNextOccurrence(startAt) - startAt).TotalMilliseconds;
    }

    /// <summary>
    /// 计算距离下一个发生时间相差时间戳
    /// </summary>
    /// <param name="baseTime">起始时间</param>
    /// <returns><see cref="TimeSpan"/></returns>
    public TimeSpan GetSleepTimeSpan(DateTime baseTime)
    {
        return TimeSpan.FromMilliseconds(GetSleepMilliseconds(baseTime));
    }

    /// <summary>
    /// 将 <see cref="Crontab"/> 对象转换成 Cron 表达式字符串
    /// </summary>
    /// <returns><see cref="string"/></returns>
    public override string ToString()
    {
        var paramList = new List<string>();

        // 判断当前 Cron 格式化类型是否包含秒字段域
        if (Format == CronStringFormat.WithSeconds || Format == CronStringFormat.WithSecondsAndYears)
        {
            JoinParsers(paramList, CrontabFieldKind.Second);
        }

        // Cron 常规字段域
        JoinParsers(paramList, CrontabFieldKind.Minute);
        JoinParsers(paramList, CrontabFieldKind.Hour);
        JoinParsers(paramList, CrontabFieldKind.Day);
        JoinParsers(paramList, CrontabFieldKind.Month);
        JoinParsers(paramList, CrontabFieldKind.DayOfWeek);

        // 判断当前 Cron 格式化类型是否包含年字段域
        if (Format == CronStringFormat.WithYears || Format == CronStringFormat.WithSecondsAndYears)
        {
            JoinParsers(paramList, CrontabFieldKind.Year);
        }

        // 空格分割并输出
        return string.Join(" ", paramList.ToArray());
    }
}