﻿// ------------------------------------------------------------------------
// 版权信息
// 版权归百小僧及百签科技（广东）有限公司所有。
// 所有权利保留。
// 官方网站：https://baiqian.com
//
// 许可证信息
// 项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。
// 许可证的完整文本可以在源代码树根目录中的 LICENSE-APACHE 和 LICENSE-MIT 文件中找到。
// ------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Diagnostics;
using System.Reflection;

using ThingsGateway.FriendlyException;

namespace ThingsGateway.Schedule;

/// <summary>
/// 作业调度器后台主机服务
/// </summary>
internal sealed class ScheduleHostedService : BackgroundService
{
    /// <summary>
    /// 避免由 CLR 的终结器捕获该异常从而终止应用程序，让所有未觉察异常被觉察
    /// </summary>
    internal event EventHandler<UnobservedTaskExceptionEventArgs> UnobservedTaskException;

    /// <summary>
    /// 作业调度器日志服务
    /// </summary>
    private readonly IScheduleLogger _logger;

    /// <summary>
    /// 作业计划工厂服务
    /// </summary>
    private readonly ISchedulerFactory _schedulerFactory;

    /// <summary>
    /// 服务提供器
    /// </summary>
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// 服务检测器
    /// </summary>
    private readonly IServiceProviderIsService _serviceProviderIsService;

    /// <summary>
    /// 取消作业执行 Token 器
    /// </summary>
    private readonly IJobCancellationToken _jobCancellationToken;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="serviceProvider">服务提供器</param>
    /// <param name="logger">作业调度器日志服务</param>
    /// <param name="schedulerFactory">作业计划工厂服务</param>
    /// <param name="jobCancellationToken">取消作业执行 Token 器</param>
    /// <param name="clusterId">作业集群 Id</param>
    public ScheduleHostedService(IServiceProvider serviceProvider
        , IScheduleLogger logger
        , ISchedulerFactory schedulerFactory
        , IJobCancellationToken jobCancellationToken
        , string clusterId)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _schedulerFactory = schedulerFactory;
        _jobCancellationToken = jobCancellationToken;

        Monitor = serviceProvider.GetService<IJobMonitor>();
        Executor = serviceProvider.GetService<IJobExecutor>();
        ClusterServer = serviceProvider.GetService<IJobClusterServer>();

        _serviceProviderIsService = serviceProvider.GetService<IServiceProviderIsService>();

        ClusterId = clusterId;
    }

    /// <summary>
    /// 作业处理程序监视器
    /// </summary>
    private IJobMonitor Monitor { get; }

    /// <summary>
    /// 作业处理程序执行器
    /// </summary>
    private IJobExecutor Executor { get; }

    /// <summary>
    /// 作业集群服务
    /// </summary>
    private IJobClusterServer ClusterServer { get; }

    /// <summary>
    /// 作业集群 Id
    /// </summary>
    private string ClusterId { get; }

    /// <summary>
    /// 监听作业调度服务启动
    /// </summary>
    /// <param name="cancellationToken">后台主机服务停止时取消任务 Token</param>
    /// <returns><see cref="Task"/></returns>
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        // 作业集群启动通知
        ClusterServer?.Start(new(ClusterId));

        return base.StartAsync(cancellationToken);
    }

    /// <summary>
    /// 执行后台任务
    /// </summary>
    /// <param name="stoppingToken">后台主机服务停止时取消任务 Token</param>
    /// <returns><see cref="Task"/> 实例</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Schedule hosted service is running.");

        // 注册后台主机服务停止监听
        stoppingToken.Register(() =>
        {
            _logger.LogDebug($"Schedule hosted service is stopping.");

            // 释放作业计划工厂
            _schedulerFactory.Dispose();
        });

        // 等待作业集群指示
        await WaitingClusterAsync().ConfigureAwait(false);

        // 作业调度器初始化
        await _schedulerFactory.PreloadAsync(stoppingToken).ConfigureAwait(false);

        // 监听服务是否取消
        while (!stoppingToken.IsCancellationRequested)
        {
            // 执行具体任务
            await BackgroundProcessing(stoppingToken).ConfigureAwait(false);
        }

        _logger.LogCritical($"Schedule hosted service is stopped.");
    }

    /// <summary>
    /// 后台调用处理程序
    /// </summary>
    /// <param name="stoppingToken">后台主机服务停止时取消任务 Token</param>
    /// <returns><see cref="Task"/> 实例</returns>
    private async Task BackgroundProcessing(CancellationToken stoppingToken)
    {
        // 获取当前时间作为检查时间
        var startAt = Penetrates.GetNowTime();

        // 查找所有符合触发的作业
        var currentRunJobs = _schedulerFactory.GetCurrentRunJobs(startAt).Cast<Scheduler>().ToList();

        // 输出作业调度器检查信息
        _logger.LogDebug("Schedule hosted service is checking on <{startAt}> and finds <{Count}> schedulers that should be run.", startAt, currentRunJobs.Count);

        // 创建一个任务工厂并保证执行任务都使用当前的计划程序
        var taskFactory = new TaskFactory(TaskScheduler.Current);

        // 通过并行方式提高吞吐量并解决 Thread.Sleep 问题
        Parallel.ForEach(currentRunJobs, scheduler =>
        {
            // 解构参数
            var jobId = scheduler.JobId;
            var jobDetail = scheduler.JobDetail;
            var jobLogger = scheduler.JobLogger;
            var triggersThatShouldRun = scheduler.Triggers;

            // 逐条遍历所有符合触发的作业触发器
            foreach (var triggerThatShouldRun in triggersThatShouldRun)
            {
                // 解构参数
                var (triggerId, trigger) = triggerThatShouldRun;

                // 处理串行执行逻辑（默认并行执行）
                if (CheckIsBlocked(jobDetail, trigger, startAt)) continue;

                // 设置作业触发器状态为运行状态
                trigger.SetStatus(TriggerStatus.Running);

                // 记录运行信息和计算下一个触发时间
                var occurrenceTime = trigger.NextRunTime == null
                                            ? startAt
                                            : (startAt < trigger.NextRunTime.Value
                                                ? startAt
                                                : trigger.NextRunTime.Value);
                trigger.Increment(jobDetail, startAt);

                // 将作业触发器运行数据写入持久化
                _schedulerFactory.Shorthand(jobDetail, trigger);

                // 通过并发执行提高吞吐量并解决 Thread.Sleep 问题
                Parallel.For(0, 1, _ =>
                {
                    // 创建新的线程执行
                    taskFactory.StartNew(async () =>
                    {
                        // 创建唯一的作业运行标识
                        var runId = $"{triggerId}___{Guid.NewGuid()}";

                        // 创建服务作用域
                        var serviceScoped = _serviceProvider.CreateScope();

                        // 创建作业执行前上下文
                        var jobExecutingContext = new JobExecutingContext(jobDetail, trigger, occurrenceTime, runId, serviceScoped.ServiceProvider)
                        {
                            ExecutingTime = Penetrates.GetNowTime(),
                            Mode = trigger.Mode
                        };

                        // 执行异常对象
                        InvalidOperationException executionException = default;

                        // 作业处理程序
                        IJob jobHandler = null;

                        // 创建取消作业执行 Token
                        var jobCancellationTokenSource = _jobCancellationToken.GetOrCreate(jobId, runId, stoppingToken);

                        try
                        {
                            // 创建作业处理程序实例
                            jobHandler = _schedulerFactory.CreateJob(serviceScoped.ServiceProvider, new JobFactoryContext(jobId, jobDetail.RuntimeJobType)
                            {
                                Mode = trigger.Mode
                            });

                            // 调用执行前监视器
                            if (Monitor != default)
                            {
                                await Monitor.OnExecutingAsync(jobExecutingContext, jobCancellationTokenSource.Token).ConfigureAwait(false);
                            }

                            // 计时
                            var timeOperation = Stopwatch.StartNew();

                            // 判断是否自定义了执行器
                            if (Executor == default)
                            {
                                // 调用作业处理程序并配置出错执行重试
                                await Retry.InvokeAsync(async () =>
                                {
                                    await jobHandler.ExecuteAsync(jobExecutingContext, jobCancellationTokenSource.Token).ConfigureAwait(false);
                                }
                                , trigger.NumRetries
                                , trigger.RetryTimeout
                                , retryAction: (total, times) =>
                                {
                                    // 输出重试日志
                                    _logger.LogWarning("Retrying {times}/{total} times for {jobExecutingContext}", times, total, jobExecutingContext);
                                }).ConfigureAwait(false);
                            }
                            else
                            {
                                await Executor.ExecuteAsync(jobExecutingContext, jobHandler, jobCancellationTokenSource.Token).ConfigureAwait(false);
                            }

                            // 计时结束
                            timeOperation.Stop();
                            trigger.ElapsedTime = timeOperation.ElapsedMilliseconds;

                            // 同步上下文设置的 Result
                            trigger.Result = jobExecutingContext.Result;

                            // 设置作业触发器状态为就绪状态
                            if (trigger.CheckAndFixNextOccurrence(jobDetail, startAt)) trigger.SetStatus(TriggerStatus.Ready);

                            // 将作业触发器运行数据写入持久化
                            _schedulerFactory.Shorthand(jobDetail, trigger);
                        }
                        catch (Exception ex)
                        {
                            // 记录错误信息，包含错误次数和运行状态
                            trigger.IncrementErrors(jobDetail, startAt);

                            // 将作业触发器运行数据写入持久化
                            _schedulerFactory.Shorthand(jobDetail, trigger);

                            // 重置 Result
                            trigger.Result = null;

                            // 输出异常日志
                            _logger.LogError(ex, "Error occurred executing in {jobExecutingContext}.", jobExecutingContext);

                            // 标记异常
                            executionException = new InvalidOperationException(string.Format("Error occurred executing in {0}.", jobExecutingContext), ex);

                            // 捕获 Task 任务异常信息并统计所有异常
                            if (UnobservedTaskException != default)
                            {
                                var args = new UnobservedTaskExceptionEventArgs(
                                    ex as AggregateException ?? new AggregateException(ex));

                                UnobservedTaskException.Invoke(this, args);
                            }
                        }
                        finally
                        {
                            // 标记上一个触发器阻塞已完成
                            if (!jobDetail.Concurrent)
                            {
                                jobDetail.Blocked = false;
                            }

                            // 调用作业异常回退或作业执行后监视器
                            if (executionException != null || Monitor != default)
                            {
                                // 创建作业执行后上下文
                                var jobExecutedContext = new JobExecutedContext(jobDetail, trigger, occurrenceTime, runId, serviceScoped.ServiceProvider)
                                {
                                    ExecutedTime = Penetrates.GetNowTime(),
                                    Exception = executionException,
                                    Result = jobExecutingContext.Result,
                                    Mode = trigger.Mode
                                };

                                // 是否定义 FallbackAsync 方法
                                var isDefinedFallbackAsyncMethod = jobHandler != null && jobHandler.GetType().GetMethod(nameof(IJob.FallbackAsync)
                                    , BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly
                                    , null
                                    , new[] { typeof(JobExecutedContext), typeof(CancellationToken) }
                                    , null) != null;
                                if (isDefinedFallbackAsyncMethod)
                                {
                                    // 触发作业执行异常回退逻辑
                                    try
                                    {
                                        // 输出作业执行回退日志
                                        _logger.LogInformation("Fallback called in {jobExecutedContext}.", jobExecutedContext);

                                        await jobHandler.FallbackAsync(jobExecutedContext, jobCancellationTokenSource.Token).ConfigureAwait(false);
                                    }
                                    // 处理二次异常情况，将异常进行汇总
                                    catch (Exception fallbackEx)
                                    {
                                        var aggregateException = new AggregateException(executionException, fallbackEx);
                                        jobExecutedContext.Exception = aggregateException;

                                        // 输出 Fallback 二次异常日志
                                        _logger.LogError(aggregateException, "Fallback called error in {jobExecutingContext}.", jobExecutingContext);
                                    }
                                }

                                // 调用作业执行后监视器
                                try
                                {
                                    if (Monitor != null) await Monitor.OnExecutedAsync(jobExecutedContext, jobCancellationTokenSource.Token).ConfigureAwait(false);
                                }
                                catch { }
                            }

                            // 将作业信息运行数据写入持久化
                            _schedulerFactory.Shorthand(jobDetail);

                            // 写入作业执行详细日志
                            if (executionException == null)
                            {
                                jobLogger?.LogInformation("{jobExecutingContext}", jobExecutingContext);
                            }
                            else
                            {
                                jobLogger?.LogError(executionException, "{jobExecutingContext}", jobExecutingContext);
                            }

                            // 记录作业触发器运行信息
                            await trigger.RecordTimelineAsync(_schedulerFactory, jobId, executionException?.ToString()).ConfigureAwait(false);

                            // 重置触发模式
                            trigger.Mode = 0;

                            // 释放服务作用域
                            await ReleaseJobHandlerAsync(jobHandler).ConfigureAwait(false);
                            jobHandler = null;
                            serviceScoped.Dispose();

                            // 释放取消作业执行 Token
                            _jobCancellationToken.Cancel(jobId, triggerId, false);

                            // 通知 GC 垃圾回收器回收
                            _schedulerFactory.GCCollect();
                        }
                    }, stoppingToken);
                });
            }
        });

        // 作业调度器进入休眠状态
        await _schedulerFactory.SleepAsync(startAt).ConfigureAwait(false);
    }

    /// <summary>
    /// 监听作业调度服务停止
    /// </summary>
    /// <param name="cancellationToken">后台主机服务停止时取消任务 Token</param>
    /// <returns><see cref="Task"/></returns>
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        // 作业集群停止通知
        ClusterServer?.Stop(new(ClusterId));

        return base.StopAsync(cancellationToken);
    }

    /// <summary>
    /// 监听作业调度器对象销毁
    /// </summary>
    public override void Dispose()
    {
        // 作业集群宕机通知
        ClusterServer?.Crash(new(ClusterId));

        base.Dispose();
    }

    /// <summary>
    /// 检查是否是串行执行
    /// </summary>
    /// <param name="jobDetail">作业信息</param>
    /// <param name="trigger">作业触发器</param>
    /// <param name="startAt">检查时间</param>
    /// <returns>返回 true 是串行执行，则阻塞并进入下一轮，返回 false 则继续执行</returns>
    private bool CheckIsBlocked(JobDetail jobDetail, Trigger trigger, DateTime startAt)
    {
        // 如果是并行执行则跳过
        if (jobDetail.Concurrent) return false;

        // 标记当前作业已经有触发器正在执行
        if (!jobDetail.Blocked)
        {
            jobDetail.Blocked = true;

            // 将作业信息运行数据写入持久化
            _schedulerFactory.Shorthand(jobDetail);

            return false;
        }
        // 标记当前作业的当前触发器【本该执行未执行】
        else
        {
            // 设置作业触发器状态为阻塞状态
            trigger.SetStatus(TriggerStatus.Blocked);

            // 记录运行信息和计算下一个触发时间
            var occurrenceTime = trigger.NextRunTime.Value;
            trigger.Increment(jobDetail, startAt);

            // 将作业触发器运行数据写入持久化
            _schedulerFactory.Shorthand(jobDetail, trigger);

            // 输出阻塞日志
            _logger.LogWarning("{occurrenceTime}: The <{TriggerId}> trigger of job <{JobId}> failed to execute as scheduled due to blocking.", occurrenceTime, trigger.TriggerId, jobDetail.JobId);

            return true;
        }
    }

    /// <summary>
    /// 等待作业集群指示
    /// </summary>
    /// <returns><see cref="Task"/></returns>
    private async Task WaitingClusterAsync()
    {
        // 空检查
        if (ClusterServer == null) return;

        // 输出作业集群进入等待日志
        _logger.LogInformation("The job cluster of <{ClusterId}> service has been enabled, and waiting for instructions.", ClusterId);

        // 等待作业集群服务返回消息
        await ClusterServer.WaitingForAsync(new(ClusterId)).ConfigureAwait(false);

        // 输出作业集群可正常工作日志
        _logger.LogWarning("The job cluster of <{ClusterId}> service worked now, and the current schedule hosted service will be preload.", ClusterId);
    }

    /// <summary>
    /// 释放作业处理程序对象
    /// </summary>
    /// <param name="jobHandler"><see cref="IJob"/></param>
    /// <returns><see cref="Task"/></returns>
    private async Task ReleaseJobHandlerAsync(IJob jobHandler)
    {

        if (jobHandler is null) return;

        var isService = _serviceProviderIsService.IsService(jobHandler.GetType());
        if (isService) return;

        // 手动释放
        if (jobHandler is IDisposable disposable)
        {
            disposable.Dispose();
        }

        // 手动释放
        if (jobHandler is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync().ConfigureAwait(false);
        }
    }
}