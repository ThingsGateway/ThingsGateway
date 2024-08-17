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

using ThingsGateway.ASPNetCore;
using ThingsGateway.ASPNetCore.SpecificationDocument;

namespace ThingsGateway;

/// <summary>
/// 标签文档排序/注释拦截器
/// </summary>
public class TagsOrderDocumentFilter : IDocumentFilter
{
    /// <summary>
    /// 配置拦截
    /// </summary>
    /// <param name="swaggerDoc"></param>
    /// <param name="context"></param>
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Tags = Penetrates.ControllerOrderCollection
            .Where(u => SpecificationDocumentBuilder.GetControllerGroups(u.Value.Item3).Any(c => c.Group == context.DocumentName))
            .OrderByDescending(u => u.Value.Item2)
            .ThenBy(u => u.Key)
            .Select(c => new OpenApiTag
            {
                Name = c.Value.Item1,
                Description = swaggerDoc.Tags.FirstOrDefault(m => m.Name == c.Key)?.Description
            }).ToList();
    }
}
