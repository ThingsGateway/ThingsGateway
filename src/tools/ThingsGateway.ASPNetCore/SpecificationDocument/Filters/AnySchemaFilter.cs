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

using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace ThingsGateway;

/// <summary>
/// 修正 规范化文档 object schema，统一显示为 any
/// </summary>
/// <remarks>相关 issue：https://github.com/swagger-api/swagger-codegen-generators/issues/692 </remarks>
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
