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
/// TimeCrontab 模块异常类
/// </summary>
public sealed class TimeCrontabException : Exception
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public TimeCrontabException()
        : base()
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="message">异常消息</param>
    public TimeCrontabException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="message">异常消息</param>
    /// <param name="innerException">内部异常</param>
    public TimeCrontabException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}