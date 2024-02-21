//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using NewLife.Caching.Queues;

namespace ThingsGateway.Cache
{
    /// <summary>
    /// 可信队列
    /// </summary>
    public partial class SimpleRedis : ISimpleRedis
    {
        /// <inheritdoc  />
        public RedisReliableQueue<T> GetRedisReliableQueue<T>(string key)
        {
            var queue = redisConnection.GetReliableQueue<T>(key);
            return queue;
        }

        /// <inheritdoc  />
        public int RollbackAllAck(string key, int retryInterval = 60)
        {
            var queue = GetRedisReliableQueue<string>(key);
            queue.RetryInterval = retryInterval;
            return queue.RollbackAllAck();
        }

        /// <inheritdoc  />
        public T ReliableTakeOne<T>(string key)
        {
            var queue = GetRedisReliableQueue<T>(key);
            return queue.TakeOne(1);
        }

        /// <inheritdoc  />
        public Task<T> ReliableTakeOneAsync<T>(string key)
        {
            var queue = GetRedisReliableQueue<T>(key);
            return queue.TakeOneAsync(1);
        }

        /// <inheritdoc  />
        public List<T> ReliableTake<T>(string key, int count)
        {
            var queue = GetRedisReliableQueue<T>(key);
            return queue.Take(count).ToList();
        }

        /// <inheritdoc  />
        public int AddReliableQueueList<T>(string key, List<T> value)
        {
            var queue = redisConnection.GetReliableQueue<T>(key);
            var count = queue.Count;
            var result = queue.Add(value.ToArray());
            return result - count;
        }

        /// <inheritdoc  />
        public int AddReliableQueue<T>(string key, T value)
        {
            var queue = redisConnection.GetReliableQueue<T>(key);
            var count = queue.Count;
            var result = queue.Add(value);
            return result - count;
        }
    }
}