//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation.OpcUa;

internal static class CollectionExtension
{


    /// <summary>
    /// 将项目列表分解为特定大小的块
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source">原数组</param>
    /// <param name="chunkSize">分组大小</param>
    /// <param name="isToList">是否ToList</param>
    /// <returns></returns>
    internal static IEnumerable<IEnumerable<T>> ChunkBetter<T>(this IEnumerable<T> source, int chunkSize, bool isToList = false)
    {
        if (chunkSize <= 0)
            chunkSize = source.Count();
        var pos = 0;
        while (source.Skip(pos).Any())
        {
            var chunk = source.Skip(pos).Take(chunkSize);
            yield return isToList ? chunk.ToList() : chunk;
            pos += chunkSize;
        }
    }

    /// <summary>
    /// 移除符合条件的元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="where"></param>
    internal static void RemoveWhere<T>(this ICollection<T> @this, Func<T, bool> @where)
    {
        foreach (var obj in @this.Where(where).ToList())
        {
            @this.Remove(obj);
        }
    }

    /// <summary>
    /// 异步Select
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <returns></returns>
    internal static Task<TResult[]> SelectAsync<T, TResult>(this IEnumerable<T> source, Func<T, Task<TResult>> selector)
    {
        return Task.WhenAll(source.Select(selector));
    }
}
