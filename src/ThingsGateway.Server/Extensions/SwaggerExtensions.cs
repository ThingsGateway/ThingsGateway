
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------



using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace ThingsGateway.Server;

/// <summary>
/// 规范化文档安全配置
/// </summary>
public sealed class SpecificationOpenApiSecurityScheme : OpenApiSecurityScheme
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public SpecificationOpenApiSecurityScheme()
    {
    }

    /// <summary>
    /// 唯一Id
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// 安全需求
    /// </summary>
    public SpecificationOpenApiSecurityRequirementItem Requirement { get; set; }
}

/// <summary>
/// 安全定义需求子项
/// </summary>
public sealed class SpecificationOpenApiSecurityRequirementItem
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public SpecificationOpenApiSecurityRequirementItem()
    {
        Accesses = System.Array.Empty<string>();
    }

    /// <summary>
    /// 安全Schema
    /// </summary>
    public OpenApiSecurityScheme Scheme { get; set; }

    /// <summary>
    /// 权限
    /// </summary>
    public string[] Accesses { get; set; }
}

/// <inheritdoc/>
internal static class SwaggerExtensions
{
    /// <summary>
    /// 配置JWT授权
    /// </summary>
    /// <param name="swaggerGenOptions">Swagger 生成器配置</param>
    internal static void ConfigureSecurities(this SwaggerGenOptions swaggerGenOptions)
    {
        var openApiSecurityRequirement = new OpenApiSecurityRequirement();
        var SecurityDefinitions = new SpecificationOpenApiSecurityScheme[]
            {
                    new SpecificationOpenApiSecurityScheme
                    {
                        Id="Bearer",
                        Type= SecuritySchemeType.Http,
                        Name="Authorization",
                        Description="JWT Authorization header using the Bearer scheme.",
                        BearerFormat="JWT",
                        Scheme="bearer",
                        In= ParameterLocation.Header,
                        Requirement=new SpecificationOpenApiSecurityRequirementItem
                        {
                            Scheme=new OpenApiSecurityScheme
                            {
                                Reference=new OpenApiReference
                                {
                                    Id="Bearer",
                                    Type= ReferenceType.SecurityScheme
                                }
                            },
                            Accesses=Array.Empty<string>()
                        }
                    }
            };
        // 生成安全定义
        foreach (var securityDefinition in SecurityDefinitions)
        {
            // Id 必须定义
            if (string.IsNullOrWhiteSpace(securityDefinition.Id)
                || swaggerGenOptions.SwaggerGeneratorOptions.SecuritySchemes.ContainsKey(securityDefinition.Id)) continue;

            // 添加安全定义
            var openApiSecurityScheme = securityDefinition as OpenApiSecurityScheme;
            swaggerGenOptions.AddSecurityDefinition(securityDefinition.Id, openApiSecurityScheme);

            // 添加安全需求
            var securityRequirement = securityDefinition.Requirement;

            // C# 9.0 模式匹配新语法
            if (securityRequirement is { Scheme.Reference: not null })
            {
                securityRequirement.Scheme.Reference.Id ??= securityDefinition.Id;
                openApiSecurityRequirement.Add(securityRequirement.Scheme, securityRequirement.Accesses);
            }
        }

        // 添加安全需求
        if (openApiSecurityRequirement.Count > 0)
        {
            swaggerGenOptions.AddSecurityRequirement(openApiSecurityRequirement);
        }
    }
}