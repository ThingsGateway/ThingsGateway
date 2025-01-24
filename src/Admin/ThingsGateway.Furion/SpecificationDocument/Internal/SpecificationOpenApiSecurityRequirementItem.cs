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

namespace ThingsGateway.SpecificationDocument;

/// <summary>
/// 安全定义需求子项
/// </summary>
[SuppressSniffer]
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