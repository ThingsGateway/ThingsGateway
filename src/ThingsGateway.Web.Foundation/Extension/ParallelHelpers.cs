#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/dotnetchina/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion


namespace ThingsGateway.Web.Foundation;
/// <summary>
/// Parallel扩展
/// </summary>
public static class ParallelHelpers
{
    static ParallelOptions _options = new ParallelOptions();
    static ParallelHelpers()
    {
        _options.MaxDegreeOfParallelism = Environment.ProcessorCount / 2 == 0 ? 1 : Environment.ProcessorCount / 2;
    }
    /// <summary>
    /// 使用默认的并行设置执行<see cref="Parallel.ForEach{TSource}(IEnumerable{TSource}, Action{TSource})"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="body"></param>
    public static void ParallelForEach<T>(this IEnumerable<T> source, Action<T> body) where T : class
    {
        Parallel.ForEach(source, _options, variable =>
        {
            body(variable);
        });
    }

}