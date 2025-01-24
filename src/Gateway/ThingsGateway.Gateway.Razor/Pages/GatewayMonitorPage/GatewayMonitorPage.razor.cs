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

public partial class GatewayMonitorPage
{
    private ChannelDeviceTreeItem SelectModel { get; set; } = new() { ChannelDevicePluginType = ChannelDevicePluginTypeEnum.PluginType, PluginType = PluginTypeEnum.Collect };

    #region 查询

    private async Task TreeChangedAsync(ChannelDeviceTreeItem channelDeviceTreeItem)
    {
        ShowChannelRuntime = null;
        ShowDeviceRuntime = null;
        SelectModel = channelDeviceTreeItem;
        if (channelDeviceTreeItem.TryGetChannelRuntime(out var channelRuntime))
        {
            ShowChannelRuntime = channelRuntime;
            if (channelRuntime.IsCollect == true)
            {
                VariableRuntimes = channelRuntime.ReadDeviceRuntimes.SelectMany(a => a.Value.ReadVariableRuntimes.Select(a => a.Value));
            }
            else
            {
                VariableRuntimes = channelRuntime.ReadDeviceRuntimes.SelectMany(a => a.Value.Driver?.VariableRuntimes?.Select(a => a.Value)).Where(a => a != null);
            }

        }
        else if (channelDeviceTreeItem.TryGetDeviceRuntime(out var deviceRuntime))
        {
            ShowDeviceRuntime = deviceRuntime;
            if (deviceRuntime.IsCollect == true)
            {
                VariableRuntimes = deviceRuntime.ReadVariableRuntimes.Select(a => a.Value);
            }
            else
            {
                VariableRuntimes = deviceRuntime.Driver?.VariableRuntimes?
.Select(a => a.Value) ?? Enumerable.Empty<VariableRuntime>();
            }

        }
        else if (channelDeviceTreeItem.TryGetPluginName(out var pluginName))
        {
            var pluginType = GlobalData.PluginService.GetList().FirstOrDefault(a => a.FullName == pluginName)?.PluginType;
            if (pluginType == PluginTypeEnum.Collect)
            {
                var channels = await GlobalData.GetCurrentUserChannels().ConfigureAwait(false);
                VariableRuntimes = channels.Where(a => a.Value.PluginName == pluginName).SelectMany(a => a.Value.ReadDeviceRuntimes).SelectMany(a => a.Value.ReadVariableRuntimes).Select(a => a.Value);
            }
            else
            {
                var channels = await GlobalData.GetCurrentUserChannels().ConfigureAwait(false);
                VariableRuntimes = channels.Where(a => a.Value.PluginName == pluginName).SelectMany(a => a.Value.ReadDeviceRuntimes).SelectMany(a => a.Value.Driver?.VariableRuntimes).Select(a => a.Value).Where(a => a != null);
            }
        }
        else
        {
            var variables = await GlobalData.GetCurrentUserIdVariables().ConfigureAwait(false);
            VariableRuntimes = variables.Select(a => a.Value);
        }
        await InvokeAsync(StateHasChanged);
    }

    #endregion 查询
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await TreeChangedAsync(SelectModel);
        await base.OnAfterRenderAsync(firstRender);
    }
    public IEnumerable<VariableRuntime> VariableRuntimes { get; set; } = Enumerable.Empty<VariableRuntime>();

    private ChannelRuntime ShowChannelRuntime { get; set; }
    private DeviceRuntime ShowDeviceRuntime { get; set; }
    public ShowTypeEnum? ShowType { get; set; }
    private bool AutoRestartThread { get; set; } = true;


}
