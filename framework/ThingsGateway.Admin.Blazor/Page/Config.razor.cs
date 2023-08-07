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

using ThingsGateway.Admin.Application;
using ThingsGateway.Admin.Blazor.Core;
using ThingsGateway.Admin.Core;

namespace ThingsGateway.Admin.Blazor;

/// <summary>
/// 系统配置页面
/// </summary>
public partial class Config
{
    private IAppDataTable _datatable;
    private List<SysConfig> _sysConfig = new();
    private readonly ConfigPageInput search = new();
    StringNumber tab;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    protected override async Task OnParametersSetAsync()
    {
        _sysConfig = await App.GetService<IConfigService>().GetListByCategoryAsync(ConfigConst.SYS_CONFIGBASEDEFAULT);
        await base.OnParametersSetAsync();
    }
    [CascadingParameter]
    MainLayout MainLayout { get; set; }
    private Task AddCallAsync(ConfigAddInput input)
    {
        return ConfigService.AddAsync(input);
    }

    private Task DeleteCallAsync(IEnumerable<SysConfig> sysConfigs)
    {
        return ConfigService.DeleteAsync(sysConfigs.Select(a => a.Id).ToArray());
    }
    private Task EditCallAsync(ConfigEditInput sysConfigs)
    {
        return ConfigService.EditAsync(sysConfigs);
    }

    private async Task OnSaveAsync()
    {
        await ConfigService.EditBatchAsync(_sysConfig);
        await MainLayout.StateHasChangedAsync();
        await PopupService.EnqueueSnackbarAsync("成功", AlertTypes.Success);
    }
    private Task<SqlSugarPagedList<SysConfig>> QueryCallAsync(ConfigPageInput input)
    {
        return ConfigService.PageAsync(input);
    }
}