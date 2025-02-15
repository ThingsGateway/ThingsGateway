//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Gateway.Razor;

public partial class ChannelRuntimeInfo1 : IDisposable
{
    [Inject]
    IStringLocalizer<ThingsGateway.Gateway.Razor._Imports> GatewayLocalizer { get; set; }

    [Parameter, EditorRequired]
    public ChannelRuntime ChannelRuntime { get; set; }
    private string Name => $"{ChannelRuntime.ToString()}  -  {(ChannelRuntime.DeviceThreadManage == null ? "Task cancel" : "Task run")}";

    private async Task RestartChannelAsync()
    {
        if (ChannelRuntime.DeviceThreadManage?.ChannelThreadManage != null)
            await Task.Run(() => ChannelRuntime.DeviceThreadManage?.ChannelThreadManage.RestartChannelAsync(ChannelRuntime));
        else
            await Task.Run(() => GlobalData.ChannelThreadManage.RestartChannelAsync(ChannelRuntime));
    }


    protected override void OnInitialized()
    {
        _ = RunTimerAsync();
        base.OnInitialized();
    }

    private bool Disposed;
    private async Task RunTimerAsync()
    {
        while (!Disposed)
        {
            try
            {
                await InvokeAsync(() =>
                {
                    StateHasChanged();
                });
            }
            catch (Exception ex)
            {
                NewLife.Log.XTrace.WriteException(ex);
            }
            finally
            {
                await Task.Delay(5000);
            }
        }
    }

    public void Dispose()
    {
        Disposed = true;
        GC.SuppressFinalize(this);
    }
}
