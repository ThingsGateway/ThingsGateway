// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

// 版权归百小僧及百签科技（广东）有限公司所有。

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using Swashbuckle.AspNetCore.SwaggerGen;

using ThingsGateway;
using ThingsGateway.ASPNetCore.SpecificationDocument;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 规范化接口服务拓展类
/// </summary>
public static class SpecificationDocumentServiceCollectionExtensions
{
    /// <summary>
    /// 添加规范化文档服务
    /// </summary>
    /// <param name="mvcBuilder">Mvc 构建器</param>
    /// <param name="configure">自定义配置</param>
    /// <returns>服务集合</returns>
    public static IMvcBuilder AddSpecificationDocuments(this IMvcBuilder mvcBuilder, Action<SwaggerGenOptions> configure = default)
    {
        mvcBuilder.Services.AddSpecificationDocuments(configure);

        return mvcBuilder;
    }

    /// <summary>
    /// 添加规范化文档服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configure">自定义配置</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddSpecificationDocuments(this IServiceCollection services, Action<SwaggerGenOptions> configure = default)
    {
        // 解决服务重复注册问题
        if (services.Any(u => u.ServiceType == typeof(IConfigureOptions<SchemaGeneratorOptions>)))
        {
            return services;
        }

        services.AddOptions();
        // 配置
        services.TryAddSingleton<IConfigureOptions<SpecificationDocumentSettingsOptions>, Microsoft.Extensions.DependencyInjection.ConfigureOptions<SpecificationDocumentSettingsOptions>>();

        services.AddEndpointsApiExplorer();

        // 添加Swagger生成器服务
        services.AddSwaggerGen(options => SpecificationDocumentBuilder.BuildGen(options, configure));


        return services;
    }

}
