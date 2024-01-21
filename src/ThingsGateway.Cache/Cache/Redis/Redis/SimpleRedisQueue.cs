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

using NewLife.Caching.Queues;

namespace ThingsGateway.Cache
{
    /// <summary>
    /// 普通队列
    /// </summary>
    public partial class SimpleRedis : ISimpleRedis
    {
        /// <inheritdoc />
        public RedisQueue<T> GetRedisQueue<T>(string key)
        {
            return redisConnection.GetQueue<T>(key) as RedisQueue<T>;
        }

        /// <inheritdoc />
        public int AddQueue<T>(string key, T[] value)
        {
            var queue = GetRedisQueue<T>(key);
            return queue.Add(value);
        }

        /// <inheritdoc />
        public int AddQueue<T>(string key, T value)
        {
            var queue = GetRedisQueue<T>(key);
            return queue.Add(value);
        }

        /// <inheritdoc />
        public List<T> GetQueue<T>(string key, int Count = 1)
        {
            var queue = GetRedisQueue<T>(key);
            var result = queue.Take(Count).ToList();

            return result;
        }

        /// <inheritdoc />
        public T GetQueueOne<T>(string key, int timeout = 1)
        {
            var queue = GetRedisQueue<T>(key);
            return queue.TakeOne(timeout);
        }

        /// <inheritdoc />
        public Task<T> GetQueueOneAsync<T>(string key, int timeout = 1)
        {
            var queue = GetRedisQueue<T>(key);
            return queue.TakeOneAsync(1);
        }
    }
}