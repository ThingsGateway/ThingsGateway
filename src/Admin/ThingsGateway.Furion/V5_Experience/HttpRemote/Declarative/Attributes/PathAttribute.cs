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

namespace ThingsGateway.HttpRemote;

/// <summary>
///     HTTP 声明式路径参数特性
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface, AllowMultiple = true)]
public sealed class PathAttribute : Attribute
{
    /// <summary>
    ///     <inheritdoc cref="PathAttribute" />
    /// </summary>
    /// <param name="name">参数名称</param>
    /// <param name="value">参数值</param>
    public PathAttribute(string name, object? value)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        Value = value;
    }

    /// <summary>
    ///     路径参数键
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     路径参数的值
    /// </summary>
    public object? Value { get; set; }
}