//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Components;

using ThingsGateway.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.Debug;

public partial class Dlt645_2007Master : ComponentBase, IDisposable
{
    private ThingsGateway.Foundation.Dlt645.Dlt645_2007Master _plc;

    private string LogPath;

    ~Dlt645_2007Master()
    {
        this.SafeDispose();
    }

    private AdapterDebugComponent AdapterDebugComponent { get; set; }

    private ChannelData ChannelData { get; set; }

    public void Dispose()
    {
        _plc?.SafeDispose();
        ChannelData?.Channel?.SafeDispose();
        GC.SuppressFinalize(this);
    }

    private void OnConfimClick(ChannelData channelData)
    {
        ChannelData = channelData;
        _plc = new ThingsGateway.Foundation.Dlt645.Dlt645_2007Master(channelData.Channel);
        LogPath = channelData.Id.GetDebugLogPath();
    }

    private async Task OnConnectClick(ChannelData channelData)
    {
        if (ChannelData?.Channel != null)
        {
            try
            {
                await ChannelData.Channel.ConnectAsync(_plc.ConnectTimeout, default).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                ChannelData.Channel.Logger?.Exception(ex);
            }
        }
        await InvokeAsync(StateHasChanged);
    }

    private async Task OnDisConnectClick()
    {
        if (_plc != null)
            await _plc?.CloseAsync();
        await InvokeAsync(StateHasChanged);
    }
}
