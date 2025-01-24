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

namespace ThingsGateway.Extensions;

/// <summary>
///     <see cref="IEnumerable" /> 拓展类
/// </summary>
internal static class IEnumerableExtensions
{
    /// <summary>
    ///     根据指定类型筛选 <see cref="IEnumerable" /> 的元素
    /// </summary>
    /// <param name="source">
    ///     <see cref="IEnumerable" />
    /// </param>
    /// <param name="resultType">筛选的结果类型</param>
    /// <returns>
    ///     <see cref="IEnumerable" />
    /// </returns>
    internal static IEnumerable OfType(this IEnumerable source, Type resultType)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(resultType);

        foreach (var obj in source)
        {
            if (resultType.IsInstanceOfType(obj))
            {
                yield return obj;
            }
        }
    }

    /// <summary>
    ///     合并两个集合
    /// </summary>
    /// <param name="first">
    ///     <see cref="IEnumerable{T}" />
    /// </param>
    /// <param name="second">
    ///     <see cref="IEnumerable{T}" />
    /// </param>
    /// <typeparam name="TSource">集合元素的类型</typeparam>
    /// <returns>
    ///     <see cref="IEnumerable{T}" />
    /// </returns>
    public static IEnumerable<TSource> ConcatIgnoreNull<TSource>(this IEnumerable<TSource>? first,
        IEnumerable<TSource>? second) =>
        (first ?? []).Concat(second ?? []);
}