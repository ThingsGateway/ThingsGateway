//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Foundation.Common.Log;

namespace ThingsGateway.Gateway.Razor;

public partial class ChannelRuntimeInfo1 : IDisposable
{
    [Inject]
    IStringLocalizer<ThingsGateway.Gateway.Razor._Imports> GatewayLocalizer { get; set; }

    [Parameter, EditorRequired]
#if Management
    public ThingsGateway.Management.Application.ChannelRuntime ChannelRuntime { get; set; }
#else
    public ThingsGateway.Gateway.Application.ChannelRuntime ChannelRuntime { get; set; }
#endif
    private string Name => $"{ChannelRuntime.ToString()}  -  {(ChannelRuntime.Started == false ? "Task cancel" : "Task run")}";
    [Inject]
#if Management
    ThingsGateway.Management.Application.IChannelPageService ChannelPageService { get; set; }
#else
    ThingsGateway.Gateway.Application.IChannelPageService ChannelPageService { get; set; }
#endif
    private async Task RestartChannelAsync()
    {
        await ChannelPageService.RestartChannelAsync(ChannelRuntime.Id);
    }

    protected override void OnInitialized()
    {
#if !Management
        _ = RunTimerAsync();
#endif
        base.OnInitialized();
    }

    private bool Disposed;
#if !Management
    private async Task RunTimerAsync()
    {
        while (!Disposed)
        {
            try
            {
                if (!ChannelRuntime.Started)
                {
                    ChannelRuntime = GlobalData.ReadOnlyIdChannels.TryGetValue(ChannelRuntime.Id, out var channelRuntime) ? channelRuntime : ChannelRuntime;
                }
                await InvokeAsync(StateHasChanged);

            }
            catch (Exception ex)
            {
                XTrace.WriteException(ex);
            }
            finally
            {
                await Task.Delay(5000);
            }
        }
    }
#endif

    public void Dispose()
    {
        Disposed = true;
        GC.SuppressFinalize(this);
    }
}
