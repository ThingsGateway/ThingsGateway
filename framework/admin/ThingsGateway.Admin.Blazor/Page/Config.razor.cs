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

using Masa.Blazor;

using Microsoft.Extensions.DependencyInjection;

namespace ThingsGateway.Admin.Blazor;

/// <summary>
/// 系统配置页面
/// </summary>
public partial class Config
{
    private readonly ConfigPageInput _search = new();
    private IAppDataTable _datatable;
    private List<SysConfig> _sysConfig = new();
    private StringNumber _tabNumber;
    [CascadingParameter]
    private MainLayout _mainLayout { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    protected override async Task OnParametersSetAsync()
    {
        _sysConfig = await _serviceScope.ServiceProvider.GetService<IConfigService>().GetListByCategoryAsync(ConfigConst.SYS_CONFIGBASEDEFAULT);
        await base.OnParametersSetAsync();
    }
    private async Task AddCallAsync(ConfigAddInput input)
    {
        await _serviceScope.ServiceProvider.GetService<ConfigService>().AddAsync(input);
    }

    private async Task DeleteCallAsync(IEnumerable<SysConfig> sysConfigs)
    {
        await _serviceScope.ServiceProvider.GetService<ConfigService>().DeleteAsync(sysConfigs.Select(a => a.Id).ToArray());
    }
    private async Task EditCallAsync(ConfigEditInput sysConfigs)
    {
        await _serviceScope.ServiceProvider.GetService<ConfigService>().EditAsync(sysConfigs);
    }

    private async Task OnSaveAsync()
    {
        await _serviceScope.ServiceProvider.GetService<ConfigService>().EditBatchAsync(_sysConfig);
        await _mainLayout.StateHasChangedAsync();
        await PopupService.EnqueueSnackbarAsync("成功", AlertTypes.Success);
    }
    private async Task<ISqlSugarPagedList<SysConfig>> QueryCallAsync(ConfigPageInput input)
    {
        return await _serviceScope.ServiceProvider.GetService<ConfigService>().PageAsync(input);
    }
}