//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Gateway.Razor;

public partial class GatewayMonitorPage
{
    private ChannelDeviceTreeItem SelectModel { get; set; } = new() { ChannelDevicePluginType = ChannelDevicePluginTypeEnum.PluginType, PluginType = null };

    #region 查询
    private bool ShowMemoryVariable = false;
    private async Task TreeChangedAsync(ChannelDeviceTreeItem channelDeviceTreeItem)
    {
        ShowMemoryVariable = false;
        //ShowChannelRuntime = 0;
        //ShowDeviceRuntime = 0;
        SelectModel = channelDeviceTreeItem;
        var variables = await GlobalData.GetCurrentUserIdVariables().ConfigureAwait(false);
        var channels = await GlobalData.GetCurrentUserChannels().ConfigureAwait(false);
        var devices = await GlobalData.GetCurrentUserDevices().ConfigureAwait(false);
        if (channelDeviceTreeItem.TryGetChannelRuntime(out var channelRuntime))
        {
            //ShowChannelRuntime = channelRuntime.Id;

            if (channelRuntime.IsCollect == true)
            {
                VariableRuntimes = channelRuntime.ReadDeviceRuntimes.SelectMany(a => a.Value.ReadOnlyVariableRuntimes.Select(a => a.Value).Where(a => a != null && !a.IsMemory));
            }
            else
            {
                VariableRuntimes = channelRuntime.ReadDeviceRuntimes.Where(a => a.Value?.Driver?.IdVariableRuntimes != null).SelectMany(a => a.Value?.Driver?.IdVariableRuntimes?.Where(a => a.Value != null && !a.Value.IsMemory)?.Select(a => a.Value)).Where(a => a != null);
            }
            //ChannelRuntimes = Enumerable.Repeat(channelRuntime, 1);
            //DeviceRuntimes = channelRuntime.ReadDeviceRuntimes.Select(a => a.Value);
        }
        else if (channelDeviceTreeItem.TryGetDeviceRuntime(out var deviceRuntime))
        {
            //ShowDeviceRuntime = deviceRuntime.Id;
            if (deviceRuntime.IsCollect == true)
            {
                if (deviceRuntime.Id == MemoryConst.MemoryDeviceId)
                {
                    MemoryVariableRuntimes = GlobalData.MemoryVariableRuntimes.Select(a => a.Value).Cast<MemoryVariableRuntime>();
                    ShowMemoryVariable = true;
                }
                else
                {
                    VariableRuntimes = deviceRuntime.ReadOnlyVariableRuntimes.Select(a => a.Value).Where(a => a != null);
                }
            }
            else
            {
                if (deviceRuntime.Driver == null)
                {
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(2000);
                        VariableRuntimes = deviceRuntime.Driver?.IdVariableRuntimes?.Where(a => a.Value != null && !a.Value.IsMemory)
.Select(a => a.Value) ?? Enumerable.Empty<VariableRuntime>();
                        await InvokeAsync(StateHasChanged);
                    });
                }
                else
                {
                    VariableRuntimes = deviceRuntime.Driver?.IdVariableRuntimes?.Where(a => a.Value != null && !a.Value.IsMemory)
.Select(a => a.Value) ?? Enumerable.Empty<VariableRuntime>();
                }

            }
            //ChannelRuntimes = Enumerable.Repeat(deviceRuntime.ChannelRuntime, 1);
            //DeviceRuntimes = Enumerable.Repeat(deviceRuntime, 1);
        }
        else if (channelDeviceTreeItem.TryGetPluginName(out var pluginName))
        {
            var pluginType = GlobalData.PluginService.GetPluginList().FirstOrDefault(a => a.FullName == pluginName)?.PluginType;
            if (pluginType == PluginTypeEnum.Collect)
            {
                VariableRuntimes = channels.Where(a => a.PluginName == pluginName).SelectMany(a => a.ReadDeviceRuntimes).SelectMany(a => a.Value.ReadOnlyVariableRuntimes).Select(a => a.Value).Where(a => a != null && !a.IsMemory);
            }
            else
            {
                VariableRuntimes = channels.Where(a => a.PluginName == pluginName).SelectMany(a => a.ReadDeviceRuntimes).Where(a => a.Value.Driver?.IdVariableRuntimes != null && !a.Value.IsMemory).SelectMany(a => a.Value.Driver?.IdVariableRuntimes).Select(a => a.Value);
            }

            //ChannelRuntimes = channels.Where(a => a.PluginName == pluginName);
            //DeviceRuntimes = devices.Where(a => a.PluginName == pluginName);
        }
        else
        {
            VariableRuntimes = variables.Where(a => a != null && !a.IsMemory);
            //if (channelDeviceTreeItem.TryGetPluginType(out var pluginTypeEnum))
            //{
            //    if (pluginTypeEnum != null)
            //    {
            //        ChannelRuntimes = channels.Where(a => a.PluginType == pluginTypeEnum);
            //        DeviceRuntimes = devices.Where(a => a.PluginType == pluginTypeEnum);
            //    }
            //    else
            //    {
            //        ChannelRuntimes = channels;
            //        DeviceRuntimes = devices;
            //    }
            //}
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
    public IEnumerable<MemoryVariableRuntime> MemoryVariableRuntimes { get; set; } = Enumerable.Empty<MemoryVariableRuntime>();

    //public IEnumerable<ChannelRuntime> ChannelRuntimes { get; set; } = Enumerable.Empty<ChannelRuntime>();
    //public IEnumerable<DeviceRuntime> DeviceRuntimes { get; set; } = Enumerable.Empty<DeviceRuntime>();

    //private long ShowChannelRuntime { get; set; }
    //private long ShowDeviceRuntime { get; set; }
    //public ShowTypeEnum? ShowType { get; set; }
    private bool AutoRestartThread { get; set; } = true;
}
