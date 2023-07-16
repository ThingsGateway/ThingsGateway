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
    /// 系统缓存服务,代替Redis,存取都会进行Mapper拷贝操作
    /// </summary>
    public class SysCacheService : ISingleton
    {
        private readonly ICache _cache;
        private string _symbol = "tgcache_tgcache";
        /// <summary>
        /// <inheritdoc cref="SysCacheService"/>
        /// </summary>
        /// <param name="cache"></param>
        public SysCacheService(ICache cache)
        {
            _cache = cache;
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
            return _cache.Get<T>(str).Adapt<T>();
        }
        /// <summary>
        /// 获取验证ID
        /// </summary>
        /// <returns></returns>
        public List<SysVerificat> GetAllOpenApiVerificatId()
        {
            var ids = GetKeyByPrefixKey(CacheConst.Cache_OpenApiUserId).Select(it => it.ToLong()).ToList();
            if (ids == null || ids.Count == 0)
            {
                var syss = DbContext.Db.QueryableWithAttr<OpenApiUser>().Select(it => it.Id).ToList();
                foreach (var item in syss)
                {
                    Set(CacheConst.Cache_OpenApiUserId, item.ToString(), item);
                }
                ids = syss;
            }
            List<SysVerificat> verificats = new();
            foreach (var item in ids)
            {
                var data = GetOpenApiVerificatId(item);
                if (data != null)
                {
                    SysVerificat sysVerificat = new();
                    sysVerificat.Id = item;
                    sysVerificat.VerificatInfos = data;
                    verificats.Add(sysVerificat);
                }
            }
            return verificats;
        }
        /// <summary>
        /// 获取验证ID
        /// </summary>
        /// <returns></returns>
        public List<SysVerificat> GetAllVerificatId()
        {
            var ids = GetKeyByPrefixKey(CacheConst.Cache_UserId).Select(it => it.ToLong()).ToList();
            if (ids == null || ids.Count == 0)
            {
                var syss = DbContext.Db.QueryableWithAttr<SysUser>().Select(it => it.Id).ToList();
                foreach (var item in syss)
                {
                    Set(CacheConst.Cache_UserId, item.ToString(), item);
                }
                ids = syss;
            }
            List<SysVerificat> verificats = new();
            foreach (var item in ids)
            {
                var data = GetVerificatId(item);
                if (data != null)
                {
                    SysVerificat sysVerificat = new();
                    sysVerificat.Id = item;
                    sysVerificat.VerificatInfos = data;
                    verificats.Add(sysVerificat);
                }
            }
            return verificats;
        }

        /// <summary>
        /// 根据键名前缀获取缓存
        /// </summary>
        /// <param name="prefixKey">键名前缀</param>
        /// <returns></returns>
        public IDictionary<string, T> GetByPrefixKey<T>(string prefixKey)
        {
            var delKeys = _cache.Keys.Where(u => u.Split(_symbol).FirstOrDefault() == prefixKey).ToArray();
            var values = _cache.GetAll<T>(delKeys);
            return values;
        }

        /// <summary>
        /// 根据键名前缀获取全部中间key
        /// </summary>
        /// <param name="prefixKey">键名前缀</param>
        /// <returns></returns>
        public List<string> GetKeyByPrefixKey(string prefixKey)
        {
            var delKeys = _cache.Keys.SelectMany(u =>
            {
                List<string> strings = new();
                var data = u.Split(_symbol);
                if (data.FirstOrDefault() == prefixKey)
                {
                    strings.Add(data.LastOrDefault());
                }
                return strings;
            }
        ).ToList();
            return delKeys;
        }
        /// <summary>
        /// 获取验证ID
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public List<VerificatInfo> GetOpenApiVerificatId(long userid)
        {
            var data = Get<List<VerificatInfo>>(CacheConst.Cache_OpenApiUserVerificat, userid.ToString());
            if (data != null)
            {
                var infos = data.Where(it => it.VerificatTimeout > DateTime.UtcNow).ToList();//去掉登录超时的
                if (infos.Count != data.Count)
                    SetOpenApiVerificatId(userid, infos);
                return infos;
            }
            else
            {
                var sys = DbContext.Db.QueryableWithAttr<SysVerificat>().Where(it => it.Id == userid).First();
                if (sys != null)
                {
                    var infos = sys.VerificatInfos.Where(it => it.VerificatTimeout > DateTime.UtcNow).ToList();//去掉登录超时的
                    SetOpenApiVerificatId(userid, infos);
                    return infos;
                }
                return null;
            }
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        public T GetOrAdd<T>(string prefixKey, string key, Func<string, T> func, int expire = -1)
        {
            var str = prefixKey + _symbol + key;
            return _cache.GetOrAdd<T>(str, func, expire);
        }

        /// <summary>
        /// 获取验证ID
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public List<VerificatInfo> GetVerificatId(long userid)
        {
            var data = Get<List<VerificatInfo>>(CacheConst.Cache_UserVerificat, userid.ToString());
            if (data != null)
            {
                var infos = data.Where(it => it.VerificatTimeout > DateTime.UtcNow).ToList();//去掉登录超时的
                if (infos.Count != data.Count)
                    SetVerificatId(userid, infos);
                return infos;
            }
            else
            {
                var sys = DbContext.Db.QueryableWithAttr<SysVerificat>().Where(it => it.Id == userid).First();
                if (sys != null)
                {
                    var infos = sys.VerificatInfos.Where(it => it.VerificatTimeout > DateTime.UtcNow).ToList();//去掉登录超时的
                    SetVerificatId(userid, infos);
                    return infos;
                }
                return null;
            }
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
            _cache.Set(str, value.Adapt<object>());
        }

        /// <summary>
        /// 增加缓存并设置过期时间
        /// </summary>
        public void Set(string prefixKey, string key, object value, TimeSpan expire)
        {
            var str = prefixKey + _symbol + key;
            _cache.Set(str, value.Adapt<object>(), expire);
        }
        /// <summary>
        /// 设置验证ID
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="values"></param>
        public void SetOpenApiVerificatId(long userid, List<VerificatInfo> values)
        {
            SysVerificat sysverificat = new();
            sysverificat.Id = userid;
            sysverificat.VerificatInfos = values;
            if (DbContext.Db.Storageable(sysverificat).ExecuteCommand() > 0)
            {
                Set(CacheConst.Cache_OpenApiUserVerificat, userid.ToString(), values);
            }
        }
        /// <summary>
        /// 设置验证ID
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="values"></param>
        public void SetVerificatId(long userid, List<VerificatInfo> values)
        {
            SysVerificat sysverificat = new();
            sysverificat.Id = userid;
            sysverificat.VerificatInfos = values;
            if (DbContext.Db.Storageable(sysverificat).ExecuteCommand() > 0)
            {
                Set(CacheConst.Cache_UserVerificat, userid.ToString(), values);
            }
        }
    }
}