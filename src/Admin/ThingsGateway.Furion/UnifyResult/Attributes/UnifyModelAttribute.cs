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

namespace ThingsGateway.UnifyResult;

/// <summary>
/// 规范化模型特性
/// </summary>
[SuppressSniffer, AttributeUsage(AttributeTargets.Class)]
public sealed class UnifyModelAttribute : Attribute
{
    /// <summary>
    /// 规范化模型
    /// </summary>
    /// <param name="modelType"></param>
    public UnifyModelAttribute(Type modelType)
    {
        ModelType = modelType;
    }

    /// <summary>
    /// 模型类型（泛型）
    /// </summary>
    public Type ModelType { get; set; }
}