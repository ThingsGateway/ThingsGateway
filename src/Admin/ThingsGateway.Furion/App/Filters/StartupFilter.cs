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
using Microsoft.AspNetCore.Http;

namespace ThingsGateway;

/// <summary>
/// 应用启动时自动注册中间件
/// </summary>
/// <remarks>
/// </remarks>
[SuppressSniffer]
public class StartupFilter : IStartupFilter
{
    /// <summary>
    /// 配置中间件
    /// </summary>
    /// <param name="next"></param>
    /// <returns></returns>
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            // 存储根服务
            InternalApp.RootServices ??= app.ApplicationServices;

            // 环境名
            var envName = App.HostEnvironment?.EnvironmentName ?? "Unknown";
            var version = $"{GetType().Assembly.GetName().Version}";

            // 设置响应报文头信息
            app.Use(async (context, next) =>
            {
                // 处理 WebSocket 请求
                if (context.IsWebSocketRequest()) await next.Invoke().ConfigureAwait(false);
                else
                {
                    // 输出当前环境标识
                    context.Response.Headers["environment"] = envName;

                    // 输出框架版本
                    context.Response.Headers[nameof(ThingsGateway)] = version;

                    // 执行下一个中间件
                    await next.Invoke().ConfigureAwait(false);

                    // 解决刷新 Token 时间和 Token 时间相近问题
                    if (!context.Response.HasStarted
                        && context.Response.StatusCode == StatusCodes.Status401Unauthorized
                        && context.Response.Headers.ContainsKey("access-token")
                        && context.Response.Headers.ContainsKey("x-access-token"))
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    }

                }
            });

            // 调用默认中间件
            app.UseApp();


            // 调用启动层的 Startup
            next(app);
        };
    }

}
