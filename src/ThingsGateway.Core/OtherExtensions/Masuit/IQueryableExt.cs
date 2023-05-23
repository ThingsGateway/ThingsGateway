#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using System.Linq;

namespace ThingsGateway.Core
{
    public static partial class IQueryableExt
    {
        /// <summary>
        /// 生成分页集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="page">当前页</param>
        /// <param name="size">页大小</param>
        /// <returns></returns>
        public static PagedList<T> ToPagedList<T>(this IQueryable<T> query, int page, int size)
        {
            var totalCount = query.Count();
            if (1L * page * size > totalCount)
            {
                page = (int)Math.Ceiling(totalCount / (size * 1.0));
            }

            if (page <= 0)
            {
                page = 1;
            }

            var list = query.Skip(size * (page - 1)).Take(size).ToList();
            return new PagedList<T>(list, page, size, totalCount);
        }

        /// <summary>
        /// 生成分页集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="page">当前页</param>
        /// <param name="size">页大小</param>
        /// <returns></returns>
        public static PagedList<T> ToPagedList<T>(this IEnumerable<T> query, int page, int size)
        {
            var totalCount = query.Count();
            if (1L * page * size > totalCount)
            {
                page = (int)Math.Ceiling(totalCount / (size * 1.0));
            }

            if (page <= 0)
            {
                page = 1;
            }

            var list = query.Skip(size * (page - 1)).Take(size).ToList();
            return new PagedList<T>(list, page, size, totalCount);
        }
    }
}