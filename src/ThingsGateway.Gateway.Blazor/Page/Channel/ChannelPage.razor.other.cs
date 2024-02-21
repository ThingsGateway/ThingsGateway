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

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;

using ThingsGateway.Core.Extension;
using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Gateway.Blazor;

public partial class ChannelPage
{
    private async Task CopyClickAsync(IEnumerable<Channel> channels)
    {
        if (!channels.Any())
        {
            await PopupService.EnqueueSnackbarAsync("需选择一项或多项", AlertTypes.Warning);
            return;
        }
        var input = await PopupService.PromptAsync(AppService.I18n.T("复制设备"), AppService.I18n.T("输入复制数量")
            , a => int.TryParse(a, out var result1) ? channels.Count() * result1 > 100000 ? "不支持大批量" : true : "填入数字");
        if (int.TryParse(input, out var result))
        {
            await _serviceScope.ServiceProvider.GetService<IChannelService>().CopyAsync(channels, result);
            await _datatable.QueryClickAsync();
            await PopupService.EnqueueSnackbarAsync(AppService.I18n.T("成功"), AlertTypes.Success);
            await MainLayout.StateHasChangedAsync();
        }
    }

    private async Task ImportClickAsync()
    {
        Func<IBrowserFile, Task<Dictionary<string, ImportPreviewOutputBase>>> preview = (a => _serviceScope.ServiceProvider.GetService<IChannelService>().PreviewAsync(a));
        var import = EventCallback.Factory.Create<Dictionary<string, ImportPreviewOutputBase>>(this, value => _serviceScope.ServiceProvider.GetService<IChannelService>().ImportAsync(value));
        var data = (bool?)await PopupService.OpenAsync(typeof(ImportExcel), new Dictionary<string, object?>()
        {
            {nameof(ImportExcel.Import),import },
            {nameof(ImportExcel.Preview),preview },
        });
        if (data == true)
        {
            await _datatable.QueryClickAsync();
            await MainLayout.StateHasChangedAsync();
        }
    }

    private async Task DownExportAsync(bool isAll = false)
    {
        var query = _search?.Adapt<ChannelInput>();
        query.All = isAll;
        await AppService.DownFileAsync("gatewayExport/channel", DateTime.Now.ToFileDateTimeFormat(), query);
    }
}