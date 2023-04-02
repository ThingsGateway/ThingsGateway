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