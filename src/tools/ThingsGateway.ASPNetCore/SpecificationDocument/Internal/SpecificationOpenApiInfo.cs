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

using Microsoft.OpenApi.Models;

namespace ThingsGateway;

/// <summary>
/// 规范化文档开放接口信息
/// </summary>
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
