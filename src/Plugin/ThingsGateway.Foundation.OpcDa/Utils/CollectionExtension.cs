//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation.OpcDa;

internal static class CollectionExtension
{
    /// <summary>
    /// 将项目列表分解为特定大小的块
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="chunksize"></param>
    /// <returns></returns>
    internal static List<List<T>> ChunkTrivialBetter<T>(this IEnumerable<T> source, int chunksize)
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
}
