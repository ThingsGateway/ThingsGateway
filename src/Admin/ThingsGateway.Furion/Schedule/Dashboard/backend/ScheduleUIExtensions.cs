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

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

using ThingsGateway.Schedule;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Schedule 模块 UI 中间件拓展
/// </summary>
[SuppressSniffer]
public static class ScheduleUIExtensions
{
    /// <summary>
    /// 添加 Schedule 模块 UI 中间件
    /// </summary>
    /// <param name="app"><see cref="IApplicationBuilder"/></param>
    /// <param name="configureAction">Schedule 模块 UI 配置选项委托</param>
    /// <returns><see cref="IApplicationBuilder"/></returns>
    public static IApplicationBuilder UseScheduleUI(this IApplicationBuilder app, Action<ScheduleUIOptions> configureAction = default)
    {
        var scheduleUIOptions = new ScheduleUIOptions();
        configureAction?.Invoke(scheduleUIOptions);

        return app.UseScheduleUI(scheduleUIOptions);
    }

    /// <summary>
    /// 添加 Schedule 模块 UI 中间件
    /// </summary>
    /// <param name="app"><see cref="IApplicationBuilder"/></param>
    /// <param name="options">Schedule 模块 UI 配置选项</param>
    /// <returns><see cref="IApplicationBuilder"/></returns>
    public static IApplicationBuilder UseScheduleUI(this IApplicationBuilder app, ScheduleUIOptions options)
    {
        // 判断是否配置了定时任务服务
        if (app.ApplicationServices.GetService<ISchedulerFactory>() == null) return app;

        // 初始化默认值
        options ??= new ScheduleUIOptions();

        // 生产环境关闭
        if (options.DisableOnProduction
            && app.ApplicationServices.GetRequiredService<IWebHostEnvironment>().IsProduction()) return app;

        // 如果入口地址为空则不启动看板
        if (string.IsNullOrWhiteSpace(options.RequestPath)) return app;

        // 修复无效的入口地址
        options.RequestPath = $"/{options.RequestPath.TrimStart('/').TrimEnd('/')}";

        // 注册 Schedule 中间件
        app.UseMiddleware<ScheduleUIMiddleware>(options);

        // 获取当前类型所在程序集
        var currentAssembly = typeof(ScheduleUIExtensions).Assembly;

        // 注册嵌入式文件服务器
        app.UseFileServer(new FileServerOptions
        {
            FileProvider = new EmbeddedFileProvider(currentAssembly, $"{currentAssembly.GetName().Name}.Schedule.Dashboard.frontend"),
            RequestPath = options.RequestPath,
            EnableDirectoryBrowsing = options.EnableDirectoryBrowsing
        });

        return app;
    }
}