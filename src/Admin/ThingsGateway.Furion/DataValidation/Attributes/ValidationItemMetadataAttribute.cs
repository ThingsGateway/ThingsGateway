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

using System.Text.RegularExpressions;

namespace ThingsGateway.DataValidation;

/// <summary>
/// 验证项元数据
/// </summary>
[SuppressSniffer, AttributeUsage(AttributeTargets.Field)]
public sealed class ValidationItemMetadataAttribute : Attribute
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="regularExpression">正则表达式</param>
    /// <param name="defaultErrorMessage">失败提示默认消息</param>
    /// <param name="regexOptions">正则表达式匹配选项</param>
    public ValidationItemMetadataAttribute(string regularExpression, string defaultErrorMessage, RegexOptions regexOptions = RegexOptions.None)
    {
        RegularExpression = regularExpression;
        DefaultErrorMessage = defaultErrorMessage;
        RegexOptions = regexOptions;
    }

    /// <summary>
    /// 正则表达式
    /// </summary>
    public string RegularExpression { get; set; }

    /// <summary>
    /// 默认验证失败类型
    /// </summary>
    public string DefaultErrorMessage { get; set; }

    /// <summary>
    /// 正则表达式选项
    /// </summary>
    public RegexOptions RegexOptions { get; set; }
}