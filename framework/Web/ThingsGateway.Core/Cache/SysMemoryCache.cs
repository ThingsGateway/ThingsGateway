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
public class SysMemoryCache
{
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());

    /// <inheritdoc/>
    public T Get<T>(string key, bool mapster)
    {
        return _cache.TryGetValue<T>(key, out var value) ? mapster ? value.Adapt<T>() : value : default;
    }


    /// <inheritdoc/>
    public async Task<T> GetOrCreateAsync<T>(string key, Func<ICacheEntry, Task<T>> func, bool mapster) where T : class
    {
        var value = await _cache.GetOrCreateAsync(key, a =>
        {
            a.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            return func(a);
        });
        return mapster ? value.Adapt<T>() : value;
    }


    /// <inheritdoc/>
    public T GetOrCreate<T>(string key, Func<ICacheEntry, T> func, bool mapster) where T : class
    {
        var value = _cache.GetOrCreate(key, a =>
        {
            a.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            return func(a);
        });
        return mapster ? value.Adapt<T>() : value;
    }


    /// <inheritdoc/>
    public void Remove(object key)
    {
        _cache.Remove(key);
    }


    /// <inheritdoc/>
    public void Set<T>(object key, T value, bool mapster)
    {
        _cache.Set(key, mapster ? value.Adapt<T>() : value, TimeSpan.FromHours(1));
    }


    /// <inheritdoc/>
    public void Set<T>(object key, T value, TimeSpan offset, bool mapster)
    {
        _cache.Set(key, mapster ? value.Adapt<T>() : value, offset);
    }

}