// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

using System.Collections.Concurrent;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 内存推送事件服务
/// </summary>
/// <typeparam name="TEntry"></typeparam>
public class EventService<TEntry> : IEventService<TEntry>
{
    private ConcurrentDictionary<string, Func<TEntry, Task>> Cache { get; } = new();

    public void Dispose()
    {
        Cache.Clear();
    }

    /// <inheritdoc/>
    public async Task Publish(string key, TEntry payload)
    {
        if (Cache.TryGetValue(key, out var func))
        {
            await func(payload).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public void Subscribe(string key, Func<TEntry, Task> callback)
    {
        Cache.TryAdd(key, callback);
    }

    /// <inheritdoc/>
    public void UnSubscribe(string key)
    {
        Cache.Remove(key);
    }
}
