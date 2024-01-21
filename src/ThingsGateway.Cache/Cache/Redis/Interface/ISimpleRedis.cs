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

using NewLife.Caching.Models;

namespace ThingsGateway.Cache
{
    /// <summary>
    /// Redis实例
    /// </summary>
    public partial interface ISimpleRedis
    {
        /// <summary>
        /// 获取FullRedis实例
        /// </summary>
        /// <returns></returns>
        FullRedis GetFullRedis();

        /// <summary>
        /// 获取所有的Key
        /// </summary>
        /// <returns></returns>
        List<string> AllKeys();

        /// <summary>
        /// 清空所有缓存项
        /// </summary>
        void Clear();

        /// <summary>
        /// 是否包含某一个key
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        bool ContainsKey(string key);

        /// <summary>
        /// 根据Key模糊删除，从key头开始匹配
        /// </summary>
        /// <param name="pattern">部分键</param>
        /// <returns>删除数量</returns>
        long DelByPattern(string pattern);

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="key">键</param>
        /// <returns></returns>
        TEntity Get<TEntity>(string key);

        /// <summary>
        /// 根据Key移除数据
        /// </summary>
        /// <param name="key">键</param>
        void Remove(string key);

        /// <summary>
        /// 根据key删除全部数据
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="count">删除数量</param>
        /// <returns>删除数量</returns>
        int RemoveAllByKey(string key, int count = 999);

        /// <summary>
        /// 根据key删除数据
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="count">删除数量</param>
        /// <returns>删除数量</returns>
        int RemoveByKey(string key, int count);

        /// <summary>
        /// 搜索
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        List<string> Search(SearchModel model);

        /// <summary>
        /// 插入数据,设置过期时间
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="cacheTime">过期时间</param>
        /// <returns></returns>
        bool Set<TEntity>(string key, TEntity value, TimeSpan cacheTime);

        /// <summary>
        /// 插入数据
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="expire">过期时间</param>
        /// <returns></returns>
        bool Set<TEntity>(string key, TEntity value, int expire = -1);

        /// <summary>
        /// 设置过期时间
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="timeSpan">过期时间</param>
        void SetExpire(string key, TimeSpan timeSpan);

        /// <summary>
        /// 重命名
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="newKey">新键</param>
        /// <param name="overwrire">是否覆盖</param>
        /// <returns></returns>
        bool Rename(string key, string newKey, bool overwrire = true);
    }
}