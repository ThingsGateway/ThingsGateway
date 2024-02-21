//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Admin.Blazor;
using ThingsGateway.Core;
using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Gateway.Blazor;

/// <summary>
/// PluginPage
/// </summary>
public partial class PluginPage
{
    private readonly PluginPageInput _search = new();
    private IAppDataTable _datatable;

    [Inject]
    private IPluginService PluginService { get; set; }

    private async Task AddCallAsync(PluginAddInput input)
    {
        await PluginService.AddAsync(input);
    }

    private async Task<SqlSugarPagedList<PluginOutput>> QueryCallAsync(PluginPageInput input)
    {
        await Task.CompletedTask;
        var data = PluginService.Page(input);
        return data;
    }

    private async Task Remove()
    {
        PluginService.Remove();
        await MainLayout.StateHasChangedAsync();
    }

    [CascadingParameter(Name = "MainLayout")]
    private IMainLayout MainLayout { get; set; }
}