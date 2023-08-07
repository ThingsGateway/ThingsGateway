#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using ThingsGateway.Admin.Application;
using ThingsGateway.Admin.Blazor.Core;
using ThingsGateway.Admin.Core;

namespace ThingsGateway.Admin.Blazor;
/// <summary>
/// SPA
/// </summary>
public partial class Spa
{
    private readonly SpaPageInput search = new();
    private IAppDataTable _datatable;
    [CascadingParameter]
    MainLayout MainLayout { get; set; }

    private async Task AddCallAsync(SpaAddInput input)
    {
        await SpaService.AddAsync(input);
        await MainLayout.StateHasChangedAsync();
    }
    private async Task DeleteCallAsync(IEnumerable<SysResource> input)
    {
        await SpaService.DeleteAsync(input.Select(a => a.Id).ToArray());
        await MainLayout.StateHasChangedAsync();
    }

    private async Task EditCallAsync(SpaEditInput input)
    {
        await SpaService.EditAsync(input);
        await MainLayout.StateHasChangedAsync();
    }

    private Task<SqlSugarPagedList<SysResource>> QueryCallAsync(SpaPageInput input)
    {
        return SpaService.PageAsync(input);
    }
}