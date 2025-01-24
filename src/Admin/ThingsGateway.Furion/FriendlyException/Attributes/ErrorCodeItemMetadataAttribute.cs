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

namespace ThingsGateway.FriendlyException;

/// <summary>
/// 异常元数据特性
/// </summary>
[SuppressSniffer, AttributeUsage(AttributeTargets.Field)]
public sealed class ErrorCodeItemMetadataAttribute : Attribute
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="errorMessage">错误消息</param>
    /// <param name="args">格式化参数</param>
    public ErrorCodeItemMetadataAttribute(string errorMessage, params object[] args)
    {
        ErrorMessage = errorMessage;
        Args = args;
    }

    /// <summary>
    /// 错误消息
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// 错误码
    /// </summary>
    public object ErrorCode { get; set; }

    /// <summary>
    /// 格式化参数
    /// </summary>
    public object[] Args { get; set; }
}