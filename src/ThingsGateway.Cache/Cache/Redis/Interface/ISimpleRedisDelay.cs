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
        /// 添加一条数据到延迟队列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="delay">延迟时间</param>
        /// <returns>添加成功数量</returns>
        int AddDelayQueue<T>(string key, T value, int delay);

        /// <summary>
        /// 批量添加数据到延迟队列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="delay">延迟时间</param>
        /// <returns>添加成功数量</returns>
        int AddDelayQueue<T>(string key, List<T> value, int delay);

        /// <summary>
        /// 获取延迟队列实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <returns></returns>
        RedisDelayQueue<T> GetDelayQueue<T>(string key);
    }
}