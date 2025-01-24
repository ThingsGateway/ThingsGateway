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

using System.Collections.Concurrent;

namespace ThingsGateway.Schedule;

/// <summary>
/// 作业计划工厂默认实现类（内部服务）
/// </summary>
internal sealed partial class SchedulerFactory : ISchedulerFactory
{
    /// <summary>
    /// 作业计划变更通知
    /// </summary>
    public event EventHandler<SchedulerEventArgs> OnChanged;

    /// <summary>
    /// 作业触发记录通知
    /// </summary>
    public event EventHandler<JobExecutionRecordEventArgs> OnExecutionRecord;

    /// <summary>
    /// 服务提供器
    /// </summary>
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// 作业调度器日志服务
    /// </summary>
    private readonly IScheduleLogger _logger;

    /// <summary>
    /// 取消作业执行 Token 器
    /// </summary>
    private readonly IJobCancellationToken _jobCancellationToken;

    /// <summary>
    /// 长时间运行的后台任务
    /// </summary>
    /// <remarks>实现作业运行消息持久化</remarks>
    private readonly Task _processQueueTask;

    /// <summary>
    /// 作业调度器取消休眠 Token
    /// </summary>
    /// <remarks>用于取消休眠状态（唤醒）</remarks>
    private CancellationTokenSource _sleepCancellationTokenSource;

    /// <summary>
    /// GC 垃圾回收间隔
    /// </summary>
    /// <remarks>单位毫秒</remarks>
    private const int GC_COLLECT_INTERVAL_MILLISECONDS = 3000;

    /// <summary>
    /// 作业计划集合
    /// </summary>
    private readonly ConcurrentDictionary<string, Scheduler> _schedulers = new();

    /// <summary>
    /// 作业计划构建器集合
    /// </summary>
    private readonly IList<SchedulerBuilder> _schedulerBuilders;

    /// <summary>
    /// 作业持久化记录消息队列（线程安全）
    /// </summary>
    private readonly BlockingCollection<PersistenceContext> _persistenceMessageQueue = new(12000);

    /// <summary>
    /// 不受控的作业 Id 集合
    /// </summary>
    /// <remarks>用于实现 立即执行 的作业</remarks>
    private readonly List<(string JobId, string TriggerId)> _manualRunJobIds;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="serviceProvider">服务提供器</param>
    /// <param name="logger">作业调度器日志服务</param>
    /// <param name="jobCancellationToken">取消作业执行 Token 器</param>
    /// <param name="schedulerBuilders">初始作业计划构建集合</param>
    public SchedulerFactory(IServiceProvider serviceProvider
        , IScheduleLogger logger
        , IJobCancellationToken jobCancellationToken
        , IList<SchedulerBuilder> schedulerBuilders)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _jobCancellationToken = jobCancellationToken;
        _schedulerBuilders = schedulerBuilders;
        _manualRunJobIds = new();

        Persistence = _serviceProvider.GetService<IJobPersistence>();

        // 初始化作业调度器取消休眠 Token
        CreateCancellationTokenSource();

        if (Persistence is not null)
        {
            // 创建长时间运行的后台任务，并将作业运行消息写入持久化中
            _processQueueTask = Task.Factory.StartNew(async state => await ((SchedulerFactory)state).ProcessQueueAsync().ConfigureAwait(false)
                , this, TaskCreationOptions.LongRunning);
        }
    }

    /// <summary>
    /// 作业调度持久化服务
    /// </summary>
    private IJobPersistence Persistence { get; }

    /// <summary>
    /// 标识 Preload 是否初始化完成
    /// </summary>
    private bool PreloadCompleted { get; set; } = false;

    /// <summary>
    /// GC 最近一次回收时间
    /// </summary>
    private DateTime? LastGCCollectTime { get; set; }

    /// <summary>
    /// 作业调度器初始化
    /// </summary>
    /// <param name="stoppingToken">取消任务 Token</param>
    /// <returns><see cref="Task"/></returns>
    public async Task PreloadAsync(CancellationToken stoppingToken)
    {
        // 输出作业调度度初始化日志
        _logger.LogInformation("Schedule hosted service is preloading...");

        // 标记是否初始化成功
        var preloadSucceed = true;

        // 标记是否启用作业持久化
        var isSetPersistence = Persistence is not null;

        try
        {
            // 获取持久化预设的作业计划
            IEnumerable<SchedulerBuilder> preloadSchedulerBuilders = null;
            if (isSetPersistence)
            {
                preloadSchedulerBuilders = await Persistence.PreloadAsync(stoppingToken).ConfigureAwait(false);
            }

            // 装载初始作业计划
            var initialSchedulerBuilders = _schedulerBuilders.Concat(preloadSchedulerBuilders ?? Enumerable.Empty<SchedulerBuilder>());

            // 如果作业调度器中包含作业计划构建器
            if (initialSchedulerBuilders.Any())
            {
                // 逐条遍历并加载到内存中
                foreach (var schedulerBuilder in initialSchedulerBuilders)
                {
                    SchedulerBuilder schedulerBuilderObj = null;
                    if (isSetPersistence)
                    {
                        schedulerBuilderObj = await Persistence.OnLoadingAsync(schedulerBuilder, stoppingToken).ConfigureAwait(false);
                    }

                    _ = TrySaveJob(schedulerBuilderObj ?? schedulerBuilder, out _, false);
                }
            }
        }
        catch (Exception ex)
        {
            preloadSucceed = false;
            _logger.LogError(ex, "Schedule hosted service preload failed, and a total of <0> schedulers are appended.");
        }

        // 标记当前方法初始化完成
        PreloadCompleted = true;

        // 释放引用内存并立即回收GC
        _schedulerBuilders.Clear();
        GCCollect();

        // 输出作业调度器初始化日志
        if (!preloadSucceed) _logger.LogWarning("Schedule hosted service preload completed, and a total of <{Count}> schedulers are appended.", _schedulers.Count);
    }

    /// <summary>
    /// 查找即将触发的作业
    /// </summary>
    /// <param name="startAt">起始时间</param>
    /// <param name="group">作业组名称</param>
    /// <returns><see cref="IEnumerable{IScheduler}"/></returns>
    public IEnumerable<IScheduler> GetCurrentRunJobs(DateTime startAt, string group = default)
    {
        // 定义静态内部函数用于委托检查
        bool triggerShouldRun(Scheduler s, Trigger t) => t.CurrentShouldRun(s.JobDetail, startAt);

        // 查询分组所有作业计划
        var jobsOfGroup = (GetJobs(group, true) as IEnumerable<Scheduler>);

        // 查找所有即将触发的作业计划
        var currentRunSchedulers = jobsOfGroup
                 .Where(s => s.Triggers.Values.Any(t => triggerShouldRun(s, t)))
                 .Select(s => new Scheduler(s.JobDetail, s.Triggers.Values.Where(t => triggerShouldRun(s, t)).ToDictionary(t => t.TriggerId, t => t))
                 {
                     Factory = s.Factory,
                     Logger = s.Logger,
                     JobLogger = s.JobLogger,
                 });

        // 查看 立即执行 的作业
        var runJobIds = _manualRunJobIds.ToArray();
        var manualRunSchedulers = from job in jobsOfGroup
                                  join runJob in runJobIds on job.JobId equals runJob.JobId into newRunJobs
                                  from runJob in newRunJobs.DefaultIfEmpty()
                                  where job.JobId == runJob.JobId
                                  select new Scheduler(job.JobDetail, job.Triggers.Values
                                  .Where(t => string.IsNullOrWhiteSpace(runJob.TriggerId) || (t.JobId == runJob.JobId && t.TriggerId == runJob.TriggerId))
                                  .Select(t =>
                                  {
                                      t.Mode = 1;
                                      return t;
                                  })
                                  .ToDictionary(t => t.TriggerId, t => t))
                                  {
                                      Factory = job.Factory,
                                      Logger = job.Logger,
                                      JobLogger = job.JobLogger,
                                  };

        // 合并即将执行的作业
        var willBeRunJobs = currentRunSchedulers.Concat(manualRunSchedulers);

        // 清空 立即执行 作业 Id 集合
        _manualRunJobIds.Clear();

        return willBeRunJobs;
    }

    /// <summary>
    /// 查找即将触发的作业并转换成 <see cref="SchedulerModel"/>
    /// </summary>
    /// <param name="startAt">起始时间</param>
    /// <param name="group">作业组名称</param>
    /// <returns><see cref="IEnumerable{SchedulerModel}"/></returns>
    public IEnumerable<SchedulerModel> GetCurrentRunJobsOfModels(DateTime startAt, string group = default)
    {
        return GetCurrentRunJobs(startAt, group).Select(s => s.GetModel());
    }

    /// <summary>
    /// 使作业调度器进入休眠状态
    /// </summary>
    /// <param name="startAt">起始时间</param>
    public async Task SleepAsync(DateTime startAt)
    {
        // 输出作业调度器进入休眠日志
        _logger.LogDebug("Schedule hosted service enters hibernation.");

        // 获取作业调度器总休眠时间
        var sleepMilliseconds = GetSleepMilliseconds(startAt);
        var delay = sleepMilliseconds != null
            ? sleepMilliseconds.Value
            : int.MaxValue;   // 约 24.8 天

        try
        {
            // 进入休眠状态
            while (delay > 0)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(Math.Min(int.MaxValue, delay)), _sleepCancellationTokenSource.Token).ConfigureAwait(false);
                delay -= int.MaxValue;
            }
        }
        catch (Exception ex)
        {
            // 输出非任务取消异常日志
            if (!(ex is TaskCanceledException || (ex is AggregateException aggEx && aggEx.InnerExceptions.Count == 1 && aggEx.InnerExceptions[0] is TaskCanceledException)))
            {
                _logger.LogError(ex, ex.Message);
            }

            // 重新初始化作业调度器取消休眠 Token
            CreateCancellationTokenSource();
        }
    }

    /// <summary>
    /// 取消作业调度器休眠状态（强制唤醒）
    /// </summary>
    public void CancelSleep()
    {
        try
        {
            // 取消休眠，如果存在错误立即抛出
            _sleepCancellationTokenSource.Cancel(true);
        }
        catch (Exception ex)
        {
            // 输出非任务取消异常日志
            if (!(ex is TaskCanceledException || (ex is AggregateException aggEx && aggEx.InnerExceptions.Count == 1 && aggEx.InnerExceptions[0] is TaskCanceledException)))
            {
                _logger.LogError(ex, ex.Message);
            }

            // 重新初始化作业调度器取消休眠 Token
            CreateCancellationTokenSource();
        }
    }

    /// <summary>
    /// 将作业信息运行数据写入持久化
    /// </summary>
    /// <param name="jobDetail">作业信息</param>
    /// <param name="behavior">作业持久化行为</param>
    public void Shorthand(JobDetail jobDetail, PersistenceBehavior behavior = PersistenceBehavior.Updated)
    {
        Shorthand(jobDetail, null, behavior);

        try
        {
            // 调用事件委托
            OnChanged?.Invoke(this, new(jobDetail));
        }
        catch { }
    }

    /// <summary>
    /// 将作业触发器运行数据写入持久化
    /// </summary>
    /// <param name="jobDetail">作业信息</param>
    /// <param name="trigger">作业触发器</param>
    /// <param name="behavior">作业持久化行为</param>
    public void Shorthand(JobDetail jobDetail, Trigger trigger, PersistenceBehavior behavior = PersistenceBehavior.Updated)
    {
        // 动态委托作业无需触发持久化操作
        if (jobDetail.DynamicExecuteAsync != null) return;

        // 设置更新时间
        var nowTime = Penetrates.GetNowTime();
        jobDetail.UpdatedTime = nowTime;
        if (trigger != null) trigger.UpdatedTime = nowTime;

        // 空检查
        if (Persistence == null) return;

        // 只有队列可持续入队才写入
        if (!_persistenceMessageQueue.IsAddingCompleted)
        {
            try
            {
                // 创建作业信息/触发器持久化上下文
                var context = trigger == null ?
                    new PersistenceContext(jobDetail, behavior)
                    : new PersistenceTriggerContext(jobDetail, trigger, behavior)
                    {
                        Mode = trigger.Mode
                    };

                _persistenceMessageQueue.Add(context);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
    }

    /// <summary>
    /// 创建作业处理程序实例
    /// </summary>
    /// <param name="serviceProvider">服务提供器</param>
    /// <param name="context"><see cref="JobFactoryContext"/> 上下文</param>
    /// <returns><see cref="IJob"/></returns>
    public IJob CreateJob(IServiceProvider serviceProvider, JobFactoryContext context)
    {
        var jobFactory = _serviceProvider.GetService<IJobFactory>();

        // 通过作业处理程序工厂创建
        var jobHandler = jobFactory?.CreateJob(serviceProvider, context);
        if (jobHandler != null) return jobHandler;

        jobHandler = ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, context.JobType) as IJob;

        return jobHandler;
    }

    /// <summary>
    /// GC 垃圾回收器回收处理
    /// </summary>
    /// <remarks>避免频繁 GC 回收</remarks>
    public void GCCollect()
    {
        var nowTime = DateTime.UtcNow;
        if ((LastGCCollectTime == null || (nowTime - LastGCCollectTime.Value).TotalMilliseconds > GC_COLLECT_INTERVAL_MILLISECONDS))
        {
            LastGCCollectTime = nowTime;

            // 通知 GC 垃圾回收器立即回收
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    /// <summary>
    /// 释放非托管资源
    /// </summary>
    public void Dispose()
    {
        // 标记作业持久化记录消息队列停止写入
        _persistenceMessageQueue.CompleteAdding();

        try
        {
            // 取消当前任务并释放作业调度器取消休眠 Token
            if (!_sleepCancellationTokenSource.IsCancellationRequested) _sleepCancellationTokenSource.Cancel();
            _sleepCancellationTokenSource.Dispose();

            // 设置 1.5秒的缓冲时间，避免还有消息没有完成持久化
            _processQueueTask?.Wait(1500);
        }
        catch (TaskCanceledException) { }
        catch (AggregateException ex) when (ex.InnerExceptions.Count == 1 && ex.InnerExceptions[0] is TaskCanceledException) { }
        catch { }
    }

    /// <summary>
    /// 记录作业触发器运行信息
    /// </summary>
    /// <param name="context">作业执行记录持久上下文</param>
    /// <returns><see cref="Task"/></returns>
    internal async Task RecordTimelineAsync(PersistenceExecutionRecordContext context)
    {
        try
        {
            // 作业触发记录通知
            if (Persistence is not null)
            {
                await Persistence.OnExecutionRecordAsync(context).ConfigureAwait(false);
            }

            // 调用事件委托
            OnExecutionRecord?.Invoke(this, new(context));
        }
        catch { }
    }

    /// <summary>
    /// 获取作业调度器总休眠时间
    /// </summary>
    /// <param name="startAt">起始时间</param>
    /// <returns><see cref="double"/></returns>
    private double? GetSleepMilliseconds(DateTime startAt)
    {
        // 空检查
        if (_schedulers.IsEmpty)
        {
            // 输出作业调度器休眠总时长和唤醒时间日志
            _logger.LogWarning("Schedule hosted service will sleep until it wakes up.");

            return null;
        }

        // 获取所有作业计划下一批执行时间
        var nextRunTimes = (GetJobs(active: true) as IEnumerable<Scheduler>)
            .SelectMany(u => u.Triggers.Values
                .Where(t => t.NextShouldRun(startAt))
                .Select(t => t.NextRunTime.Value));

        // 空检查
        if (!nextRunTimes.Any()) return null;

        // 获取最早触发的时间
        var earliestTriggerTime = nextRunTimes.Min();

        // 计算总休眠时间
        var sleepMilliseconds = (earliestTriggerTime - startAt).TotalMilliseconds;

        // 输出作业调度器休眠总时长和唤醒时间日志
        _logger.LogDebug("Schedule hosted service will sleep <{sleepMilliseconds}> milliseconds and be waked up at <{earliestTriggerTime}>.", sleepMilliseconds, earliestTriggerTime.ToFormatString());

        return sleepMilliseconds;
    }

    /// <summary>
    /// 监听作业计划变更并调用持久化方法
    /// </summary>
    /// <returns><see cref="Task"/></returns>
    private async Task ProcessQueueAsync()
    {
        foreach (var context in _persistenceMessageQueue.GetConsumingEnumerable())
        {
            try
            {
                // 作业触发器更改通知
                if (context is PersistenceTriggerContext triggerContext)
                {
                    await Persistence.OnTriggerChangedAsync(triggerContext).ConfigureAwait(false);
                }
                // 作业信息更改通知
                else
                {
                    await Persistence.OnChangedAsync(context).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                if (context is PersistenceTriggerContext triggerContext) _logger.LogError(ex, "Persistence of <{TriggerId}> trigger of <{JobId}> job failed.", triggerContext.TriggerId, triggerContext.JobId);
                else _logger.LogError(ex, "The JobDetail of <{JobId}> persist failed.", context.JobId);
            }
        }
    }

    /// <summary>
    /// 创建新的作业调度器取消休眠 Token
    /// </summary>
    private void CreateCancellationTokenSource()
    {
        _sleepCancellationTokenSource?.Dispose();

        // 初始化作业调度器休眠 Token
        _sleepCancellationTokenSource = new CancellationTokenSource();

        // 监听休眠被取消，并通知 GC 垃圾回收器回收
        _sleepCancellationTokenSource.Token.Register(() =>
        {
            //_logger.LogWarning("Schedule hosted service cancels hibernation.");

            // 通知 GC 垃圾回收器立即回收
            GCCollect();
        });
    }

    /// <summary>
    /// 内部获取作业
    /// </summary>
    /// <param name="jobId">作业 Id</param>
    /// <param name="scheduler">作业计划</param>
    /// <param name="outputLog">是否显示日志</param>
    /// <param name="group">作业组名称</param>
    /// <returns><see cref="ScheduleResult"/></returns>
    private ScheduleResult InternalTryGetJob(string jobId, out Scheduler scheduler, bool outputLog = false, string group = default)
    {
        // 空检查
        if (string.IsNullOrWhiteSpace(jobId))
        {
            // 输出日志
            if (outputLog) _logger.LogWarning("Empty identity scheduler.");

            scheduler = default;
            return ScheduleResult.NotIdentify;
        }

        // 查找作业
        var succeed = _schedulers.TryGetValue(jobId, out var originScheduler);

        // 检查作业组名称
        if (!succeed
            || (!string.IsNullOrWhiteSpace(group) && originScheduler.JobDetail.GroupName != group))
        {
            // 输出日志
            if (outputLog) _logger.LogWarning(message: "The scheduler of <{jobId}> is not found.", jobId);

            scheduler = default;
            return ScheduleResult.NotFound;
        }

        scheduler = originScheduler;
        return ScheduleResult.Succeed;
    }
}
