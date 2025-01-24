//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using System.ComponentModel.DataAnnotations;

using ThingsGateway.Extension.Generic;

namespace ThingsGateway.Admin.Application;

public class ResourceTableSearchModel : ITableSearchModel
{
    public string? Href { get; set; }

    /// <summary>
    /// 模块ID，单独搜索
    /// </summary>
    [Required]
    public long Module { get; set; }

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

public class GrantResourceInput
{
    public long Id { get; set; }
    public string? Href { get; set; }
    public int SortCode { get; set; }

    /// <summary>
    /// 分类
    ///</summary>
    public ResourceCategoryEnum Category { get; set; } = ResourceCategoryEnum.Menu;

    /// <summary>
    /// 模块ID，单独搜索
    /// </summary>
    [Required]
    public long Module { get; set; }

    public virtual string? Title { get; set; }

    public DataScopeEnum ScopeCategory { get; set; }
    public List<long> ScopeDefineOrgIdList { get; set; }
}
