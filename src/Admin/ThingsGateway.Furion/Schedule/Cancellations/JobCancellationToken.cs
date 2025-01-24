// ------------------------------------------------------------------------
// 版权信息
// 版权归百小僧及百签科技（广东）有限公司所有。
// 所有权利保留。
// 官方网站：https://baiqian.com
//
// 许可证信息
// 项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。
// 许可证的完整文本可以在源代码树根目录中的 LICENSE-APACHE 和 LICENSE-MIT 文件中找到。
// ------------------------------------------------------------------------

using System.Collections.Concurrent;

namespace ThingsGateway.Schedule;

/// <summary>
/// 取消作业执行 Token 器
/// </summary>
internal sealed class JobCancellationToken : IJobCancellationToken
{
    /// <summary>
    /// 取消作业执行 Token 集合
    /// </summary>
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _cancellationTokenSources;

    /// <summary>
    /// 作业调度器日志服务
    /// </summary>
    private readonly IScheduleLogger _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="logger">作业调度器日志服务</param>
    public JobCancellationToken(IScheduleLogger logger)
    {
        _logger = logger;
        _cancellationTokenSources = new();
    }

    /// <summary>
    /// 获取或创建取消作业执行 Token
    /// </summary>
    /// <param name="jobId">作业 Id</param>
    /// <param name="runId">作业触发器触发的唯一标识</param>
    /// <param name="stoppingToken">后台主机服务停止时取消任务 Token</param>
    /// <returns><see cref="CancellationToken"/></returns>
    public CancellationTokenSource GetOrCreate(string jobId, string runId, CancellationToken stoppingToken)
    {
        return _cancellationTokenSources.GetOrAdd(GetTokenKey(jobId, runId)
            , _ => CancellationTokenSource.CreateLinkedTokenSource(stoppingToken));
    }

    /// <summary>
    /// 取消（完成）正在执行的执行
    /// </summary>
    /// <param name="jobId">作业 Id</param>
    /// <param name="triggerId">作业触发器 Id</param>
    /// <param name="outputLog">是否显示日志</param>
    public void Cancel(string jobId, string triggerId = null, bool outputLog = true)
    {
        var containsTriggerId = !string.IsNullOrWhiteSpace(triggerId);

        // 获取所有以作业 Id 或作业 Id + 作业触发器 Id 开头的作业 TokenKey
        var allJobKeys = _cancellationTokenSources.Keys
            .Where(u => u.StartsWith(!containsTriggerId
                ? $"{jobId}__"
                : $"{jobId}__{triggerId}___"));

        foreach (var tokenKey in allJobKeys)
        {
            try
            {
                if (_cancellationTokenSources.TryRemove(tokenKey, out var cancellationTokenSource))
                {
                    if (!cancellationTokenSource.IsCancellationRequested) cancellationTokenSource.Cancel();
                    cancellationTokenSource.Dispose();

                    // 输出日志
                    if (outputLog)
                    {
                        if (!containsTriggerId) _logger.LogWarning("The scheduler of <{JobId}> cancellation request has been sent to stop its execution.", jobId);
                        else _logger.LogWarning("The <{triggerId}> trigger for scheduler of <{jobId}> cancellation request has been sent to stop its execution.", triggerId, jobId);
                    }
                }
                else
                {
                    // 输出日志
                    if (outputLog)
                    {
                        if (!containsTriggerId) _logger.LogWarning(message: "The scheduler of <{jobId}> is not found.", jobId);
                        else _logger.LogWarning(message: "The <{triggerId}> trigger for scheduler of <{jobId}> is not found.", triggerId, jobId);
                    }
                }
            }
            catch (TaskCanceledException) { }
            catch (AggregateException ex) when (ex.InnerExceptions.Count == 1 && ex.InnerExceptions[0] is TaskCanceledException) { }
            catch { }
        }
    }

    /// <summary>
    /// 获取取消作业执行 Token 键
    /// </summary>
    /// <param name="jobId">作业 Id</param>
    /// <param name="runId">作业触发器触发的唯一标识</param>
    /// <returns><see cref="string"/></returns>
    private static string GetTokenKey(string jobId, string runId)
    {
        return $"{jobId}__{runId}";
    }
}