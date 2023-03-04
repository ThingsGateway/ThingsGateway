using System.Collections.Generic;

namespace ThingsGateway.Foundation.Extension
{
    public static class ConcurrentQueueExtensions
    {
        /// <summary>
        /// 批量出队
        /// </summary>
        public static List<T> ToListWithDequeue<T>(this IntelligentConcurrentQueue<T> values, int conut)
        {
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