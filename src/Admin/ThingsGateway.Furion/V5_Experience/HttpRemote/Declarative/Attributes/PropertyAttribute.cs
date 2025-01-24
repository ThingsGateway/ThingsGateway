﻿// ------------------------------------------------------------------------
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
///     HTTP 声明式 <see cref="HttpRequestMessage" /> 请求属性特性
/// </summary>
/// <remarks>支持多次指定。</remarks>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface | AttributeTargets.Parameter,
    AllowMultiple = true)]
public sealed class PropertyAttribute : Attribute
{
    /// <summary>
    ///     <inheritdoc cref="PropertyAttribute" />
    /// </summary>
    /// <remarks>特性作用于参数时有效。</remarks>
    public PropertyAttribute()
    {
    }

    /// <summary>
    ///     <inheritdoc cref="PropertyAttribute" />
    /// </summary>
    /// <remarks>
    ///     当特性作用于参数时，则表示添加 <see cref="HttpRequestMessage" /> 请求属性，同时设置 <see cref="HttpRequestMessage" /> 请求属性键为
    ///     <c>name</c> 的值。
    /// </remarks>
    /// <param name="name"><see cref="HttpRequestMessage" /> 请求属性键</param>
    public PropertyAttribute(string name)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
    }

    /// <summary>
    ///     <inheritdoc cref="PropertyAttribute" />
    /// </summary>
    /// <param name="name"><see cref="HttpRequestMessage" /> 请求属性键</param>
    /// <param name="value"><see cref="HttpRequestMessage" /> 请求属性的值</param>
    public PropertyAttribute(string name, object? value)
        : this(name) =>
        Value = value;

    /// <summary>
    ///     <see cref="HttpRequestMessage" /> 请求属性键
    /// </summary>
    /// <remarks>该属性优先级低于 <see cref="AliasAs" /> 属性设置的值。</remarks>
    public string? Name { get; set; }

    /// <summary>
    ///     <see cref="HttpRequestMessage" /> 请求属性的值
    /// </summary>
    /// <remarks>当特性作用于参数时，表示默认值。</remarks>
    public object? Value { get; set; }

    /// <summary>
    ///     别名
    /// </summary>
    /// <remarks>
    ///     <para>特性用于参数时有效。</para>
    ///     <para>该属性优先级高于 <see cref="Name" /> 属性设置的值。</para>
    /// </remarks>
    public string? AliasAs { get; set; }

    /// <summary>
    ///     表示是否作为 <see cref="HttpRequestMessage" /> 请求属性的一项
    /// </summary>
    /// <remarks>
    ///     <para>当参数类型为对象类型时有效。</para>
    ///     <para>
    ///         该属性值为 <c>true</c> 时作为 <see cref="HttpRequestMessage" /> 请求属性的一项。否则将遍历对象类型的每一个公开属性作为
    ///         <see cref="HttpRequestMessage" /> 请求属性的项。默认值为：<c>true</c>。
    ///     </para>
    /// </remarks>
    public bool AsItem { get; set; } = true;
}