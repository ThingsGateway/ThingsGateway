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

using System.Diagnostics.CodeAnalysis;

namespace ThingsGateway.Extensions;

/// <summary>
///     <see cref="ICollection{T}" /> 拓展类
/// </summary>
internal static class ICollectionExtensions
{
    /// <summary>
    ///     判断集合是否为空
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="collection">
    ///     <see cref="ICollection{T}" />
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    internal static bool IsNullOrEmpty<T>([NotNullWhen(false)] this ICollection<T>? collection) =>
        collection is null
        || collection.Count == 0;
}