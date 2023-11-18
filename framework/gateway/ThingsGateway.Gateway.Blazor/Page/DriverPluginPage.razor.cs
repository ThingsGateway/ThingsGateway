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

using Microsoft.AspNetCore.Components;

namespace ThingsGateway.Gateway.Blazor;
/// <summary>
/// 插件管理页面
/// </summary>
public partial class DriverPluginPage
{
    private readonly DriverPluginPageInput _search = new();
    private IAppDataTable _datatable;
    [Inject]
    DriverPluginService _driverPluginService { get; set; }
    private async Task AddCallAsync(DriverPluginAddInput input)
    {
        _driverPluginService.TryAddDriver(input);
        await Task.CompletedTask;
    }

    private async Task<ISqlSugarPagedList<DriverPlugin>> QueryCallAsync(DriverPluginPageInput input)
    {
        await Task.CompletedTask;
        var data = _driverPluginService.Page(input);
        return data;
    }
}