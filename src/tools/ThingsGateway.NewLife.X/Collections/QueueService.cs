//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Collections.Concurrent;

using ThingsGateway.NewLife.X.Caching;

namespace ThingsGateway.NewLife.X.Collections;

/// <summary>主动式消息服务</summary>
/// <typeparam name="T">数据类型</typeparam>
public interface IQueueService<T>
{
    /// <summary>消费消息</summary>
    /// <param name="clientId">客户标识</param>
    /// <param name="topic">主题</param>
    /// <param name="count">要拉取的消息数</param>
    /// <returns></returns>
    T[] Consume(String clientId, String topic, Int32 count);

    /// <summary>发布消息</summary>
    /// <param name="topic">主题</param>
    /// <param name="value">消息</param>
    /// <returns></returns>
    Int32 Public(String topic, T value);

    /// <summary>订阅</summary>
    /// <param name="clientId">客户标识</param>
    /// <param name="topic">主题</param>
    Boolean Subscribe(String clientId, String topic);

    /// <summary>取消订阅</summary>
    /// <param name="clientId">客户标识</param>
    /// <param name="topic">主题</param>
    Boolean UnSubscribe(String clientId, String topic);
}

/// <summary>轻量级主动式消息服务</summary>
/// <typeparam name="T">数据类型</typeparam>
public class QueueService<T> : IQueueService<T>
{
    #region 属性

    /// <summary>每个主题的所有订阅者</summary>
    private readonly ConcurrentDictionary<String, ConcurrentDictionary<String, IProducerConsumer<T>>> _topics = new();

    /// <summary>数据存储</summary>
    public ICache Cache { get; set; } = MemoryCache.Instance;

    #endregion 属性

    #region 方法

    /// <summary>消费消息</summary>
    /// <param name="clientId">客户标识</param>
    /// <param name="topic">主题</param>
    /// <param name="count"></param>
    /// <returns></returns>
    public T[] Consume(String clientId, String topic, Int32 count)
    {
        if (_topics.TryGetValue(topic, out var clients))
        {
            if (clients.TryGetValue(clientId, out var queue))
            {
                return queue.Take(count).ToArray();
            }
        }

        return Array.Empty<T>();
    }

    /// <summary>发布消息</summary>
    /// <param name="topic">主题</param>
    /// <param name="value">消息</param>
    /// <returns></returns>
    public Int32 Public(String topic, T value)
    {
        var rs = 0;
        if (_topics.TryGetValue(topic, out var clients))
        {
            // 向每个订阅者推送
            foreach (var item in clients)
            {
                var queue = item.Value;
                rs += queue.Add([value]);
            }
        }

        return rs;
    }

    /// <summary>订阅</summary>
    /// <param name="clientId">客户标识</param>
    /// <param name="topic">主题</param>
    public Boolean Subscribe(String clientId, String topic)
    {
        var dic = _topics.GetOrAdd(topic, k => new ConcurrentDictionary<String, IProducerConsumer<T>>());
        if (dic.ContainsKey(clientId)) return false;

        // 创建队列
        var queue = Cache.GetQueue<T>($"{topic}_{clientId}");
        return dic.TryAdd(clientId, queue);
    }

    /// <summary>取消订阅</summary>
    /// <param name="clientId">客户标识</param>
    /// <param name="topic">主题</param>
    public Boolean UnSubscribe(String clientId, String topic)
    {
        if (_topics.TryGetValue(topic, out var clients))
        {
            return clients.TryRemove(clientId, out _);
        }

        return false;
    }

    #endregion 方法
}
