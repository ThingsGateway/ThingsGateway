#region copyright

//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

#endregion

namespace ThingsGateway.Core;

/// <summary>
/// Parallel扩展
/// </summary>
public static class ParallelExtensions
{
    private static readonly ParallelOptions _options = new();

    static ParallelExtensions()
    {
        _options.MaxDegreeOfParallelism = Environment.ProcessorCount / 2 == 0 ? 1 : Environment.ProcessorCount / 2;
    }

    /// <summary>
    /// 使用默认的并行设置执行<see cref="Parallel.ForEach{TSource}(IEnumerable{TSource}, Action{TSource})"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="body"></param>
    public static void ParallelForEach<T>(this IEnumerable<T> source, Action<T> body)
    {
        Parallel.ForEach(source, _options, variable =>
        {
            body(variable);
        });
    }

    /// <summary>
    /// 执行<see cref="Parallel.ForEach{TSource}(IEnumerable{TSource}, Action{TSource})"/>
    /// </summary>
    public static void ParallelForEach<T>(this IEnumerable<T> source, Action<T> body, int parallelCount)
    {
        var options = new ParallelOptions();
        options.MaxDegreeOfParallelism = parallelCount / 2 == 0 ? 1 : parallelCount;
        Parallel.ForEach(source, options, variable =>
        {
            body(variable);
        });
    }

    /// <summary>
    /// 执行<see cref="Parallel.ForEach{TSource}(IEnumerable{TSource}, Action{TSource})"/>
    /// </summary>
    public static async Task ParallelForEachAsync<T>(this IEnumerable<T> source, Func<T, CancellationToken, ValueTask> body, int parallelCount, CancellationToken cancellationToken = default)
    {
        var options = new ParallelOptions(); options.CancellationToken = cancellationToken;
        options.MaxDegreeOfParallelism = parallelCount / 2 == 0 ? 1 : parallelCount;
        await Parallel.ForEachAsync(source, options, body);
    }
}