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

using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace ThingsGateway.SpecificationDocument;

/// <summary>
/// 修正 规范化文档 object schema，统一显示为 any
/// </summary>
/// <remarks>相关 issue：https://github.com/swagger-api/swagger-codegen-generators/issues/692 </remarks>
[SuppressSniffer]
public class AnySchemaFilter : ISchemaFilter
{
    /// <summary>
    /// 实现过滤器方法
    /// </summary>
    /// <param name="model"></param>
    /// <param name="context"></param>
    public void Apply(OpenApiSchema model, SchemaFilterContext context)
    {
        var type = context.Type;

        if (type == typeof(object))
        {
            model.AdditionalPropertiesAllowed = false;
        }
    }
}