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
        /// <param name="value">值</param>
        /// <returns></returns>
        string AddSteamQueue<T>(string key, T value);

        /// <summary>
        /// 获取智能重复消费队列实例:新的消费组从当前开始消费，旧的不变
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="group">消费组</param>
        /// <param name="consumer">消费者</param>
        /// <returns></returns>
        RedisStream<T> GetAutoSteamQueue<T>(string key, string group, string consumer = "");

        /// <summary>
        /// 获取RedisStream实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <returns>RedisStream实例</returns>
        RedisStream<T> GetRedisStream<T>(string key);

        /// <summary>
        /// 获取重复消息队列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="group">消费组</param>
        /// <param name="consumer">消费者</param>
        /// <param name="fromLastOffset">首次消费时的消费策略,默认值false，表示是否从头部开始消费</param>
        /// <returns></returns>
        RedisStream<T> GetSteamQueue<T>(string key, string group = "", string consumer = "", bool fromLastOffset = false);

        /// <summary>
        /// 确认消费
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="keys">消息id列表</param>
        /// <returns></returns>
        int SteamQueueAcknowledge<T>(string key, string[] keys);

        /// <summary>
        /// 从队列拿数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="count">数量</param>
        /// <returns></returns>
        List<T> SteamQueueTake<T>(string key, int count);

        /// <summary>
        /// 从队列拿一条消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="timeout">超时时间</param>
        /// <returns></returns>
        Task<Message> SteamQueueTakeMessageAsync<T>(string key, int timeout = 1);

        /// <summary>
        /// 从队列拿指定条数消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="timeout">超时时间</param>
        /// <returns></returns>
        Task<IList<Message>> SteamQueueTakeMessagesAsync<T>(string key, int timeout = 1);

        /// <summary>
        /// 从队列拿一条消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="timeout">超时时间</param>
        /// <returns></returns>
        T SteamQueueTakeOne<T>(string key, int timeout = 1);

        /// <summary>
        /// 从队列拿一条消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="timeout">超时时间</param>
        /// <returns></returns>
        Task<T> SteamQueueTakeOneAsync<T>(string key, int timeout = 0);
    }
}