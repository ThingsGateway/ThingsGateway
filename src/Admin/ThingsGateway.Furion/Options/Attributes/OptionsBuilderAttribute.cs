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

namespace ThingsGateway.Options;

/// <summary>
/// 选项构建器特性
/// </summary>
[SuppressSniffer, AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class OptionsBuilderAttribute : Attribute
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public OptionsBuilderAttribute()
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="sectionKey">配置节点</param>
    public OptionsBuilderAttribute(string sectionKey)
    {
        SectionKey = sectionKey;
    }

    /// <summary>
    /// 配置节点
    /// </summary>
    public string SectionKey { get; set; }

    /// <summary>
    /// 未知配置节点抛异常
    /// </summary>
    public bool ErrorOnUnknownConfiguration { get; set; }

    /// <summary>
    /// 绑定非公开属性
    /// </summary>
    public bool BindNonPublicProperties { get; set; }

    /// <summary>
    /// 启用验证特性支持
    /// </summary>
    public bool ValidateDataAnnotations { get; set; }

    /// <summary>
    /// 验证选项类型
    /// </summary>
    public Type[] ValidateOptionsTypes { get; set; }
}