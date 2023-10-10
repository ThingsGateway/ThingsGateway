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

//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System.Collections;
using System.Collections.Concurrent;

namespace ThingsGateway.Foundation.Core
{
    /// <summary>
    /// 一个简单的内存缓存
    /// </summary>
    public class MemoryCache<TKey, TValue> : IEnumerable<ICacheEntry<TKey, TValue>>, ICache<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, ICacheEntry<TKey, TValue>> m_pairs = new ConcurrentDictionary<TKey, ICacheEntry<TKey, TValue>>();
        private readonly Timer m_timer;

        /// <summary>
        ///  一个简单的内存缓存
        /// </summary>
        public MemoryCache()
        {
            this.m_timer = new Timer((o) =>
            {
                var list = new List<TKey>();
                foreach (var item in this.m_pairs)
                {
                    if (DateTime.Now - item.Value.UpdateTime > item.Value.Duration)
                    {
                        list.Add(item.Key);
                    }
                }
                foreach (var item in list)
                {
                    this.OnRemove(item, out _);
                }
            }, null, 0, 60 * 1000);
        }

        /// <summary>
        /// 当每个元素超时被移除时触发。
        /// </summary>
        public Action<ICacheEntry<TKey, TValue>> Remove { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="entity"></param>
        public bool AddCache(ICacheEntry<TKey, TValue> entity)
        {
            return this.m_pairs.TryAdd(entity.Key, entity);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public Task<bool> AddCacheAsync(ICacheEntry<TKey, TValue> entity)
        {
            return Task.Run(() =>
            {
                return this.AddCache(entity);
            });
        }

        /// <summary>
        /// 清空所有缓存
        /// </summary>
        public void ClearCache()
        {
            this.m_pairs.Clear();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public Task ClearCacheAsync()
        {
            return Task.Run(() =>
            {
                this.ClearCache();
            });
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsCache(TKey key)
        {
            if (this.m_pairs.TryGetValue(key, out var cache))
            {
                if (cache.Duration == TimeSpan.Zero)
                {
                    return true;
                }
                else
                {
                    if (DateTime.Now - cache.UpdateTime > cache.Duration)
                    {
                        this.OnRemove(key, out _);
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Task<bool> ContainsCacheAsync(TKey key)
        {
            return Task.Run(() =>
            {
                return this.ContainsCache(key);
            });
        }

        /// <summary>
        /// 获取缓存实体。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public ICacheEntry<TKey, TValue> GetCache(TKey key)
        {
            if (this.m_pairs.TryGetValue(key, out var cache))
            {
                if (cache.Duration == TimeSpan.Zero)
                {
                    return cache;
                }
                else
                {
                    if (DateTime.Now - cache.UpdateTime > cache.Duration)
                    {
                        this.OnRemove(key, out _);
                        return default;
                    }
                    else
                    {
                        return cache;
                    }
                }
            }
            return default;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Task<ICacheEntry<TKey, TValue>> GetCacheAsync(TKey key)
        {
            return Task.Run(() =>
            {
                return this.GetCache(key);
            });
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public IEnumerator<ICacheEntry<TKey, TValue>> GetEnumerator()
        {
            return this.m_pairs.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool RemoveCache(TKey key, out ICacheEntry<TKey, TValue> entity)
        {
            return this.OnRemove(key, out entity);
        }

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool RemoveCache(TKey key)
        {
            return this.OnRemove(key, out _);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Task<bool> RemoveCacheAsync(TKey key)
        {
            return Task.Run(() =>
            {
                return this.RemoveCache(key);
            });
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool SetCache(ICacheEntry<TKey, TValue> entity)
        {
            this.m_pairs.AddOrUpdate(entity.Key, entity, (k, v) =>
            {
                return entity;
            });
            return true;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public Task<bool> SetCacheAsync(ICacheEntry<TKey, TValue> entity)
        {
            return Task.Run(() =>
            {
                return this.SetCache(entity);
            });
        }

        /// <summary>
        /// 获取对应的值。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="update"></param>
        /// <returns></returns>
        public bool TryGetValue(TKey key, out TValue value, bool update = false)
        {
            if (this.m_pairs.TryGetValue(key, out var cache))
            {
                if (cache.Duration == TimeSpan.Zero)
                {
                    if (update)
                    {
                        cache.UpdateTime = DateTime.Now;
                    }
                    value = cache.Value;
                    return true;
                }
                else
                {
                    if (DateTime.Now - cache.UpdateTime > cache.Duration)
                    {
                        this.RemoveCache(key);
                        value = default;
                        return false;
                    }
                    else
                    {
                        if (update)
                        {
                            cache.UpdateTime = DateTime.Now;
                        }
                        value = cache.Value;
                        return true;
                    }
                }
            }
            value = default;
            return false;
        }

        private bool OnRemove(TKey key, out ICacheEntry<TKey, TValue> cache)
        {
            if (this.m_pairs.TryRemove(key, out cache))
            {
                try
                {
                    this.Remove?.Invoke(cache);
                    return true;
                }
                catch
                {
                }
            }

            return false;
        }
    }
}