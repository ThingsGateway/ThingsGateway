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

using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ThingsGateway.DataValidation;

/// <summary>
/// 验证信息元数据
/// </summary>
public sealed class ValidationMetadata
{
    /// <summary>
    /// 验证结果
    /// </summary>
    /// <remarks>返回字典或字符串类型</remarks>
    public object ValidationResult { get; internal set; }

    /// <summary>
    /// 异常消息
    /// </summary>
    public string Message { get; internal set; }

    /// <summary>
    /// 验证状态
    /// </summary>
    public ModelStateDictionary ModelState { get; internal set; }

    /// <summary>
    /// 错误码
    /// </summary>
    public object ErrorCode { get; internal set; }

    /// <summary>
    /// 错误码（没被复写过的 ErrorCode ）
    /// </summary>
    public object OriginErrorCode { get; internal set; }

    /// <summary>
    /// 状态码
    /// </summary>
    public int? StatusCode { get; internal set; }

    /// <summary>
    /// 首个错误属性
    /// </summary>
    public string FirstErrorProperty { get; internal set; }

    /// <summary>
    /// 首个错误消息
    /// </summary>
    public string FirstErrorMessage { get; internal set; }

    /// <summary>
    /// 额外数据
    /// </summary>
    public object Data { get; internal set; }

    /// <summary>
    /// 默认只显示验证错误的首个消息
    /// </summary>
    public bool SingleValidationErrorDisplay { get; set; }
}