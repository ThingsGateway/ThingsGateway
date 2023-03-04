using System.Collections.Generic;
using System.Linq;

namespace ThingsGateway.Foundation.Extension
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// 将项目列表分解为特定大小的块
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="chunksize"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> ChunkTrivialBetter<T>(this IEnumerable<T> source, int chunksize)
        {
            var pos = 0;
            while (source.Skip(pos).Any())
            {
                yield return source.Skip(pos).Take(chunksize);
                pos += chunksize;
            }
        }
    }
}