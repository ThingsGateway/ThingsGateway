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
///     HTTP 声明式请求标头特性
/// </summary>
/// <remarks>支持多次指定。</remarks>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface | AttributeTargets.Parameter,
    AllowMultiple = true)]
public sealed class HeaderAttribute : Attribute
{
    /// <summary>
    ///     <inheritdoc cref="HeaderAttribute" />
    /// </summary>
    /// <remarks>特性作用于参数时有效。</remarks>
    public HeaderAttribute()
    {
    }

    /// <summary>
    ///     <inheritdoc cref="HeaderAttribute" />
    /// </summary>
    /// <remarks>
    ///     <para>当特性作用于方法或接口时，则表示移除指定请求标头操作。</para>
    ///     <para>当特性作用于参数时，则表示添加请求标头，同时设置请求标头键为 <c>name</c> 的值。</para>
    /// </remarks>
    /// <param name="name">请求标头键</param>
    public HeaderAttribute(string name)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
    }

    /// <summary>
    ///     <inheritdoc cref="HeaderAttribute" />
    /// </summary>
    /// <param name="name">请求标头键</param>
    /// <param name="value">请求标头的值</param>
    public HeaderAttribute(string name, object? value)
        : this(name) =>
        Value = value;

    /// <summary>
    ///     请求标头键
    /// </summary>
    /// <remarks>该属性优先级低于 <see cref="AliasAs" /> 属性设置的值。</remarks>
    public string? Name { get; set; }



    /// <summary>
    ///     请求标头的值
    /// </summary>
    /// <remarks>当特性作用于参数时，表示默认值。</remarks>
    public object? Value
    {
        get
        {
            return _value;
        }
        set
        {
            _value = value;
            HasSetValue = true;
        }
    }
    private object? _value { get; set; }

    /// <summary>
    ///     别名
    /// </summary>
    /// <remarks>
    ///     <para>特性用于参数时有效。</para>
    ///     <para>该属性优先级高于 <see cref="Name" /> 属性设置的值。</para>
    /// </remarks>
    public string? AliasAs { get; set; }

    /// <summary>
    ///     是否转义
    /// </summary>
    public bool Escape { get; set; }

    /// <summary>
    ///     是否替换已存在的请求标头。默认值为 <c>false</c>
    /// </summary>
    public bool Replace { get; set; }

    /// <summary>
    ///     是否设置了值
    /// </summary>
    internal bool HasSetValue { get; private set; }
}