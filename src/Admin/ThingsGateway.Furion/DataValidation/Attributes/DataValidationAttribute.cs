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

using ThingsGateway;
using ThingsGateway.DataValidation;

namespace System.ComponentModel.DataAnnotations;

/// <summary>
/// 数据类型验证特性
/// </summary>
[SuppressSniffer]
public sealed class DataValidationAttribute : ValidationAttribute
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="validationPattern">验证逻辑</param>
    /// <param name="validationTypes"></param>
    public DataValidationAttribute(ValidationPattern validationPattern, params object[] validationTypes)
    {
        ValidationPattern = validationPattern;
        ValidationTypes = validationTypes;
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="validationTypes"></param>
    public DataValidationAttribute(params object[] validationTypes)
    {
        ValidationPattern = ValidationPattern.AllOfThem;
        ValidationTypes = validationTypes;
    }

    /// <summary>
    /// 验证逻辑
    /// </summary>
    /// <param name="value"></param>
    /// <param name="validationContext"></param>
    /// <returns></returns>
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        // 判断是否允许 空值
        if (AllowNullValue && value == null) return ValidationResult.Success;

        // 是否忽略空字符串
        if (AllowEmptyStrings && value is string && string.IsNullOrEmpty(value?.ToString())) return ValidationResult.Success;

        // 执行值验证
        var dataValidationResult = value.TryValidate(ValidationPattern, ValidationTypes);
        dataValidationResult.MemberOrValue = validationContext.DisplayName ?? validationContext.MemberName;

        // 验证失败
        if (!dataValidationResult.IsValid)
        {
            var resultMessage = dataValidationResult.ValidationResults.FirstOrDefault().ErrorMessage;

            // 进行多语言处理
            var errorMessage = !string.IsNullOrWhiteSpace(ErrorMessage) ? ErrorMessage : resultMessage;

            //TODO: 修改为类型本地化
            return new ValidationResult(string.Format(App.StringLocalizerFactory == null ? errorMessage : App.CreateLocalizerByType(validationContext.ObjectType)[errorMessage], validationContext.DisplayName ?? validationContext.MemberName));
        }

        // 验证成功
        return ValidationResult.Success;
    }

    /// <summary>
    /// 验证类型
    /// </summary>
    public object[] ValidationTypes { get; set; }

    /// <summary>
    /// 验证逻辑
    /// </summary>
    public ValidationPattern ValidationPattern { get; set; }

    /// <summary>
    ///是否允许空字符串
    /// </summary>
    public bool AllowEmptyStrings { get; set; } = false;

    /// <summary>
    /// 允许空值，有值才验证，默认 false
    /// </summary>
    public bool AllowNullValue { get; set; } = false;
}