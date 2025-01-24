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

namespace ThingsGateway;

/// <summary>
///     组件元数据
/// </summary>
internal readonly struct ComponentMetadata
{
    /// <summary>
    ///     <inheritdoc cref="ComponentMetadata" />
    /// </summary>
    /// <param name="name">组件名称</param>
    /// <param name="version">版本号</param>
    /// <param name="description">描述</param>
    internal ComponentMetadata(string name, Version? version, string? description)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        Version = version?.ToString();
        Description = description;

    }

    /// <summary>
    ///     组件名称
    /// </summary>
    internal string Name { get; init; }

    /// <summary>
    ///     版本号
    /// </summary>
    internal string? Version { get; init; }

    /// <summary>
    ///     描述
    /// </summary>
    internal string? Description { get; init; }

}