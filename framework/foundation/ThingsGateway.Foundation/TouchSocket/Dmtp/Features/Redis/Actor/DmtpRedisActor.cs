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

using ThingsGateway.Foundation.Resources;

namespace ThingsGateway.Foundation.Dmtp.Redis
{
    /// <summary>
    /// DmtpRedisActor
    /// </summary>
    public class DmtpRedisActor : IDmtpRedisActor
    {
        /// <summary>
        /// DmtpRedisActor
        /// </summary>
        /// <param name="dmtpActor"></param>
        public DmtpRedisActor(IDmtpActor dmtpActor)
        {
            this.DmtpActor = dmtpActor;
        }

        /// <inheritdoc/>
        public BytesConverter Converter { get; set; }

        /// <inheritdoc/>
        public IDmtpActor DmtpActor { get; }

        /// <inheritdoc/>
        public ICache<string, byte[]> ICache { get; set; }

        /// <inheritdoc/>
        public int Timeout { get; set; } = 30 * 1000;

        /// <inheritdoc/>
        public bool Add<TValue>(string key, TValue value, int duration = 60000)
        {
            var cache = new CacheEntry<string, byte[]>(key)
            {
                Duration = TimeSpan.FromSeconds(duration)
            };
            if (!(value is byte[]))
            {
                cache.Value = this.Converter.ConvertTo(value);
            }
            return this.AddCache(cache);
        }

        /// <inheritdoc/>
        public bool AddCache(ICacheEntry<string, byte[]> entity)
        {
            return !this.ContainsCache(entity.Key) && this.SetCache(entity);
        }

        /// <inheritdoc/>
        public Task<bool> AddCacheAsync(ICacheEntry<string, byte[]> entity)
        {
            return Task.Run(() =>
            {
                return this.AddCache(entity);
            });
        }

        /// <inheritdoc/>
        public void ClearCache()
        {
            var package = new RedisRequestWaitPackage
            {
                packageType = RedisPackageType.Clear
            };

            var waitData = this.DmtpActor.WaitHandlePool.GetWaitData(package);
            try
            {
                using (var byteBlock = new ByteBlock())
                {
                    package.Package(byteBlock);
                    this.DmtpActor.Send(this.m_redis_Request, byteBlock);
                }
                switch (waitData.Wait(this.Timeout))
                {
                    case WaitDataStatus.SetRunning:
                        {
                            if (waitData.WaitResult.Status == 1)
                            {
                                return;
                            }
                            else
                            {
                                throw new Exception(waitData.WaitResult.Message);
                            }
                        }
                    case WaitDataStatus.Overtime: throw new TimeoutException(TouchSocketDmtpStatus.Overtime.GetDescription());
                    case WaitDataStatus.Canceled:
                    case WaitDataStatus.Default:
                    case WaitDataStatus.Disposed:
                    default:
                        throw new Exception(TouchSocketDmtpStatus.UnknownError.GetDescription());
                }
            }
            finally
            {
                this.DmtpActor.WaitHandlePool.Destroy(waitData);
            }
        }

        /// <inheritdoc/>
        public Task ClearCacheAsync()
        {
            return Task.Run(() =>
            {
                this.ClearCache();
            });
        }

        /// <inheritdoc/>
        public bool ContainsCache(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException($"“{nameof(key)}”不能为 null 或空。", nameof(key));
            }

            var package = new RedisRequestWaitPackage
            {
                key = key,
                packageType = RedisPackageType.Contains
            };

            var waitData = this.DmtpActor.WaitHandlePool.GetWaitData(package);
            try
            {
                using (var byteBlock = new ByteBlock())
                {
                    package.Package(byteBlock);
                    this.DmtpActor.Send(this.m_redis_Request, byteBlock);
                }
                switch (waitData.Wait(this.Timeout))
                {
                    case WaitDataStatus.SetRunning:
                        {
                            return waitData.WaitResult.Status == 1
                                ? true
                                : waitData.WaitResult.Status == byte.MaxValue ? false : throw new Exception(waitData.WaitResult.Message);
                        }
                    case WaitDataStatus.Overtime: throw new TimeoutException(TouchSocketDmtpStatus.Overtime.GetDescription());
                    case WaitDataStatus.Canceled:
                    case WaitDataStatus.Default:
                    case WaitDataStatus.Disposed:
                    default:
                        throw new Exception(TouchSocketDmtpStatus.UnknownError.GetDescription());
                }
            }
            finally
            {
                this.DmtpActor.WaitHandlePool.Destroy(waitData);
            }
        }

        /// <inheritdoc/>
        public Task<bool> ContainsCacheAsync(string key)
        {
            return Task.Run(() =>
            {
                return this.ContainsCache(key);
            });
        }

        /// <inheritdoc/>
        public TValue Get<TValue>(string key)
        {
            return this.TryGet(key, out TValue cache) ? cache : default;
        }

        /// <inheritdoc/>
        public ICacheEntry<string, byte[]> GetCache(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException($"“{nameof(key)}”不能为 null 或空。", nameof(key));
            }

            var package = new RedisRequestWaitPackage()
            {
                key = key,
                packageType = RedisPackageType.Get
            };

            var waitData = this.DmtpActor.WaitHandlePool.GetWaitData(package);
            try
            {
                using (var byteBlock = new ByteBlock((package.value == null ? 0 : package.value.Length) + 1024))
                {
                    package.Package(byteBlock);
                    this.DmtpActor.Send(this.m_redis_Request, byteBlock);
                }
                switch (waitData.Wait(this.Timeout))
                {
                    case WaitDataStatus.SetRunning:
                        {
                            var responsePackage = (RedisResponseWaitPackage)waitData.WaitResult;
                            return responsePackage.Status == 1
                                ? new CacheEntry<string, byte[]>(key)
                                {
                                    Value = responsePackage.value
                                }
                                : responsePackage.Status == byte.MaxValue ? new CacheEntry<string, byte[]>(key) : (ICacheEntry<string, byte[]>)default;
                        }
                    case WaitDataStatus.Overtime: throw new TimeoutException(TouchSocketDmtpStatus.Overtime.GetDescription());
                    case WaitDataStatus.Canceled:
                    case WaitDataStatus.Default:
                    case WaitDataStatus.Disposed:
                    default:
                        throw new TimeoutException(TouchSocketDmtpStatus.UnknownError.GetDescription());
                }
            }
            finally
            {
                this.DmtpActor.WaitHandlePool.Destroy(waitData);
            }
        }

        /// <inheritdoc/>
        public Task<ICacheEntry<string, byte[]>> GetCacheAsync(string key)
        {
            return Task.Run(() =>
            {
                return this.GetCache(key);
            });
        }

        /// <summary>
        /// 处理收到的消息
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<bool> InputReceivedData(DmtpMessage message)
        {
            if (message.ProtocolFlags == this.m_redis_Request)
            {
                var waitResult = new RedisResponseWaitPackage();
                try
                {
                    var package = new RedisRequestWaitPackage();
                    package.Unpackage(message.BodyByteBlock);
                    waitResult.Sign = package.Sign;

                    switch (package.packageType)
                    {
                        case RedisPackageType.Set:
                            {
                                var success = this.ICache.SetCache(new CacheEntry<string, byte[]>(package.key)
                                {
                                    Duration = package.timeSpan.Value,
                                    Value = package.value
                                });
                                waitResult.Status = success ? (byte)1 : byte.MaxValue;
                                break;
                            }
                        case RedisPackageType.Get:
                            {
                                var cache = this.ICache.GetCache(package.key);
                                if (cache != null)
                                {
                                    waitResult.Status = 1;
                                    waitResult.value = cache.Value;
                                }
                                else
                                {
                                    waitResult.Status = byte.MaxValue;
                                }
                            }
                            break;

                        case RedisPackageType.Contains:
                            {
                                waitResult.Status = this.ICache.ContainsCache(package.key) ? (byte)1 : byte.MaxValue;
                            }
                            break;

                        case RedisPackageType.Remove:
                            {
                                waitResult.Status = this.ICache.RemoveCache(package.key) ? (byte)1 : byte.MaxValue;
                            }
                            break;

                        case RedisPackageType.Clear:
                            {
                                this.ICache.ClearCache();
                                waitResult.Status = 1;
                            }
                            break;

                        default:
                            return true;
                    }
                }
                catch (Exception ex)
                {
                    waitResult.Status = 2;
                    waitResult.Message = ex.Message;
                }

                using (var byteBlock = new ByteBlock())
                {
                    waitResult.Package(byteBlock);
                    await this.DmtpActor.SendAsync(this.m_redis_Response, byteBlock);
                }
                return true;
            }
            else if (message.ProtocolFlags == this.m_redis_Response)
            {
                var waitResult = new RedisResponseWaitPackage();
                waitResult.Unpackage(message.BodyByteBlock);
                this.DmtpActor.WaitHandlePool.SetRun(waitResult);
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public bool RemoveCache(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException($"“{nameof(key)}”不能为 null 或空。", nameof(key));
            }

            var package = new RedisRequestWaitPackage
            {
                key = key,
                packageType = RedisPackageType.Remove
            };

            var waitData = this.DmtpActor.WaitHandlePool.GetWaitData(package);
            try
            {
                using (var byteBlock = new ByteBlock((package.value == null ? 0 : package.value.Length) + 1024))
                {
                    package.Package(byteBlock);
                    this.DmtpActor.Send(this.m_redis_Request, byteBlock);
                }
                switch (waitData.Wait(this.Timeout))
                {
                    case WaitDataStatus.SetRunning:
                        {
                            return waitData.WaitResult.Status == 1
                                ? true
                                : waitData.WaitResult.Status == byte.MaxValue ? false : throw new Exception(waitData.WaitResult.Message);
                        }
                    case WaitDataStatus.Overtime: throw new TimeoutException(Resources.TouchSocketDmtpStatus.Overtime.GetDescription());
                    case WaitDataStatus.Canceled: return false;
                    case WaitDataStatus.Default:
                    case WaitDataStatus.Disposed:
                    default:
                        throw new TimeoutException(Resources.TouchSocketDmtpStatus.UnknownError.GetDescription());
                }
            }
            finally
            {
                this.DmtpActor.WaitHandlePool.Destroy(waitData);
            }
        }

        /// <inheritdoc/>
        public Task<bool> RemoveCacheAsync(string key)
        {
            return Task.Run(() =>
            {
                return this.RemoveCache(key);
            });
        }

        /// <inheritdoc/>
        public bool Set<TValue>(string key, TValue value, int duration = 60000)
        {
            var cache = new CacheEntry<string, byte[]>(key)
            {
                Duration = TimeSpan.FromSeconds(duration)
            };
            cache.Value = value is byte[] bytes ? bytes : this.Converter.ConvertTo(value);
            return this.SetCache(cache);
        }

        /// <inheritdoc/>
        public bool SetCache(ICacheEntry<string, byte[]> cache)
        {
            if (string.IsNullOrEmpty(cache.Key))
            {
                throw new ArgumentException($"“{nameof(cache.Key)}”不能为 null 或空。", nameof(cache.Key));
            }

            if (cache is null)
            {
                throw new ArgumentNullException(nameof(cache));
            }

            var package = new RedisRequestWaitPackage
            {
                key = cache.Key,
                timeSpan = cache.Duration,
                value = cache.Value,
                packageType = RedisPackageType.Set
            };

            var waitData = this.DmtpActor.WaitHandlePool.GetWaitData(package);
            try
            {
                using (var byteBlock = new ByteBlock((package.value == null ? 0 : package.value.Length) + 1024))
                {
                    package.Package(byteBlock);
                    this.DmtpActor.Send(this.m_redis_Request, byteBlock);
                }
                switch (waitData.Wait(this.Timeout))
                {
                    case WaitDataStatus.SetRunning:
                        {
                            return waitData.WaitResult.Status == 1 || (waitData.WaitResult.Status == byte.MaxValue ? false : throw new Exception(waitData.WaitResult.Message));
                        }
                    case WaitDataStatus.Overtime: throw new TimeoutException(Resources.TouchSocketDmtpStatus.Overtime.GetDescription());
                    case WaitDataStatus.Canceled: return false;
                    case WaitDataStatus.Default:
                    case WaitDataStatus.Disposed:
                    default:
                        throw new TimeoutException(TouchSocketDmtpStatus.UnknownError.GetDescription());
                }
            }
            finally
            {
                this.DmtpActor.WaitHandlePool.Destroy(waitData);
            }
        }

        /// <inheritdoc/>
        public Task<bool> SetCacheAsync(ICacheEntry<string, byte[]> entity)
        {
            return Task.Run(() =>
            {
                return this.SetCache(entity);
            });
        }

        /// <summary>
        /// 设置处理协议标识的起始标识。
        /// </summary>
        /// <param name="start"></param>
        public void SetProtocolFlags(ushort start)
        {
            this.m_redis_Request = start++;
            this.m_redis_Response = start++;
        }

        /// <inheritdoc/>
        public bool TryGet<TValue>(string key, out TValue value)
        {
            var cache = this.GetCache(key);

            if (cache != null)
            {
                if (cache.Value is null)
                {
                    value = default;
                    return true;
                }
                if (cache.Value is TValue value1)
                {
                    value = value1;
                    return true;
                }
                value = (TValue)this.Converter.ConvertFrom(cache.Value, typeof(TValue));
                return true;
            }
            value = default;
            return false;
        }

        #region 字段

        private ushort m_redis_Request;
        private ushort m_redis_Response;

        #endregion 字段
    }
}