// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://kimdiego2098.github.io/
// QQ群：605534569
// ------------------------------------------------------------------------------

using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using ThingsGateway.ASPNetCore;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// 跨域中间件拓展
/// </summary>
public static class CorsAccessorApplicationBuilderExtensions
{
    /// <summary>
    /// 添加跨域中间件
    /// </summary>
    /// <param name="app"></param>
    /// <param name="corsPolicyBuilderHandler"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseCorsAccessor(this IApplicationBuilder app, Action<CorsPolicyBuilder> corsPolicyBuilderHandler = default)
    {
        // 获取选项
        var corsAccessorSettings = app.ApplicationServices.GetService<IOptions<CorsAccessorSettingsOptions>>().Value;

        // 判断是否启用 SignalR 跨域支持
        if (corsAccessorSettings.SignalRSupport == false)
        {
            // 配置跨域中间件
            _ = corsPolicyBuilderHandler == null
                   ? app.UseCors(corsAccessorSettings.PolicyName)
                   : app.UseCors(corsPolicyBuilderHandler);
        }
        else
        {
            // 配置跨域中间件
            app.UseCors(builder =>
            {
                // 设置跨域策略
                Penetrates.SetCorsPolicy(builder, corsAccessorSettings, true);

                // 添加自定义配置
                corsPolicyBuilderHandler?.Invoke(builder);
            });
        }

        return app;
    }
}
