//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using NewLife.Caching.Queues;

namespace ThingsGateway.Cache
{
    /// <summary>
    /// Redis实例
    /// </summary>
    public partial interface ISimpleRedis
    {
        /// <summary>
        /// 添加到队列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="value">值数组</param>
        /// <returns>添加数量</returns>
        int AddQueue<T>(string key, T[] value);

        /// <summary>
        /// 添加到队列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <returns>添加数量</returns>
        int AddQueue<T>(string key, T value);

        /// <summary>
        /// 获取队列实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <returns>队列实例</returns>
        RedisQueue<T> GetRedisQueue<T>(string key);

        /// <summary>
        /// 从队列中获取数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="Count">数量</param>
        /// <returns>数据列表</returns>
        List<T> GetQueue<T>(string key, int Count = 1);

        /// <summary>
        /// 取一条数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="timeout">阻塞时间</param>
        /// <returns>数据</returns>
        T GetQueueOne<T>(string key, int timeout = 1);

        /// <summary>
        /// 异步取一条数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="timeout">阻塞时间</param>
        /// <returns>数据</returns>
        Task<T> GetQueueOneAsync<T>(string key, int timeout = 1);
    }
}