using System.Collections.Concurrent;

namespace ThingsGateway.Foundation.Extension
{
    public static class ConcurrentQueueExtensions
    {
        /// <summary>
        /// 批量出队
        /// </summary>
        public static List<T> ToListWithDequeue<T>(this ConcurrentQueue<T> values, int conut = 0)
        {
            if (conut == 0)
            {
                conut = values.Count;
            }
            List<T> newlist = new();
            for (int i = 0; i < conut; i++)
            {
                if (values.TryDequeue(out T result))
                {
                    newlist.Add(result);
                }
                else
                {
                    break;
                }
            }
            return newlist;
        }
    }
}