// ------------------------------------------------------------------------
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
    /// 创建指定特定秒开始作业触发器构建器
    /// </summary>
    /// <param name="fields">字段值</param>
    /// <returns><see cref="Crontab"/></returns>
    public static Crontab SecondlyAt(params object[] fields)
    {
        // 检查字段合法性
        CheckFieldsNotNullOrEmpty(fields);

        return Parse($"{FieldsToString(fields)} * * * * *", CronStringFormat.WithSeconds);
    }

    /// <summary>
    /// 创建每分钟特定秒开始作业触发器构建器
    /// </summary>
    /// <param name="fields">字段值</param>
    /// <returns><see cref="Crontab"/></returns>
    public static Crontab MinutelyAt(params object[] fields)
    {
        // 检查字段合法性
        CheckFieldsNotNullOrEmpty(fields);

        return Parse($"{FieldsToString(fields)} * * * * *", CronStringFormat.WithSeconds);
    }

    /// <summary>
    /// 创建每小时特定分钟开始作业触发器构建器
    /// </summary>
    /// <param name="fields">字段值</param>
    /// <returns><see cref="Crontab"/></returns>
    public static Crontab HourlyAt(params object[] fields)
    {
        // 检查字段合法性
        CheckFieldsNotNullOrEmpty(fields);

        return Parse($"{FieldsToString(fields)} * * * *", CronStringFormat.Default);
    }

    /// <summary>
    /// 创建每天特定小时开始作业触发器构建器
    /// </summary>
    /// <param name="fields">字段值</param>
    /// <returns><see cref="Crontab"/></returns>
    public static Crontab DailyAt(params object[] fields)
    {
        // 检查字段合法性
        CheckFieldsNotNullOrEmpty(fields);

        return Parse($"0 {FieldsToString(fields)} * * *", CronStringFormat.Default);
    }

    /// <summary>
    /// 创建每月特定天（午夜）开始作业触发器构建器
    /// </summary>
    /// <param name="fields">字段值</param>
    /// <returns><see cref="Crontab"/></returns>
    public static Crontab MonthlyAt(params object[] fields)
    {
        // 检查字段合法性
        CheckFieldsNotNullOrEmpty(fields);

        return Parse($"0 0 {FieldsToString(fields)} * *", CronStringFormat.Default);
    }

    /// <summary>
    /// 创建每周特定星期几（午夜）开始作业触发器构建器
    /// </summary>
    /// <param name="fields">字段值</param>
    /// <returns><see cref="Crontab"/></returns>
    public static Crontab WeeklyAt(params object[] fields)
    {
        // 检查字段合法性
        CheckFieldsNotNullOrEmpty(fields);

        return Parse($"0 0 * * {FieldsToString(fields)}", CronStringFormat.Default);
    }

    /// <summary>
    /// 创建每年特定月1号（午夜）开始作业触发器构建器
    /// </summary>
    /// <param name="fields">字段值</param>
    /// <returns><see cref="Crontab"/></returns>
    public static Crontab YearlyAt(params object[] fields)
    {
        // 检查字段合法性
        CheckFieldsNotNullOrEmpty(fields);

        return Parse($"0 0 1 {FieldsToString(fields)} *", CronStringFormat.Default);
    }

    /// <summary>
    /// 检查字段域 非 Null 非空数组
    /// </summary>
    /// <param name="fields">字段值</param>
    private static void CheckFieldsNotNullOrEmpty(params object[] fields)
    {
        // 空检查
        if (fields == null || fields.Length == 0) throw new ArgumentNullException(nameof(fields));

        // 检查 fields 只能是 int, long，string 和非 null 类型
        if (fields.Any(f => f == null || (f.GetType() != typeof(int) && f.GetType() != typeof(long) && f.GetType() != typeof(string)))) throw new InvalidOperationException("Invalid Cron expression.");
    }

    /// <summary>
    /// 将字段域转换成 string
    /// </summary>
    /// <param name="fields">字段值</param>
    /// <returns><see cref="string"/></returns>
    private static string FieldsToString(params object[] fields)
    {
        return string.Join(",", fields);
    }
}