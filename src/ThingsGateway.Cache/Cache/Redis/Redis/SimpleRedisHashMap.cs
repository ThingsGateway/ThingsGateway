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
    /// HashMap
    /// </summary>
    public partial class SimpleRedis : ISimpleRedis
    {
        /// <inheritdoc />
        public RedisHash<string, T> GetHashMap<T>(string key)
        {
            return redisConnection.GetDictionary<T>(key) as RedisHash<string, T>;
        }

        /// <inheritdoc />
        public bool HashSet<T>(string key, Dictionary<string, T> dic)
        {
            var hash = GetHashMap<T>(key);
            return hash.HMSet(dic);
        }

        /// <inheritdoc />
        public void HashAdd<T>(string key, string hashKey, T value)
        {
            var hash = GetHashMap<T>(key);
            hash.Add(hashKey, value);
        }

        /// <inheritdoc />
        public List<T> HashGet<T>(string key, params string[] fields)
        {
            var hash = GetHashMap<T>(key);
            var result = hash.HMGet(fields);
            return result.ToList();
        }

        /// <inheritdoc />
        public T HashGetOne<T>(string key, string field)
        {
            var hash = GetHashMap<T>(key);
            var result = hash.HMGet(new string[] { field });
            return result[0];
        }

        /// <inheritdoc />
        public IDictionary<string, T> HashGetAll<T>(string key)
        {
            var hash = GetHashMap<T>(key);
            return hash.GetAll();
        }

        /// <inheritdoc />
        public int HashDel<T>(string key, params string[] fields)
        {
            var hash = GetHashMap<T>(key);
            return hash.HDel(fields);
        }

        /// <inheritdoc />
        public List<KeyValuePair<string, T>> HashSearch<T>(string key, SearchModel searchModel)
        {
            var hash = GetHashMap<T>(key);
            return hash.Search(searchModel).ToList();
        }

        /// <inheritdoc />
        public List<KeyValuePair<string, T>> HashSearch<T>(string key, string pattern, int count)
        {
            var hash = GetHashMap<T>(key);
            return hash.Search(pattern, count).ToList();
        }
    }
}