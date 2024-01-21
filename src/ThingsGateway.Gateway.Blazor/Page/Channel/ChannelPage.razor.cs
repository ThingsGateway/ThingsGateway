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

using Mapster;

using Masa.Blazor;

using Microsoft.Extensions.DependencyInjection;

using ThingsGateway.Admin.Blazor;
using ThingsGateway.Core;
using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Gateway.Blazor;

public partial class ChannelPage
{
    private readonly ChannelPageInput _search = new();
    private IAppDataTable _datatable;

    [CascadingParameter(Name = "MainLayout")]
    private IMainLayout MainLayout { get; set; }

    private async Task AddCallAsync(ChannelAddInput input)
    {
        await _serviceScope.ServiceProvider.GetService<IChannelService>().AddAsync(input);
        await MainLayout.StateHasChangedAsync();
    }

    private async Task DeleteAllAsync()
    {
        var confirm = await PopupService.ConfirmAsync(AppService.I18n.T("清空"), AppService.I18n.T("确定?"), AlertTypes.Warning);
        if (confirm)
        {
            await _serviceScope.ServiceProvider.GetService<IChannelService>().ClearAsync();
            await MainLayout.StateHasChangedAsync();
            await _datatable.QueryClickAsync();
            await PopupService.EnqueueSnackbarAsync(AppService.I18n.T("成功"), AlertTypes.Success);
        }
    }

    private async Task DeleteCallAsync(IEnumerable<Channel> channel)
    {
        await _serviceScope.ServiceProvider.GetService<IChannelService>().DeleteAsync(channel.Adapt<List<BaseIdInput>>());
        await MainLayout.StateHasChangedAsync();
    }

    private async Task EditCallAsync(ChannelEditInput input)
    {
        await _serviceScope.ServiceProvider.GetService<IChannelService>().EditAsync(input);
        await MainLayout.StateHasChangedAsync();
    }

    private Task<SqlSugarPagedList<Channel>> QueryCallAsync(ChannelPageInput input)
    {
        return _serviceScope.ServiceProvider.GetService<IChannelService>().PageAsync(input);
    }
}