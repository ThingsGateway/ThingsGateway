﻿#region copyright
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

namespace ThingsGateway.Core
{
    /// <summary>
    /// 分页集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PagedList<T>
    {
        /// <summary>
        /// 分页数据
        /// </summary>
        /// <param name="items">数据集</param>
        /// <param name="page">当前页</param>
        /// <param name="size">页大小</param>
        /// <param name="count">总条数</param>
        public PagedList(List<T> items, int page, int size, int count)
        {
            TotalCount = count;
            PageSize = size;
            CurrentPage = page;
            TotalPages = (int)Math.Ceiling(count * 1.0 / size);
            Data = items;
        }

        /// <summary>
        /// 当前页数据条数
        /// </summary>
        public int CurrentCount => Data.Count;

        /// <summary>
        /// 当前页
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// 数据集
        /// </summary>
        public List<T> Data { get; }

        /// <summary>
        /// 是否有后一页
        /// </summary>
        public bool HasNext => CurrentPage < TotalPages;

        /// <summary>
        /// 是否有前一页
        /// </summary>
        public bool HasPrev => CurrentPage > 1;

        /// <summary>
        /// 页大小
        /// </summary>
        public int PageSize { get; }

        /// <summary>
        /// 总条数
        /// </summary>
        public int TotalCount { get; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPages { get; }
    }
}