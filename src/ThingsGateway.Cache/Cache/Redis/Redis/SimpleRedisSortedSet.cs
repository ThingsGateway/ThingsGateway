//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Cache
{
    /// <summary>
    /// SortedSet
    /// </summary>
    public partial class SimpleRedis : ISimpleRedis
    {
        /// <inheritdoc  />
        public RedisSortedSet<T> GetRedisSortedSet<T>(string key)
        {
            var zset = redisConnection.GetSortedSet<T>(key);
            return zset;
        }

        /// <inheritdoc  />
        public double SortedSetAdd<T>(string key, Dictionary<T, double> value, string options = null)
        {
            var zset = GetRedisSortedSet<T>(key);
            return zset.Add(options, value);
        }

        /// <inheritdoc  />
        public int SortedSetAdd<T>(string key, T member, double score)
        {
            var zset = GetRedisSortedSet<T>(key);
            return zset.Add(member, score);
        }

        /// <inheritdoc  />
        public int SortedSetAdd<T>(string key, IEnumerable<T> members, double score)
        {
            var zset = GetRedisSortedSet<T>(key);
            return zset.Add(members, score);
        }

        /// <inheritdoc  />
        public IDictionary<T, double> SortedSetPopMax<T>(string key, int count = 1)
        {
            var zset = GetRedisSortedSet<T>(key);
            return zset.PopMax(count);
        }

        /// <inheritdoc  />
        public IDictionary<T, double> SortedSetPopMin<T>(string key, int count = 1)
        {
            var zset = GetRedisSortedSet<T>(key);
            return zset.PopMin(count);
        }

        /// <inheritdoc  />
        public int SortedSetFindCount<T>(string key, double min, double max)
        {
            var zset = GetRedisSortedSet<T>(key);
            return zset.FindCount(min, max);
        }

        /// <inheritdoc  />
        public T[] SortedSetRange<T>(string key, int start, int stop)
        {
            var zset = GetRedisSortedSet<T>(key);
            return zset.Range(start, stop);
        }

        /// <inheritdoc  />
        public IDictionary<T, double> SortedSetRangeWithScores<T>(string key, int start, int stop)
        {
            var zset = GetRedisSortedSet<T>(key);
            return zset.RangeWithScores(start, stop);
        }

        /// <inheritdoc  />
        public T[] SortedSetRangeByScore<T>(string key, double min, double max, int offset, int count)
        {
            var zset = GetRedisSortedSet<T>(key);
            return zset.RangeByScore(min, max, offset, count);
        }

        /// <inheritdoc  />
        public Task<T[]> SortedSetRangeByScoreAsync<T>(string key, double min, double max, int offset, int count)
        {
            var zset = GetRedisSortedSet<T>(key);
            return zset.RangeByScoreAsync(min, max, offset, count);
        }

        /// <inheritdoc  />
        public IDictionary<T, double> SortedSetRangeByScoreWithScores<T>(string key, double min, double max, int offset, int count)
        {
            var zset = GetRedisSortedSet<T>(key);
            return zset.RangeByScoreWithScores(min, max, offset, count);
        }

        /// <inheritdoc  />
        public int SortedSetRank<T>(string key, T member)
        {
            var zset = GetRedisSortedSet<T>(key);
            return zset.Rank(member);
        }

        /// <inheritdoc  />
        public IEnumerable<KeyValuePair<T, double>> SortedSetSearch<T>(string key, string pattern, int count, int position = 0)
        {
            var zset = GetRedisSortedSet<T>(key);
            return zset.Search(pattern, count, position);
        }

        /// <inheritdoc  />
        public double SortedSetIncrement<T>(string key, T member, double score)
        {
            var zset = GetRedisSortedSet<T>(key);
            return zset.Increment(member, score);
        }
    }
}