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
        /// 添加到可信队列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <returns>添加数量</returns>
        int AddReliableQueue<T>(string key, T value);

        /// <summary>
        /// 批量添加到可信队列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <returns>添加数量</returns>
        int AddReliableQueueList<T>(string key, List<T> value);

        /// <summary>
        /// 获取可信队列实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <returns>可信队列实例</returns>
        RedisReliableQueue<T> GetRedisReliableQueue<T>(string key);

        /// <summary>
        /// 从可信队列获取数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="count">数量</param>
        /// <returns>数据列表</returns>
        List<T> ReliableTake<T>(string key, int count);

        /// <summary>
        /// 从可信队列获取一条数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <returns>数据</returns>
        T ReliableTakeOne<T>(string key);

        /// <summary>
        /// 异步从可信队列获取一条数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <returns>数据</returns>
        Task<T> ReliableTakeOneAsync<T>(string key);

        /// <summary>
        /// 回滚所有未消费完成的数据
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="retryInterval">间隔</param>
        /// <returns>回滚数量</returns>
        int RollbackAllAck(string key, int retryInterval = 60);
    }
}