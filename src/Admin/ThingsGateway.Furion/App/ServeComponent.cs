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

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System.Text.Encodings.Web;

namespace ThingsGateway.Components;

/// <summary>
/// Serve 组件应用服务组件
/// </summary>
[SuppressSniffer]
public sealed class ServeServiceComponent : IServiceComponent
{
    /// <summary>
    /// 装载服务
    /// </summary>
    /// <param name="services"></param>
    /// <param name="componentContext"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void Load(IServiceCollection services, ComponentContext componentContext)
    {
        // 控制台日志美化
        services.AddConsoleFormatter();

        // 配置跨域
        services.AddCorsAccessor();

        // 控制器和规范化结果
        services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
                })
                .AddInjectWithUnifyResult();
    }
}

/// <summary>
/// Serve 组件应用中间件组件
/// </summary>
[SuppressSniffer]
public sealed class ServeApplicationComponent : IApplicationComponent
{
    /// <summary>
    /// 装载中间件
    /// </summary>
    /// <param name="app"></param>
    /// <param name="env"></param>
    /// <param name="componentContext"></param>
    public void Load(IApplicationBuilder app, IWebHostEnvironment env, ComponentContext componentContext)
    {
        // 配置错误页
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // 401，403 规范化结果
        app.UseUnifyResultStatusCodes();

        // 配置静态
        app.UseStaticFiles();

        // 注册定时任务 UI
        app.UseScheduleUI();

        // 配置路由
        app.UseRouting();

        // 配置跨域
        app.UseCorsAccessor();

        // 配置授权
        app.UseAuthentication();
        app.UseAuthorization();

        // 框架基础配置
        app.UseInject(string.Empty);

        // 配置路由
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}