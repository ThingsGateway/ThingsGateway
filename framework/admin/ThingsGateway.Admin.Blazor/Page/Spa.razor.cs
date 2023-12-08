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

using Microsoft.Extensions.DependencyInjection;

namespace ThingsGateway.Admin.Blazor;

/// <summary>
/// SPA
/// </summary>
public partial class Spa
{
    private readonly SpaPageInput _search = new();

    private IAppDataTable _datatable;

    [CascadingParameter]
    private MainLayout _mainLayout { get; set; }

    private async Task AddCallAsync(SpaAddInput input)
    {
        await _serviceScope.ServiceProvider.GetService<ISpaService>().AddAsync(input);
        await _mainLayout.StateHasChangedAsync();
    }

    private async Task DeleteCallAsync(IEnumerable<SysResource> input)
    {
        await _serviceScope.ServiceProvider.GetService<ISpaService>().DeleteAsync(input.Select(a => a.Id).ToArray());
        await _mainLayout.StateHasChangedAsync();
    }

    private async Task EditCallAsync(SpaEditInput input)
    {
        await _serviceScope.ServiceProvider.GetService<ISpaService>().EditAsync(input);
        await _mainLayout.StateHasChangedAsync();
    }

    private async Task<ISqlSugarPagedList<SysResource>> QueryCallAsync(SpaPageInput input)
    {
        return await _serviceScope.ServiceProvider.GetService<ISpaService>().PageAsync(input);
    }
}