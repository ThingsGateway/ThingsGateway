// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://kimdiego2098.github.io/
// QQ群：605534569
// ------------------------------------------------------------------------------

// 版权归百小僧及百签科技（广东）有限公司所有。

using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerUI;

using ThingsGateway.ASPNetCore.SpecificationDocument;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// 规范化文档中间件拓展
/// </summary>
public static class SpecificationDocumentApplicationBuilderExtensions
{
    /// <summary>
    /// 添加规范化文档中间件
    /// </summary>
    /// <param name="app"></param>
    /// <param name="routePrefix"></param>
    /// <param name="configureSwagger"></param>
    /// <param name="configureSwaggerUI"></param>
    /// <param name="withProxy">解决 Swagger 被代理问题</param>
    /// <returns></returns>
    public static IApplicationBuilder UseSpecificationDocuments(this IApplicationBuilder app
        , string routePrefix = default
        , Action<SwaggerOptions> configureSwagger = default
        , Action<SwaggerUIOptions> configureSwaggerUI = default
        , bool withProxy = false)
    {

        // 配置 Swagger 全局参数
        app.UseSwagger(options => SpecificationDocumentBuilder.Build(options, configureSwagger));

        // 配置 Swagger UI 参数
        app.UseSwaggerUI(options => SpecificationDocumentBuilder.BuildUI(options, routePrefix, configureSwaggerUI, withProxy));

        return app;
    }
}
