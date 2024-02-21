//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Mapster;

using Masa.Blazor;

using Microsoft.Extensions.DependencyInjection;

using ThingsGateway.Core;

namespace ThingsGateway.Admin.Blazor;

/// <summary>
/// 系统配置页面
/// </summary>
public partial class Config
{
    private readonly ConfigPageInput _search = new();
    private IAppDataTable _datatable;
    private List<SysConfig> _loginConfig = new();
    private List<SysConfig> _passwordConfig = new();

    [CascadingParameter(Name = "MainLayout")]
    private IMainLayout MainLayout { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    protected override async Task OnParametersSetAsync()
    {
        _loginConfig = await _serviceScope.ServiceProvider.GetService<IConfigService>().GetListByCategoryAsync(CateGoryConst.LOGIN_POLICY);
        _passwordConfig = await _serviceScope.ServiceProvider.GetService<IConfigService>().GetListByCategoryAsync(CateGoryConst.Config_PWD_POLICY);
        await base.OnParametersSetAsync();
    }

    private async Task AddCallAsync(ConfigAddInput input)
    {
        await _serviceScope.ServiceProvider.GetService<IConfigService>().AddAsync(input);
    }

    private async Task DeleteCallAsync(IEnumerable<SysConfig> sysConfigs)
    {
        await _serviceScope.ServiceProvider.GetService<IConfigService>().DeleteAsync(sysConfigs.Adapt<List<BaseIdInput>>());
    }

    private async Task EditCallAsync(ConfigEditInput sysConfigs)
    {
        await _serviceScope.ServiceProvider.GetService<IConfigService>().EditAsync(sysConfigs);
    }

    private async Task loginOnSaveAsync()
    {
        await _serviceScope.ServiceProvider.GetService<IConfigService>().EditBatchAsync(_loginConfig);
        await MainLayout.StateHasChangedAsync();
        await PopupService.EnqueueSnackbarAsync(AppService.I18n.T("成功"), AlertTypes.Success);
    }

    private async Task passwordOnSaveAsync()
    {
        await _serviceScope.ServiceProvider.GetService<IConfigService>().EditBatchAsync(_passwordConfig);
        await MainLayout.StateHasChangedAsync();
        await PopupService.EnqueueSnackbarAsync(AppService.I18n.T("成功"), AlertTypes.Success);
    }

    private async Task<SqlSugarPagedList<SysConfig>> QueryCallAsync(ConfigPageInput input)
    {
        return await _serviceScope.ServiceProvider.GetService<IConfigService>().PageAsync(input);
    }
}