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
/// 规范化文档开放接口信息
/// </summary>
[SuppressSniffer]
public sealed class SpecificationOpenApiInfo : OpenApiInfo
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public SpecificationOpenApiInfo()
    {
        Version = "1.0.0";
    }

    /// <summary>
    /// 分组私有字段
    /// </summary>
    private string _group;

    /// <summary>
    /// 所属组
    /// </summary>
    public string Group
    {
        get => _group;
        set
        {
            _group = value;
            //Title ??= string.Join(' ', _group.SplitCamelCase());
            Title ??= _group;
        }
    }

    /// <summary>
    /// 排序
    /// </summary>
    public int? Order { get; set; }

    /// <summary>
    /// 是否可见
    /// </summary>
    public bool? Visible { get; set; }

    /// <summary>
    /// 路由模板
    /// </summary>
    public string RouteTemplate { get; internal set; }
}