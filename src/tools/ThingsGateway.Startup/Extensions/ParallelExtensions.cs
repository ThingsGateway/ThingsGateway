//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway;

/// <summary>
/// 提供对 IEnumerable 的并行操作扩展方法
/// </summary>
public static class ParallelExtensions
{
    /// <summary>
    /// 使用默认的并行设置执行指定的操作
    /// </summary>
    /// <typeparam name="T">集合元素类型</typeparam>
    /// <param name="source">要操作的集合</param>
    /// <param name="body">要执行的操作</param>
    public static void ParallelForEach<T>(this IEnumerable<T> source, Action<T> body)
    {
        // 创建并行操作的选项对象，设置最大并行度为当前处理器数量的一半
        ParallelOptions options = new();
        options.MaxDegreeOfParallelism = Environment.ProcessorCount / 2 == 0 ? 1 : Environment.ProcessorCount / 2;
        // 使用 Parallel.ForEach 执行指定的操作
        Parallel.ForEach(source, options, (variable) =>
        {
            body(variable);
        });
    }

    /// <summary>
    /// 使用默认的并行设置执行指定的操作
    /// </summary>
    /// <typeparam name="T">集合元素类型</typeparam>
    /// <param name="source">要操作的集合</param>
    /// <param name="body">要执行的操作</param>
    public static void ParallelForEach<T>(this IEnumerable<T> source, Action<T, ParallelLoopState, long> body)
    {
        // 创建并行操作的选项对象，设置最大并行度为当前处理器数量的一半
        ParallelOptions options = new();
        options.MaxDegreeOfParallelism = Environment.ProcessorCount / 2 == 0 ? 1 : Environment.ProcessorCount / 2;
        // 使用 Parallel.ForEach 执行指定的操作
        Parallel.ForEach(source, options, (variable, state, index) =>
        {
            body(variable, state, index);
        });
    }

    /// <summary>
    /// 执行指定的操作，并指定最大并行度
    /// </summary>
    /// <typeparam name="T">集合元素类型</typeparam>
    /// <param name="source">要操作的集合</param>
    /// <param name="body">要执行的操作</param>
    /// <param name="parallelCount">最大并行度</param>
    public static void ParallelForEach<T>(this IEnumerable<T> source, Action<T> body, int parallelCount)
    {
        // 创建并行操作的选项对象，设置最大并行度为指定的值
        var options = new ParallelOptions();
        options.MaxDegreeOfParallelism = parallelCount / 2 == 0 ? 1 : parallelCount;
        // 使用 Parallel.ForEach 执行指定的操作
        Parallel.ForEach(source, options, variable =>
        {
            body(variable);
        });
    }

    /// <summary>
    /// 异步执行指定的操作，并指定最大并行度和取消标志
    /// </summary>
    /// <typeparam name="T">集合元素类型</typeparam>
    /// <param name="source">要操作的集合</param>
    /// <param name="body">异步执行的操作</param>
    /// <param name="parallelCount">最大并行度</param>
    /// <param name="cancellationToken">取消操作的标志</param>
    /// <returns>表示异步操作的任务</returns>
    public static Task ParallelForEachAsync<T>(this IEnumerable<T> source, Func<T, CancellationToken, ValueTask> body, int parallelCount, CancellationToken cancellationToken = default)
    {
        // 创建并行操作的选项对象，设置最大并行度和取消标志
        var options = new ParallelOptions();
        options.CancellationToken = cancellationToken;
        options.MaxDegreeOfParallelism = parallelCount / 2 == 0 ? 1 : parallelCount;
        // 使用 Parallel.ForEachAsync 异步执行指定的操作，并返回表示异步操作的任务
        return Parallel.ForEachAsync(source, options, body);
    }
}
