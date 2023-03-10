using Furion.Schedule;

namespace ThingsGateway.Web.Core;

public class JobPersistence : IJobPersistence
{
    private readonly IServiceScope _serviceScope;

    public JobPersistence(IServiceProvider serviceProvider)
    {
        _serviceScope = serviceProvider.CreateScope();
    }

    /// <summary>
    /// 作业调度服务启动时
    /// </summary>
    /// <returns></returns>
    public IEnumerable<SchedulerBuilder> Preload()
    {
        // 获取所有定义的作业
        var allJobs = App.EffectiveTypes.ScanToBuilders().ToList();
        return allJobs;
    }

    /// <summary>
    /// 作业计划初始化通知
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public SchedulerBuilder OnLoading(SchedulerBuilder builder)
    {
        return builder;
    }

    public void Dispose()
    {
        _serviceScope?.Dispose();
    }

    public void OnChanged(PersistenceContext context)
    {

    }

    public void OnTriggerChanged(PersistenceTriggerContext context)
    {

    }
}