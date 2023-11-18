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

using Furion.Schedule;

using Microsoft.Extensions.DependencyInjection;

namespace ThingsGateway.Admin.Application;

/// <inheritdoc cref="IJobPersistence"/>
public class JobPersistence : IJobPersistence
{
    private readonly IServiceScope _serviceScope;

    /// <inheritdoc/>
    public JobPersistence(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScope = serviceScopeFactory.CreateScope();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _serviceScope?.Dispose();
    }

    /// <inheritdoc/>
    public void OnChanged(PersistenceContext context)
    {

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

    /// <inheritdoc/>
    public void OnTriggerChanged(PersistenceTriggerContext context)
    {

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
}