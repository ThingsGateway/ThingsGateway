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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ThingsGateway.Schedule;

/// <summary>
/// 调度作业服务静态类
/// </summary>
[SuppressSniffer]
public static class ScheduleServe
{
    /// <summary>
    /// 初始化 Schedule 服务
    /// </summary>
    /// <remarks>仅限不能依赖注入的服务使用</remarks>
    /// <param name="configureOptionsBuilder">作业调度器配置选项构建器委托</param>
    /// <returns><see cref="IDisposable"/></returns>
    public static IDisposable Run(Action<ScheduleOptionsBuilder> configureOptionsBuilder)
    {
        // 创建初始作业调度器配置选项构建器
        var scheduleOptionsBuilder = new ScheduleOptionsBuilder();
        configureOptionsBuilder.Invoke(scheduleOptionsBuilder);

        return Run(scheduleOptionsBuilder);
    }

    /// <summary>
    /// 初始化 Schedule 服务
    /// </summary>
    /// <remarks>仅限不能依赖注入的服务使用</remarks>
    /// <param name="scheduleOptionsBuilder">作业调度器配置选项构建器</param>
    public static IDisposable Run(ScheduleOptionsBuilder scheduleOptionsBuilder = default)
    {
        // 创建服务集合
        var services = new ServiceCollection();

        // 注册初始服务
        services.AddConsoleFormatter();

        // 注册 Schedule 服务
        services.AddSchedule(scheduleOptionsBuilder);

        // 构建服务并解析 ScheduleHostedService
        var serviceProvider = services.BuildServiceProvider();
        var scheduleHostedService = serviceProvider.GetServices<IHostedService>()
            .Single(s => s.GetType().Name == nameof(ScheduleHostedService));

        // 启动服务
        var cancellationTokenSource = new CancellationTokenSource();
        scheduleHostedService.StartAsync(cancellationTokenSource.Token);

        return serviceProvider;
    }
}

/// <summary>
/// 作业调度器静态类
/// </summary>
[SuppressSniffer]
public static class Schedular
{
    /// <summary>
    /// 获取作业计划工厂
    /// </summary>
    /// <returns><see cref="ISchedulerFactory"/></returns>
    public static ISchedulerFactory GetFactory()
    {
        return App.GetService<ISchedulerFactory>(App.RootServices);
    }

    /// <summary>
    /// 获取作业
    /// </summary>
    /// <param name="jobId">作业 Id</param>
    /// <returns><see cref="IScheduler"/></returns>
    public static IScheduler GetJob(string jobId)
    {
        return GetFactory().GetJob(jobId);
    }

    /// <summary>
    /// 序列化对象
    /// </summary>
    /// <remarks>主要用于作业触发器参数，作业信息额外数据序列化</remarks>
    /// <param name="obj">对象</param>
    /// <returns><see cref="string"/></returns>
    public static string Serialize(object obj)
    {
        return Penetrates.Serialize(obj);
    }

    /// <summary>
    /// 反序列化对象
    /// </summary>
    /// <remarks>主要用于作业触发器参数，作业信息额外数据序列化</remarks>
    /// <param name="json">JSON 字符串</param>
    /// <returns>T</returns>
    public static T Deserialize<T>(string json)
    {
        return Penetrates.Deserialize<T>(json);
    }

}
