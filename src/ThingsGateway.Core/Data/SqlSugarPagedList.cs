//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;

namespace ThingsGateway.Core;

/// <summary>
/// SqlSugar 分页泛型集合
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public class SqlSugarPagedList<TEntity>
{
    /// <summary>
    /// 页码
    /// </summary>
    public int Current { get; set; }

    /// <summary>
    /// 是否有下一页
    /// </summary>
    public bool HasNextPages { get; set; }

    /// <summary>
    /// 是否有上一页
    /// </summary>
    public bool HasPrevPages { get; set; }

    /// <summary>
    /// 总页数
    /// </summary>
    public int Pages { get; set; }

    /// <summary>
    /// 当前页集合
    /// </summary>
    [NotNull]
    public IEnumerable<TEntity>? Records { get; set; }

    /// <summary>
    /// 数量
    /// </summary>
    public int Size { get; set; }

    /// <summary>
    /// 总条数
    /// </summary>
    public int Total { get; set; }
}
