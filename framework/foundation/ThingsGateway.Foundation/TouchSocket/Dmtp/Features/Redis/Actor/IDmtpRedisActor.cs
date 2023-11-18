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

namespace ThingsGateway.Foundation.Dmtp.Redis
{
    /// <summary>
    /// 具有远程键值存贮的操作端。
    /// </summary>
    public interface IDmtpRedisActor : ICache<string, byte[]>, IActor
    {
        /// <summary>
        /// 序列化转换器。
        /// </summary>
        BytesConverter Converter { get; set; }

        /// <summary>
        /// 实际储存缓存。
        /// </summary>
        ICache<string, byte[]> ICache { get; set; }

        /// <summary>
        /// 超时设定。默认30000ms
        /// </summary>
        int Timeout { get; set; }

        /// <summary>
        /// <inheritdoc cref="ICache{TKey, TValue}.AddCache(ICacheEntry{TKey, TValue})"/>
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">参数为空</exception>
        /// <exception cref="TimeoutException">操作超时</exception>
        /// <exception cref="Exception">其他异常</exception>
        public bool Add<TValue>(string key, TValue value, int duration = 60000);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">参数为空</exception>
        /// <exception cref="TimeoutException">操作超时</exception>
        /// <exception cref="Exception">其他异常</exception>
        public Task<bool> AddCacheAsync(ICacheEntry<string, byte[]> entity);

        /// <summary>
        /// 获取缓存的键值对。
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">参数为空</exception>
        /// <exception cref="TimeoutException">操作超时</exception>
        /// <exception cref="Exception">其他异常</exception>
        public TValue Get<TValue>(string key);

        /// <summary>
        /// <inheritdoc cref="ICache{TKey, TValue}.SetCache(ICacheEntry{TKey, TValue})"/>
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">参数为空</exception>
        /// <exception cref="TimeoutException">操作超时</exception>
        /// <exception cref="Exception">其他异常</exception>
        public bool Set<TValue>(string key, TValue value, int duration = 60000);

        /// <summary>
        /// 获取指定键的值。
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">参数为空</exception>
        /// <exception cref="TimeoutException">操作超时</exception>
        /// <exception cref="Exception">其他异常</exception>
        public bool TryGet<TValue>(string key, out TValue value);
    }
}