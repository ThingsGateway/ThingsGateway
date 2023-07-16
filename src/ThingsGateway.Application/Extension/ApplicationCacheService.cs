#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/dotnetchina/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using NewLife.Caching;

namespace ThingsGateway.Application
{
    /// <summary>
    /// Application缓存服务,只用于固定项
    /// </summary>
    public class ApplicationCacheService
    {
        private readonly ICache _cache;
        private string _symbol = "TBCache_TBCache";
        /// <summary>
        /// <inheritdoc cref="ApplicationCacheService"/>
        /// </summary>
        public ApplicationCacheService()
        {
            _cache = new MemoryCache();
        }

        /// <summary>
        /// 检查缓存是否存在
        /// </summary>
        public bool ExistKey(string prefixKey, string key)
        {
            var str = prefixKey + _symbol + key;
            return _cache.ContainsKey(str);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        public T Get<T>(string prefixKey, string key)
        {
            var str = prefixKey + _symbol + key;
            return _cache.Get<T>(str);
        }

        /// <summary>
        /// 根据键名前缀获取缓存
        /// </summary>
        public IDictionary<string, T> GetByPrefixKey<T>(string prefixKey)
        {
            var delKeys = _cache.Keys.Where(u => u.Split(_symbol).FirstOrDefault() == prefixKey).ToArray();
            var values = _cache.GetAll<T>(delKeys);
            return values;
        }

        /// <summary>
        /// 根据键名前缀获取全部中间key
        /// </summary>
        public List<long> GetKeyByPrefixKey(string prefixKey)
        {
            var delKeys = _cache.Keys.SelectMany(u =>
            {
                List<long> longs = new();
                var data = u.Split(_symbol);
                if (data.FirstOrDefault() == prefixKey)
                {
                    longs.Add(data.LastOrDefault().ToLong());
                }
                return longs;
            }
        ).ToList();
            return delKeys;
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        public T GetOrAdd<T>(string prefixKey, string key, Func<string, T> func, int expire = -1)
        {
            var str = prefixKey + _symbol + key;
            return _cache.GetOrAdd(str, func, expire);
        }

        /// <summary>
        /// 删除缓存
        /// </summary>
        public void Remove(string prefixKey, string key)
        {
            var str = prefixKey + _symbol + key;
            _cache.Remove(str);
        }

        /// <summary>
        /// 根据键名前缀删除缓存
        /// </summary>
        /// <param name="prefixKey">键名前缀</param>
        /// <returns></returns>
        public int RemoveByPrefixKey(string prefixKey)
        {
            var delKeys = _cache.Keys.Where(u => u.Split(_symbol).FirstOrDefault() == prefixKey).ToArray();
            if (!delKeys.Any()) return 0;
            return _cache.Remove(delKeys);
        }

        /// <summary>
        /// 增加缓存
        /// </summary>
        public void Set(string prefixKey, string key, object value)
        {
            var str = prefixKey + _symbol + key;
            _cache.Set(str, value);
        }

        /// <summary>
        /// 增加缓存并设置过期时间
        /// </summary>
        public void Set(string prefixKey, string key, object value, TimeSpan expire)
        {
            var str = prefixKey + _symbol + key;
            _cache.Set(str, value, expire);
        }
    }
}