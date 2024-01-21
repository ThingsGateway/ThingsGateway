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

using Microsoft.Extensions.Configuration;

using NewLife.Caching.Models;

namespace ThingsGateway.Cache
{
    /// <summary>
    /// 基础
    /// </summary>
    public partial class SimpleRedis : ISimpleRedis
    {
        /// <summary>
        /// Redis实例
        /// </summary>
        public volatile FullRedis redisConnection;

        private readonly object redisConnectionLock = new object();

        /// <summary>
        /// 配置文件注入
        /// </summary>
        /// <param name="configuration"></param>
        /// <exception cref="ArgumentException"></exception>
        public SimpleRedis(IConfiguration configuration)
        {
            string redisConfiguration = configuration["ConnectionStrings:Redis"];//获取连接字符串
            if (string.IsNullOrWhiteSpace(redisConfiguration))
            {
                throw new ArgumentException("redis config [ConnectionStrings: Redis] is empty", nameof(redisConfiguration));
            }
            this.redisConnection = GetFullRedis(redisConfiguration);
        }

        /// <summary>
        /// 通过字符串注入
        /// </summary>
        /// <param name="redisConfiguration">连接字符串</param>
        /// <exception cref="ArgumentException"></exception>
        public SimpleRedis(string redisConfiguration)
        {
            if (string.IsNullOrWhiteSpace(redisConfiguration))
            {
                throw new ArgumentException("redis config is empty", nameof(redisConfiguration));
            }
            this.redisConnection = GetFullRedis(redisConfiguration);
        }

        /// <summary>
        /// 核心代码，获取连接实例
        /// 通过双if 夹lock的方式，实现单例模式
        /// </summary>
        /// <returns></returns>
        private FullRedis GetFullRedis(string redisConnenctionString)
        {
            //如果已经连接实例，直接返回
            if (this.redisConnection != null)
            {
                return this.redisConnection;
            }
            //加锁，防止异步编程中，出现单例无效的问题
            lock (redisConnectionLock)
            {
                if (this.redisConnection != null)
                {
                    //释放redis连接
                    this.redisConnection.Dispose();
                }
                try
                {
                    this.redisConnection = FullRedis.Create(redisConnenctionString);
                }
                catch (Exception)
                {
                    throw new Exception(CacheStringConst.RedisNotStarted);
                }
            }
            return this.redisConnection;
        }

        #region 普通方法

        /// <inheritdoc />
        public FullRedis GetFullRedis()
        {
            return redisConnection;
        }

        /// <inheritdoc />
        public List<string> AllKeys()
        {
            return redisConnection.Keys.ToList();
        }

        /// <inheritdoc />
        public bool ContainsKey(string key)
        {
            return redisConnection.ContainsKey(key);
        }

        /// <inheritdoc />
        public List<string> Search(SearchModel model)
        {
            return redisConnection.Search(model).ToList();
        }

        /// <inheritdoc />
        public long DelByPattern(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                return 0;
            //pattern = Regex.Replace(pattern, @"\{*.\}", "(.*)");
            //var keys = fullRedis.Search(new SearchModel { Pattern = pattern });
            var keys = redisConnection.Keys?.Where(k => k.StartsWith(pattern));
            //var keys = GetAllKeys().Where(k => k.StartsWith(pattern));
            if (keys != null && keys.Any())
                return redisConnection.Remove(keys.ToArray());
            return 0;
        }

        /// <inheritdoc />
        public void Clear()
        {
            redisConnection.Clear();
        }

        /// <inheritdoc />
        public void SetExpire(string key, TimeSpan timeSpan)
        {
            redisConnection.SetExpire(key, timeSpan);
        }

        /// <inheritdoc />
        public int RemoveByKey(string key, int count)
        {
            var result = 0;
            var keys = redisConnection.Search(key, count).ToList();
            foreach (var k in keys)
                result += redisConnection.Remove(k);
            return result;
        }

        /// <inheritdoc />
        public int RemoveAllByKey(string key, int count = 999)
        {
            var result = 0;
            while (true)
            {
                var keyList = redisConnection.Search(key, count).ToList();
                if (keyList.Count > 0)
                {
                    foreach (var k in keyList)
                        result += redisConnection.Remove(k);
                }
                else
                {
                    break;
                }
            }
            return result;
        }

        /// <inheritdoc />
        public bool Set<TEntity>(string key, TEntity value, TimeSpan cacheTime)
        {
            return redisConnection.Set(key, value, cacheTime);
        }

        /// <inheritdoc />
        public bool Set<TEntity>(string key, TEntity value, int expire = -1)
        {
            return redisConnection.Set(key, value, expire);
        }

        /// <inheritdoc />
        public TEntity Get<TEntity>(string key)
        {
            return redisConnection.Get<TEntity>(key);
        }

        /// <inheritdoc />
        public void Remove(string key)
        {
            redisConnection.Remove(key);
        }

        /// <inheritdoc />
        public bool Rename(string key, string newKey, bool overwrire = true)
        {
            return redisConnection.Rename(key, newKey, overwrire);
        }

        #endregion 普通方法
    }
}