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
using Microsoft.AspNetCore.Http;

namespace ThingsGateway.AspNetCore.Extensions;

/// <summary>
///     <see cref="IApplicationBuilder" /> 拓展类
/// </summary>
public static class IApplicationBuilderExtensions
{
    /// <summary>
    ///     启用请求正文缓存
    /// </summary>
    /// <remarks>
    ///     <para>支持 <c>HttpRequest.Body</c> 重复读取。</para>
    ///     <para>https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/use-http-context?view=aspnetcore-8.0#enable-request-body-buffering</para>
    /// </remarks>
    /// <param name="app">
    ///     <see cref="IApplicationBuilder" />
    /// </param>
    /// <returns>
    ///     <see cref="IApplicationBuilder" />
    /// </returns>
    public static IApplicationBuilder UseEnableBuffering(this IApplicationBuilder app) =>
        app.Use(async (context, next) =>
        {
            context.Request.EnableBuffering();
            context.Request.Body.Position = 0;

            await next.Invoke().ConfigureAwait(false);
        });
}