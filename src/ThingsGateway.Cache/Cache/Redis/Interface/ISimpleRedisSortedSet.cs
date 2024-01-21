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

namespace ThingsGateway.Cache
{
    /// <summary>
    /// Redis实例
    /// </summary>
    public partial interface ISimpleRedis
    {
        /// <summary>
        /// 获取RedisSort实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <returns></returns>
        RedisSortedSet<T> GetRedisSortedSet<T>(string key);

        /// <summary>
        /// 批量添加
        /// 将所有指定成员添加到键为key有序集合（sorted set）里面。 添加时可以指定多个分数/成员（score/member）对。 如果指定添加的成员已经是有序集合里面的成员，则会更新改成员的分数（scrore）并更新到正确的排序位置。
        /// ZADD 命令在key后面分数/成员（score/member）对前面支持一些参数，他们是： XX: 仅仅更新存在的成员，不添加新成员。 NX: 不更新存在的成员。只添加新成员。
        /// CH: 修改返回值为发生变化的成员总数，原始是返回新添加成员的总数(CH 是 changed 的意思)。 更改的元素是新添加的成员，已经存在的成员更新分数。
        /// 所以在命令中指定的成员有相同的分数将不被计算在内。 注：在通常情况下，ZADD返回值只计算新添加成员的数量。 INCR: 当ZADD指定这个选项时，成员的操作就等同ZINCRBY命令，对成员的分数进行递增操作。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="options">options</param>
        /// <returns>添加到有序集合的成员数量，不包括已经存在更新分数的成员</returns>
        double SortedSetAdd<T>(string key, Dictionary<T, double> value, string options = null);

        /// <summary>
        /// 添加元素并指定分数，返回添加到集合的成员数量
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="member">元素</param>
        /// <param name="score">分数</param>
        /// <returns>添加到有序集合的成员数量，不包括已经存在更新分数的成员</returns>
        int SortedSetAdd<T>(string key, T member, double score);

        /// <summary>
        /// 批量添加，返回添加到集合的成员数量
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="members">元素集合</param>
        /// <param name="score">分数</param>
        /// <returns>添加到有序集合的成员数量，不包括已经存在更新分数的成员</returns>
        int SortedSetAdd<T>(string key, IEnumerable<T> members, double score);

        /// <summary>
        /// 返回有序集key中，score值在min和max之间(默认包括score值等于min或max)的成员个数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <returns>有序集key中，score值在min和max之间(默认包括score值等于min或max)的成员个数</returns>
        int SortedSetFindCount<T>(string key, double min, double max);

        /// <summary>
        /// 为有序集key的成员member的score值加上增量increment
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="member">元素</param>
        /// <param name="score">分速</param>
        /// <returns></returns>
        double SortedSetIncrement<T>(string key, T member, double score);

        /// <summary>
        /// 删除并返回有序集合key中的最多count个具有最高得分的成员
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="count">数量</param>
        /// <returns></returns>
        IDictionary<T, double> SortedSetPopMax<T>(string key, int count = 1);

        /// <summary>
        /// 删除并返回有序集合key中的最多count个具有最低得分的成员
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="count"></param>
        /// <returns></returns>
        IDictionary<T, double> SortedSetPopMin<T>(string key, int count = 1);

        /// <summary>
        /// 返回指定范围的列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="start">开始</param>
        /// <param name="stop">结束</param>
        /// <returns></returns>
        T[] SortedSetRange<T>(string key, int start, int stop);

        /// <summary>
        /// 返回指定分数区间的成员列表，低分到高分排序
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <param name="offset">偏移</param>
        /// <param name="count">数量</param>
        /// <returns></returns>
        T[] SortedSetRangeByScore<T>(string key, double min, double max, int offset, int count);

        /// <summary>
        /// 返回指定分数区间的成员列表，低分到高分排序
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        Task<T[]> SortedSetRangeByScoreAsync<T>(string key, double min, double max, int offset, int count);

        /// <summary>
        /// 返回指定分数区间的成员分数对，低分到高分排序
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="min">最低分</param>
        /// <param name="max">最高分</param>
        /// <param name="offset">偏移</param>
        /// <param name="count">数量</param>
        /// <returns></returns>
        IDictionary<T, double> SortedSetRangeByScoreWithScores<T>(string key, double min, double max, int offset, int count);

        /// <summary>
        /// 返回指定范围的成员分数对
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="start">开始</param>
        /// <param name="stop">结束</param>
        /// <returns></returns>
        IDictionary<T, double> SortedSetRangeWithScores<T>(string key, int start, int stop);

        /// <summary>
        /// 返回有序集key中成员member的排名。其中有序集成员按score值递增(从小到大)顺序排列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="member">元素</param>
        /// <returns></returns>
        int SortedSetRank<T>(string key, T member);

        /// <summary>
        ///  模糊搜索，支持?和*
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="pattern">表达式</param>
        /// <param name="count">数量</param>
        /// <param name="position">位置</param>
        /// <returns></returns>
        IEnumerable<KeyValuePair<T, double>> SortedSetSearch<T>(string key, string pattern, int count, int position = 0);
    }
}