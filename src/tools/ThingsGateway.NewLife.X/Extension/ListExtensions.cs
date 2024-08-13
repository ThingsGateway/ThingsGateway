//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.NewLife.X.Collections.Generic;

/// <summary>扩展List，支持遍历中修改元素</summary>
public static class ListExtensions
{
    /// <summary>线程安全，搜索并返回第一个，支持遍历中修改元素</summary>
    /// <param name="list">实体列表</param>
    /// <param name="match">条件</param>
    /// <returns></returns>
    public static T? Find<T>(this IList<T> list, Predicate<T> match)
    {
        if (list is List<T> list2) return list2.Find(match);

        return list.ToArray().FirstOrDefault(e => match(e));
    }

    /// <summary>线程安全，搜索并返回第一个，支持遍历中修改元素</summary>
    /// <param name="list">实体列表</param>
    /// <param name="match">条件</param>
    /// <returns></returns>
    public static IList<T> FindAll<T>(this IList<T> list, Predicate<T> match)
    {
        if (list is List<T> list2) return list2.FindAll(match);

        return list.ToArray().Where(e => match(e)).ToList();
    }
}
