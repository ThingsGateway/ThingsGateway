
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------




using BootstrapBlazor.Components;

using System.ComponentModel.DataAnnotations;

using ThingsGateway.Core.Extension;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 操作日志分页输入
/// </summary>
public class ResourceSearchInput : ITableSearchModel
{
    /// <summary>
    /// 模块ID，单独搜索
    /// </summary>
    [Required]
    public long Module { get; set; }

    public string? Href { get; set; }

    public virtual string? Title { get; set; }

    /// <inheritdoc/>
    public IEnumerable<IFilterAction> GetSearches()
    {
        var ret = new List<IFilterAction>();
        ret.AddIF(!string.IsNullOrEmpty(Href), () => new SearchFilterAction(nameof(SysResource.Href), Href));
        ret.AddIF(!string.IsNullOrEmpty(Title), () => new SearchFilterAction(nameof(SysResource.Title), Title));
        return ret;
    }

    /// <inheritdoc/>
    public void Reset()
    {
        Module = ResourceConst.SystemId;//系统管理ModuleID
        Href = null;
        Title = null;
    }
}