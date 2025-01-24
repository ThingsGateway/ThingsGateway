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

namespace ThingsGateway.RescuePolicy;

/// <summary>
///     重试策略上下文
/// </summary>
/// <typeparam name="TResult">操作返回值类型</typeparam>
public sealed class RetryPolicyContext<TResult> : PolicyContextBase
{
    /// <summary>
    ///     <inheritdoc cref="RetryPolicyContext{TResult}" />
    /// </summary>
    internal RetryPolicyContext()
    {
    }

    /// <inheritdoc cref="System.Exception" />
    public System.Exception? Exception { get; internal set; }

    /// <summary>
    ///     操作返回值
    /// </summary>
    public TResult? Result { get; internal set; }

    /// <summary>
    ///     当前重试次数
    /// </summary>
    public int RetryCount { get; internal set; }

    /// <summary>
    ///     附加属性
    /// </summary>
    public IDictionary<object, object?>? Properties { get; set; }

    /// <summary>
    ///     递增上下文数据
    /// </summary>
    internal void Increment() => RetryCount++;
}