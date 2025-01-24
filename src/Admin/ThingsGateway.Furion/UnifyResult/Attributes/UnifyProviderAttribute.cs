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

namespace Microsoft.AspNetCore.Mvc;

/// <summary>
/// 规范化提供器特性
/// </summary>
[SuppressSniffer, AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class UnifyProviderAttribute : Attribute
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public UnifyProviderAttribute()
        : this(string.Empty)
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="name"></param>
    public UnifyProviderAttribute(string name)
    {
        Name = name;
    }

    /// <summary>
    /// 提供器名称
    /// </summary>
    public string Name { get; set; }
}