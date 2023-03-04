using System.Linq;

namespace ThingsGateway.Core
{
    /// <summary>
    /// Linq扩展
    /// </summary>
    [SuppressSniffer]
    public static class LinqExtension
    {
        /// <summary>
        /// 是否都包含
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="first">第一个列表</param>
        /// <param name="secend">第二个列表</param>
        /// <returns></returns>
        public static bool ContainsAll<T>(this List<T> first, List<T> secend)
        {
            return secend.All(s => first.Any(f => f.Equals(s)));
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">数据列表</param>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页数量</param>
        /// <returns>分页集合</returns>
        public static SqlSugarPagedList<T> LinqPagedList<T>(this List<T> list, int pageIndex, int pageSize)
        {
            var result = list.ToPagedList(pageIndex, pageSize);//获取分页
                                                               //格式化
            return new SqlSugarPagedList<T>
            {
                Current = pageIndex,
                Size = result.PageSize,
                Records = result.Data,
                Total = result.TotalCount,
                Pages = result.TotalPages,
                HasNextPages = result.HasNext,
                HasPrevPages = result.HasPrev
            };
        }
    }
}