//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using TouchSocket.Core;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace ThingsGateway.Gateway.Application;

internal sealed partial class RedundantRpcServer : SingletonRpcServer, IRedundantRpcServer
{
    RedundancyTask RedundancyTask;
    public RedundantRpcServer(RedundancyTask redundancyTask)
    {
        RedundancyTask = redundancyTask;
    }

    [DmtpRpc]
    public void UpData(ICallContext callContext, List<DeviceDataWithValue> deviceDatas)
    {
        foreach (var deviceData in deviceDatas)
        {
            if (GlobalData.TryGetDeviceRuntime(deviceData.Name, out var device))
            {
                device.RpcDriver = RedundancyTask;
                device.Tag = callContext.Caller is IIdClient idClient ? idClient.Id : string.Empty;

                device.SetDeviceStatus(deviceData.ActiveTime, deviceData.DeviceStatus == DeviceStatusEnum.OnLine ? false : true, lastErrorMessage: deviceData.LastErrorMessage);

                foreach (var variableData in deviceData.ReadOnlyVariableRuntimes)
                {
                    if (device.ReadOnlyVariableRuntimes.TryGetValue(variableData.Key, out var value))
                    {
                        value.SetValue(variableData.Value.RawValue, variableData.Value.CollectTime, variableData.Value.IsOnline);
                        value.SetErrorMessage(variableData.Value.LastErrorMessage);
                    }
                }
            }
        }
        RedundancyTask.LogMessage?.Trace("RpcServer Update data success");
    }

    [DmtpRpc]
    public async Task SyncData(List<Channel> channels, List<Device> devices, List<MemoryVariable> variables)
    {
        List<Channel> addChannels = new();
        List<Device> addDevices = new();
        List<Variable> addVariables = new();
        List<Channel> upChannels = new();
        List<Device> upDevices = new();
        List<Variable> upVariables = new();

        Dictionary<long, long> channelNewId = new();
        Dictionary<long, long> deviceNewId = new();


        foreach (var channel in channels)
        {
            if (GlobalData.ReadOnlyChannels.TryGetValue(channel.Name, out var channelRuntime))
            {
                channelNewId.TryAdd(channel.Id, channelRuntime.Id);
                channel.Id = channelRuntime.Id;
                channel.Enable = false;
                upChannels.Add(channel);
            }
            else if (channel.Id != MemoryConst.MemoryChannelId)
            {
                var id = CommonUtils.GetSingleId();
                channelNewId.TryAdd(channel.Id, id);
                channel.Id = id;
                channel.Enable = false;
                addChannels.Add(channel);
            }
        }

        foreach (var device in devices)
        {
            if (GlobalData.TryGetDeviceRuntime(device.Name, out var deviceRuntime))
            {
                deviceNewId.TryAdd(device.Id, deviceRuntime.Id);
                device.Id = deviceRuntime.Id;

                channelNewId.TryGetValue(device.ChannelId, out var newid);
                device.ChannelId = newid;

                device.Enable = false;
                upDevices.Add(device);
            }
            else if (device.Id != MemoryConst.MemoryDeviceId)
            {
                var id = CommonUtils.GetSingleId();
                deviceNewId.TryAdd(device.Id, id);
                device.Id = id;

                channelNewId.TryGetValue(device.ChannelId, out var newid);
                device.ChannelId = newid;
                device.Enable = false;
                addDevices.Add(device);
            }
        }

        foreach (var variable in variables.Where(a => a.DeviceId != MemoryConst.MemoryDeviceId))
        {
            {
                deviceNewId.TryGetValue(variable.DeviceId, out var newid);
                if (GlobalData.TryGetDeviceRuntime(newid, out var deviceRuntime))
                {
                    if (deviceRuntime.ReadOnlyVariableRuntimes.TryGetValue(variable.Name, out var variableRuntime))
                    {
                        variable.Id = variableRuntime.Id;

                        variable.DeviceId = newid;

                        upVariables.Add(variable);
                    }
                    else
                    {
                        var id = CommonUtils.GetSingleId();
                        variable.Id = id;

                        variable.DeviceId = newid;
                        addVariables.Add(variable);
                    }
                }
                else
                {
                    var id = CommonUtils.GetSingleId();
                    variable.Id = id;

                    variable.DeviceId = newid;
                    addVariables.Add(variable);
                }
            }
        }

        await GlobalData.ChannelRuntimeService.InsertAsync(addChannels, addDevices, addVariables, true).ConfigureAwait(false);
        await GlobalData.ChannelRuntimeService.UpdateAsync(upChannels, upDevices, upVariables, true).ConfigureAwait(false);

        List<MemoryVariable> addMemVars = new();
        List<MemoryVariable> upMemVars = new();
        foreach (var variable in variables.Where(a => a.DeviceId == MemoryConst.MemoryDeviceId))
        {
            if (GlobalData.MemoryVariableRuntimes.TryGetValue(variable.Name, out var varRuntime))
            {
                variable.Id = varRuntime.Id;
                upMemVars.Add(variable);
            }
            else
            {
                addMemVars.Add(variable);
            }
        }
        await App.GetService<IMemoryVariableRuntimeService>().BatchSaveMemoryVariableAsync(addMemVars, BootstrapBlazor.Components.ItemChangedType.Add, false).ConfigureAwait(false);
        await App.GetService<IMemoryVariableRuntimeService>().BatchSaveMemoryVariableAsync(upMemVars, BootstrapBlazor.Components.ItemChangedType.Update, false).ConfigureAwait(false);

        RedundancyTask.LogMessage?.LogDebug($"Sync data success");
    }

    [DmtpRpc]
    public Task<IDictionary<string, IDictionary<string, OperResult<object>>>> RpcAsync(ICallContext callContext, Dictionary<string, Dictionary<string, string>> deviceDatas)
    {
        return GlobalData.RpcService.InvokeDeviceMethodAsync($"Redundant[{(callContext.Caller is IIdClient idClient ? idClient.Id : string.Empty)}]", deviceDatas, callContext.Token);
    }


}
