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
/// Cron 字段值含 R 字符解析器
/// </summary>
/// <remarks>
/// <para>R 表示随机生成的时刻，仅在 <see cref="CrontabFieldKind.Second"/>、<see cref="CrontabFieldKind.Minute"/> 或 <see cref="CrontabFieldKind.Hour"/> 字段域中使用。</para>
/// <para>参考文献：https://help.eset.com/protect_admin/10.0/zh-CN/cron_expression.html。</para>
/// </remarks>
internal sealed class RandomParser : ICronParser, ITimeParser
{
    /// <summary>
    /// 随机对象
    /// </summary>
    private static readonly Random random = new();

    /// <summary>
    /// Cron 字段种类最小值
    /// </summary>
    private readonly int _minimumOfKind;

    /// <summary>
    /// Cron 字段种类最大值
    /// </summary>
    private readonly int _maximumOfKind;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="kind">Cron 字段种类</param>
    /// <exception cref="TimeCrontabException"></exception>
    public RandomParser(CrontabFieldKind kind)
    {
        // 验证 R 字符是否在 Second、Minute 或 Hour 字段域中使用
        if (kind != CrontabFieldKind.Second &&
            kind != CrontabFieldKind.Minute &&
            kind != CrontabFieldKind.Hour)
        {
            throw new TimeCrontabException("The <R> parser can only be used with the Second, Minute, or Hour fields.");
        }

        Kind = kind;

        // 获取 Cron 字段种类最小值和最大值
        _minimumOfKind = Constants.MinimumDateTimeValues[Kind];
        _maximumOfKind = Constants.MaximumDateTimeValues[Kind];
    }

    /// <summary>
    /// Cron 字段种类
    /// </summary>
    public CrontabFieldKind Kind { get; }

    /// <summary>
    /// 判断当前时间是否符合 Cron 字段种类解析规则
    /// </summary>
    /// <param name="datetime">当前时间</param>
    /// <returns><see cref="bool"/></returns>
    public bool IsMatch(DateTime datetime)
    {
        return true;
    }

    /// <summary>
    /// 获取 Cron 字段种类当前值的下一个发生值
    /// </summary>
    /// <param name="currentValue">时间值</param>
    /// <returns><see cref="int"/></returns>
    /// <exception cref="TimeCrontabException"></exception>
    public int? Next(int currentValue)
    {
        // 生成最小值和最大值之间的随机数
        return random.Next(_minimumOfKind, _maximumOfKind + 1);
    }

    /// <summary>
    /// 获取 Cron 字段种类字段起始值
    /// </summary>
    /// <returns><see cref="int"/></returns>
    /// <exception cref="TimeCrontabException"></exception>
    public int First()
    {
        return 0;
    }

    /// <summary>
    /// 将解析器转换成字符串输出
    /// </summary>
    /// <returns><see cref="string"/></returns>
    public override string ToString()
    {
        return "R";
    }
}