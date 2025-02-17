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
/// Cron 字段值含 数值 字符解析器
/// </summary>
/// <remarks>
/// <para>表示具体值，这里仅处理 <see cref="CrontabFieldKind.Year"/> 字段域</para>
/// </remarks>
internal sealed class SpecificYearParser : SpecificParser
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="specificValue">年（具体值)</param>
    /// <param name="kind">Cron 字段种类</param>
    public SpecificYearParser(int specificValue, CrontabFieldKind kind)
        : base(specificValue, kind)
    {
    }

    /// <summary>
    /// 获取 Cron 字段种类当前值的下一个发生值
    /// </summary>
    /// <param name="currentValue">时间值</param>
    /// <returns><see cref="int"/></returns>
    public override int? Next(int currentValue)
    {
        // 如果当前年份小于具体值，则返回具体值，否则返回 null
        // 因为一旦指定了年份，那么就必须等到那一年才触发
        return currentValue < SpecificValue ? SpecificValue : null;
    }

    /// <summary>
    /// 获取 Cron 字段种类当前值的上一个发生值
    /// </summary>
    /// <param name="currentValue">时间值</param>
    /// <returns><see cref="int"/></returns>
    public override int? Previous(int currentValue)
    {
        return currentValue > SpecificValue ? SpecificValue : null;
    }
}