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

using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerUI;

using ThingsGateway.ConfigurableOptions;
using ThingsGateway.Reflection;

namespace ThingsGateway.SpecificationDocument;

/// <summary>
/// 规范化文档配置选项
/// </summary>
public sealed class SpecificationDocumentSettingsOptions : IConfigurableOptions<SpecificationDocumentSettingsOptions>
{
    /// <summary>
    /// 文档标题
    /// </summary>
    public string DocumentTitle { get; set; }

    /// <summary>
    /// 默认分组名
    /// </summary>
    public string DefaultGroupName { get; set; }

    /// <summary>
    /// 启用授权支持
    /// </summary>
    public bool? EnableAuthorized { get; set; }

    /// <summary>
    /// 格式化为V2版本
    /// </summary>
    public bool? FormatAsV2 { get; set; }

    /// <summary>
    /// 配置规范化文档地址
    /// </summary>
    public string RoutePrefix { get; set; }

    /// <summary>
    /// 文档展开设置
    /// </summary>
    public DocExpansion? DocExpansionState { get; set; }

    /// <summary>
    /// XML 描述文件
    /// </summary>
    public string[] XmlComments { get; set; }

    /// <summary>
    /// 是否自动加载 Xml 注释文件
    /// </summary>
    public bool? EnableXmlComments { get; set; }

    /// <summary>
    /// 分组信息
    /// </summary>
    public SpecificationOpenApiInfo[] GroupOpenApiInfos { get; set; }

    /// <summary>
    /// 安全定义
    /// </summary>
    public SpecificationOpenApiSecurityScheme[] SecurityDefinitions { get; set; }

    /// <summary>
    /// 配置 Servers
    /// </summary>
    public OpenApiServer[] Servers { get; set; }

    /// <summary>
    /// 隐藏 Servers
    /// </summary>
    public bool? HideServers { get; set; }

    /// <summary>
    /// 默认 swagger.json 路由模板
    /// </summary>
    public string RouteTemplate { get; set; }

    /// <summary>
    /// 配置安装第三方包的分组名
    /// </summary>
    public string[] PackagesGroups { get; set; }

    /// <summary>
    /// 启用枚举 Schema 筛选器
    /// </summary>
    public bool? EnableEnumSchemaFilter { get; set; }

    /// <summary>
    /// 启用标签排序筛选器
    /// </summary>
    public bool? EnableTagsOrderDocumentFilter { get; set; }

    /// <summary>
    /// 服务目录（修正 IIS 创建 Application 问题）
    /// </summary>
    public string ServerDir { get; set; }

    /// <summary>
    /// 配置规范化文档登录信息
    /// </summary>
    public SpecificationLoginInfo LoginInfo { get; set; }

    /// <summary>
    /// 启用 All Groups 功能
    /// </summary>
    public bool? EnableAllGroups { get; set; }

    /// <summary>
    /// 枚举类型生成值类型
    /// </summary>
    public bool? EnumToNumber { get; set; }

    /// <summary>
    /// 后期配置
    /// </summary>
    /// <param name="options"></param>
    /// <param name="configuration"></param>
    public void PostConfigure(SpecificationDocumentSettingsOptions options, IConfiguration configuration)
    {
        options.DocumentTitle ??= "Specification Api Document";
        options.DefaultGroupName ??= "Default";
        options.FormatAsV2 ??= false;
        //options.RoutePrefix ??= "api";    // 可以通过 UseInject() 配置，所以注释
        options.DocExpansionState ??= DocExpansion.List;

        // 加载项目注册和模块化/插件注释
        EnableXmlComments ??= true;
        if (EnableXmlComments == true)
        {
            var frameworkPackageName = Reflect.GetAssemblyName(GetType());
            var projectXmlComments = App.Assemblies.Where(u => u.GetName().Name != frameworkPackageName).Select(t => t.GetName().Name);
            var externalXmlComments = App.ExternalAssemblies.Any() ? App.PathOfExternalAssemblies.Select(u => u.EndsWith(".dll") ? u[0..^4] : u) : Array.Empty<string>();
            XmlComments ??= projectXmlComments.Concat(externalXmlComments).ToArray();
        }

        GroupOpenApiInfos ??= new SpecificationOpenApiInfo[]
        {
                new SpecificationOpenApiInfo()
                {
                    Group=options.DefaultGroupName
                }
        };

        EnableAuthorized ??= true;
        if (EnableAuthorized == true)
        {
            SecurityDefinitions ??= new SpecificationOpenApiSecurityScheme[]
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
        }

        Servers ??= Array.Empty<OpenApiServer>();
        HideServers ??= true;
        RouteTemplate ??= "swagger/{documentName}/swagger.json";
        PackagesGroups ??= Array.Empty<string>();
        EnableEnumSchemaFilter ??= true;
        EnableTagsOrderDocumentFilter ??= true;
        EnableAllGroups ??= false;
        EnumToNumber ??= false;
    }
}