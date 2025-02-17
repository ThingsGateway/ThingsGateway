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
/// DateTime 时间解析器依赖接口
/// </summary>
/// <remarks>主要用于计算 DateTime 主要组成部分（秒，分，时，年）的下一个取值</remarks>
internal interface ITimeParser
{
    /// <summary>
    /// 获取 Cron 字段种类当前值的下一个发生值
    /// </summary>
    /// <param name="currentValue">时间值</param>
    /// <returns><see cref="int"/></returns>
    int? Next(int currentValue);

    /// <summary>
    /// 获取 Cron 字段种类当前值的上一个发生值
    /// </summary>
    /// <param name="currentValue">时间值</param>
    /// <returns><see cref="int"/></returns>
    int? Previous(int currentValue);

    /// <summary>
    /// 获取 Cron 字段种类字段起始值
    /// </summary>
    /// <returns><see cref="int"/></returns>
    int First();

    /// <summary>
    /// 获取 Cron 字段种类字段末尾值
    /// </summary>
    /// <returns><see cref="int"/></returns>
    int Last();
}