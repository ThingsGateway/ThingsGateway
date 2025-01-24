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

namespace ThingsGateway.SpecificationDocument;

/// <summary>
/// 用于控制 Swager 生成 Enum 类型
/// </summary>
[SuppressSniffer, AttributeUsage(AttributeTargets.Enum, AllowMultiple = false)]
public sealed class EnumToNumberAttribute : Attribute
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public EnumToNumberAttribute()
        : this(true)
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="enabled">启用状态</param>
    public EnumToNumberAttribute(bool enabled = true)
    {
        Enabled = enabled;
    }

    /// <summary>
    /// 启用状态
    /// </summary>
    /// <remarks>设置 false 则使用字符串类型</remarks>
    public bool Enabled { get; set; }
}