//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using NewLife.Caching.Models;

namespace ThingsGateway.Cache
{
    /// <summary>
    /// Set
    /// </summary>
    public partial class SimpleRedis : ISimpleRedis
    {
        /// <inheritdoc />
        public RedisSet<T> GetRedisSet<T>(string key)
        {
            return redisConnection.GetSet<T>(key) as RedisSet<T>;
        }

        /// <inheritdoc />
        public void SetAdd<T>(string key, T value)
        {
            var set = GetRedisSet<T>(key);
            set.Add(value);
        }

        /// <inheritdoc />
        public int SetAddList<T>(string key, T[] value)
        {
            var set = GetRedisSet<T>(key);
            return set.SAdd(value);
        }

        /// <inheritdoc />
        public bool SetDel<T>(string key, T value)
        {
            var set = GetRedisSet<T>(key);
            return set.Remove(value);
        }

        /// <inheritdoc />
        public int SetDelRange<T>(string key, T[] value)
        {
            var set = GetRedisSet<T>(key);
            return set.SDel(value);
        }

        /// <inheritdoc />
        public T[] SetGetAll<T>(string key)
        {
            var set = GetRedisSet<T>(key);
            return set.GetAll();
        }

        /// <inheritdoc />
        public T[] SetRandom<T>(string key, int count)
        {
            var set = GetRedisSet<T>(key);
            return set.RandomGet(count);
        }

        /// <inheritdoc />
        public T[] SetPop<T>(string key, int count)
        {
            var set = GetRedisSet<T>(key);
            return set.Pop(count);
        }

        /// <inheritdoc />
        public List<string> Search<T>(string key, SearchModel model)
        {
            var set = GetRedisSet<T>(key);
            return set.Search(model).ToList();
        }

        /// <inheritdoc />
        public List<string> Search<T>(string key, string pattern, int count)
        {
            var set = GetRedisSet<T>(key);
            return set.Search(pattern, count).ToList();
        }

        /// <inheritdoc />
        public bool SetContains<T>(string key, T value)
        {
            var set = GetRedisSet<T>(key);
            return set.Contains(value);
        }

        /// <inheritdoc />
        public void SetClear<T>(string key)
        {
            var set = GetRedisSet<T>(key);
            set.Clear();
        }

        /// <inheritdoc />
        public void SetCopyTo<T>(string key, T[] array, int arrayIndex)
        {
            var set = GetRedisSet<T>(key);
            set.CopyTo(array, arrayIndex);
        }
    }
}