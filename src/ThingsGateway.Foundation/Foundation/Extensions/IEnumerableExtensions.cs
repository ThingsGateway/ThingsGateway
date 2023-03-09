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
        public static List<List<T>> ChunkTrivialBetter<T>(this IEnumerable<T> source, int chunksize)
        {
            var pos = 0;
            List<List<T>> n = new();
            while (source.Skip(pos).Any())
            {
                n.Add(source.Skip(pos).Take(chunksize).ToList());
                pos += chunksize;
            }
            return n;
        }
    }
}