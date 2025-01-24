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

using System.Collections.Concurrent;

namespace ThingsGateway.Extensions;

/// <summary>
///     <see cref="ConcurrentDictionary{TKey, TValue}" /> 拓展类
/// </summary>
internal static class ConcurrentDictionaryExtensions
{
    /// <summary>
    ///     根据字典键更新对应的值
    /// </summary>
    /// <typeparam name="TKey">字典键类型</typeparam>
    /// <typeparam name="TValue">字典值类型</typeparam>
    /// <param name="dictionary">
    ///     <see cref="ConcurrentDictionary{TKey, TValue}" />
    /// </param>
    /// <param name="key">
    ///     <typeparamref name="TKey" />
    /// </param>
    /// <param name="updateFactory">自定义更新委托</param>
    /// <param name="value">
    ///     <typeparamref name="TValue" />
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    internal static bool TryUpdate<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary
        , TKey key
        , Func<TValue, TValue> updateFactory
        , out TValue? value)
        where TKey : notnull
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(updateFactory);

        // 查找字典值
        if (!dictionary.TryGetValue(key, out var oldValue))
        {
            value = default;
            return false;
        }

        // 调用自定义更新委托
        var updatedValue = updateFactory(oldValue);

        // 更新字典值
        var result = dictionary.TryUpdate(key, updatedValue, oldValue);
        value = result ? updatedValue : oldValue;

        return result;
    }
}