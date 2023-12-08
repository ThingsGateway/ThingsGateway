#region copyright

//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

#endregion

using Mapster;

using Microsoft.Extensions.Caching.Memory;

using ICacheEntry = Microsoft.Extensions.Caching.Memory.ICacheEntry;

namespace ThingsGateway.Core;

/// <summary>
/// 系统内存缓存
/// </summary>
public class MemoryCache
{
    public static MemoryCache Instance { get; private set; } = new();
    private const string intervalStr = "---___---";
    private readonly Microsoft.Extensions.Caching.Memory.MemoryCache _memoryCache = new(new MemoryCacheOptions());
    private readonly Microsoft.Extensions.Caching.Memory.MemoryCache _prefixmemoryCache = new(new MemoryCacheOptions());

    /// <inheritdoc/>
    public T Get<T>(string prefixKey, string key, bool mapster)
    {
        return _memoryCache.TryGetValue<T>($"{prefixKey}{intervalStr}{key}", out var value) ? mapster ? value.Adapt<T>() : value : default;
    }

    /// <inheritdoc/>
    public T Get<T>(string key, bool mapster)
    {
        return Get<T>(string.Empty, key, mapster);
    }

    /// <inheritdoc/>
    public T GetOrCreate<T>(string prefixKey, string key, Func<ICacheEntry, T> func, bool mapster) where T : class
    {
        var value = _memoryCache.GetOrCreate($"{prefixKey}{intervalStr}{key}", a =>
        {
            a.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            return func(a);
        });
        _prefixmemoryCache.TryGetValue<HashSet<string>>(prefixKey, out var result);
        result ??= new();
        result.Add($"{prefixKey}{intervalStr}{key}");
        _prefixmemoryCache.Set(prefixKey, result, TimeSpan.FromHours(1));
        return mapster ? value.Adapt<T>() : value;
    }

    /// <inheritdoc/>
    public T GetOrCreate<T>(string key, Func<ICacheEntry, T> func, bool mapster) where T : class
    {
        return GetOrCreate(string.Empty, key, func, mapster);
    }

    /// <inheritdoc/>
    public async Task<T> GetOrCreateAsync<T>(string prefixKey, string key, Func<ICacheEntry, Task<T>> func, bool mapster) where T : class
    {
        var value = await _memoryCache.GetOrCreateAsync($"{prefixKey}{intervalStr}{key}", a =>
        {
            a.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            return func(a);
        });
        _prefixmemoryCache.TryGetValue<HashSet<string>>(prefixKey, out var result);
        result ??= new();
        result.Add($"{prefixKey}{intervalStr}{key}");
        _prefixmemoryCache.Set(prefixKey, result, TimeSpan.FromHours(1));
        return mapster ? value.Adapt<T>() : value;
    }

    /// <inheritdoc/>
    public async Task<T> GetOrCreateAsync<T>(string key, Func<ICacheEntry, Task<T>> func, bool mapster) where T : class
    {
        return await GetOrCreateAsync(string.Empty, key, func, mapster);
    }

    /// <inheritdoc/>
    public void Remove(string prefixKey, string key)
    {
        _memoryCache.Remove($"{prefixKey}{intervalStr}{key}");

        _prefixmemoryCache.TryGetValue<HashSet<string>>(prefixKey, out var result);
        result ??= new();
        result.Remove($"{prefixKey}{intervalStr}{key}");
        _prefixmemoryCache.Set(prefixKey, result, TimeSpan.FromHours(1));
    }

    /// <inheritdoc/>
    public void Remove(string key)
    {
        Remove(string.Empty, key);
    }

    public void RemoveByPrefix(string prefixKey)
    {
        _prefixmemoryCache.TryGetValue<HashSet<string>>(prefixKey, out var result);
        result ??= new();
        foreach (var key in result)
        {
            _memoryCache.Remove(key);
        }
        _prefixmemoryCache.Remove(prefixKey);
    }

    /// <inheritdoc/>
    public void Set<T>(string key, T value, bool mapster)
    {
        var prefixKey = string.Empty;
        _memoryCache.Set($"{prefixKey}{intervalStr}{key}", mapster ? value.Adapt<T>() : value, TimeSpan.FromHours(1));
        _prefixmemoryCache.TryGetValue<HashSet<string>>(prefixKey, out var result);
        result ??= new();
        result.Add($"{prefixKey}{intervalStr}{key}");
        _prefixmemoryCache.Set(prefixKey, result, TimeSpan.FromHours(1));
    }

    /// <inheritdoc/>
    public void Set<T>(string key, T value, TimeSpan offset, bool mapster)
    {
        _memoryCache.Set($"{string.Empty}{intervalStr}{key}", mapster ? value.Adapt<T>() : value, offset);
        _prefixmemoryCache.TryGetValue<HashSet<string>>(string.Empty, out var result);
        result ??= new();
        result.Add($"{string.Empty}{intervalStr}{key}");
        _prefixmemoryCache.Set(string.Empty, result, offset);
    }
}