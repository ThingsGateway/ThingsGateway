// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using Microsoft.Extensions.Logging;

using ThingsGateway.Common.Extension;

namespace ThingsGateway.Gateway.Application;

public static class RuntimeServiceHelper
{
    public static async Task InitAsync(List<ChannelRuntime> newChannelRuntimes, List<DeviceRuntime> newDeviceRuntimes, ILogger logger)
    {
        //批量修改之后，需要重新加载通道
        await newChannelRuntimes.ParallelForEachAsync(async (newChannelRuntime, token) =>
        {
            try
            {
                newChannelRuntime.Init();
                foreach (var newDeviceRuntime in newDeviceRuntimes.Where(a => a.ChannelId == newChannelRuntime.Id))
                {
                    newDeviceRuntime.Init(newChannelRuntime);

                    var newVariableRuntimes = (await GlobalData.VariableService.GetAllAsync(newDeviceRuntime.Id).ConfigureAwait(false)).AdaptEnumerableVariableRuntime();

                    newVariableRuntimes.ParallelForEach(item => item.Init(newDeviceRuntime));
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Init Channel");
            }
        }).ConfigureAwait(false);
        GlobalData.ChannelDeviceRuntimeDispatchService.Dispatch(null);
        GlobalData.VariableRuntimeDispatchService.Dispatch(null);
    }

    public static void Init(List<ChannelRuntime> newChannelRuntimes)
    {
        //批量修改之后，需要重新加载通道
        foreach (var newChannelRuntime in newChannelRuntimes)
        {
            if (GlobalData.IdChannels.TryGetValue(newChannelRuntime.Id, out var channelRuntime))
            {
                channelRuntime.Dispose();
                newChannelRuntime.Init();
                channelRuntime.DeviceRuntimes.ForEach(a => a.Value.Init(newChannelRuntime));
                newChannelRuntime.DeviceRuntimes.AddRange(channelRuntime.DeviceRuntimes);
            }
            else
            {
                newChannelRuntime.Init();
            }
        }
        GlobalData.ChannelDeviceRuntimeDispatchService.Dispatch(null);
    }

    public static async Task InitAsync(List<DeviceRuntime> newDeviceRuntimes, ILogger logger)
    {
        await newDeviceRuntimes.ParallelForEachAsync(async (newDeviceRuntime, token) =>
            {
                try
                {
                    if (GlobalData.IdChannels.TryGetValue(newDeviceRuntime.ChannelId, out var newChannelRuntime))
                    {
                        newDeviceRuntime.Init(newChannelRuntime);

                        var newVariableRuntimes = (await GlobalData.VariableService.GetAllAsync(newDeviceRuntime.Id).ConfigureAwait(false)).AdaptEnumerableVariableRuntime();

                        newVariableRuntimes.ParallelForEach(item => item.Init(newDeviceRuntime));
                    }
                    else
                    {
                        logger.LogWarning("Channel not found");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Init Device");
                }
            }).ConfigureAwait(false);

        GlobalData.ChannelDeviceRuntimeDispatchService.Dispatch(null);
        GlobalData.VariableRuntimeDispatchService.Dispatch(null);
    }

    public static void Init(List<DeviceRuntime> newDeviceRuntimes)
    {
        //批量修改之后，需要重新加载通道
        foreach (var newDeviceRuntime in newDeviceRuntimes)
        {
            if (GlobalData.IdDevices.TryGetValue(newDeviceRuntime.Id, out var deviceRuntime))
            {
                deviceRuntime.Dispose();
            }
            if (GlobalData.IdChannels.TryGetValue(newDeviceRuntime.ChannelId, out var channelRuntime))
            {
                newDeviceRuntime.Init(channelRuntime);
            }
            if (deviceRuntime != null)
            {
                deviceRuntime.VariableRuntimes.ParallelForEach(a => a.Value.Init(newDeviceRuntime));
            }
        }

        GlobalData.ChannelDeviceRuntimeDispatchService.Dispatch(null);
        GlobalData.VariableRuntimeDispatchService.Dispatch(null);
    }
    public static void Init(List<VariableRuntime> newVariableRuntimes)
    {
        foreach (var newVariableRuntime in newVariableRuntimes)
        {
            if (GlobalData.IdVariables.TryGetValue(newVariableRuntime.Id, out var variableRuntime))
            {
                variableRuntime.Dispose();
            }
            if (GlobalData.IdDevices.TryGetValue(newVariableRuntime.DeviceId, out var deviceRuntime))
            {
                newVariableRuntime.Init(deviceRuntime);
            }
            if (newVariableRuntime.DeviceId == MemoryVariable.MemoryDeviceId)
            {
                newVariableRuntime.Init(GlobalData.MemoryDeviceRuntime);
            }
        }
        GlobalData.VariableRuntimeDispatchService.Dispatch(null);
    }

    public static void RemoveOldChannelRuntimes(IEnumerable<ChannelRuntime> oldChannelRuntimes)
    {
        var channels = oldChannelRuntimes.ToArray();
        var devs = channels.SelectMany(a => a.DeviceRuntimes).Select(a => a.Value).ToArray();
        devs.SelectMany(a => a.VariableRuntimes).Select(a => a.Value).ParallelForEach(a => a.Dispose());
        devs.ParallelForEach(a => a.Dispose());
        channels.ParallelForEach(a => a.Dispose());

        GlobalData.ChannelDeviceRuntimeDispatchService.Dispatch(null);
        GlobalData.VariableRuntimeDispatchService.Dispatch(null);
    }

    public static async Task<List<ChannelRuntime>> GetNewChannelRuntimesAsync(HashSet<long> ids)
    {
        var newChannelRuntimes = (await GlobalData.ChannelService.GetFromDBAsync((a => ids.Contains(a.Id))).ConfigureAwait(false)).AdaptListChannelRuntime();
        return newChannelRuntimes;
    }

    public static async Task<List<DeviceRuntime>> GetNewDeviceRuntimesAsync(HashSet<long> deviceids)
    {
        var newDeviceRuntimes = (await GlobalData.DeviceService.GetFromDBAsync((a => deviceids.Contains(a.Id))).ConfigureAwait(false)).AdaptListDeviceRuntime();
        return newDeviceRuntimes;
    }

    public static ConcurrentHashSet<IDriver> DeleteDeviceRuntime(List<DeviceRuntime> deviceRuntimes)
    {
        ConcurrentHashSet<IDriver> changedDriver = new();

        foreach (var deviceRuntime in deviceRuntimes)
        {
            //也需要删除变量
            var vars = deviceRuntime.VariableRuntimes.Select(a => a.Value);
            vars.ParallelForEach(v =>
            {
                //需要重启业务线程
                var deviceRuntimes = GlobalData.IdDevices.Where(a => GlobalData.ContainsVariable(a.Key, v)).Select(a => a.Value);
                foreach (var deviceRuntime in deviceRuntimes)
                {
                    if (deviceRuntime.Driver != null)
                    {
                        changedDriver.TryAdd(deviceRuntime.Driver);
                    }
                }

                v.Dispose();
            });
            deviceRuntime.Dispose();
        }
        GlobalData.ChannelDeviceRuntimeDispatchService.Dispatch(null);
        GlobalData.VariableRuntimeDispatchService.Dispatch(null);

        return changedDriver;
    }
    public static ConcurrentHashSet<IDriver> DeleteChannelRuntime(IEnumerable<long> ids)
    {
        ConcurrentHashSet<IDriver> changedDriver = new();

        //批量修改之后，需要重新加载通道
        foreach (var id in ids)
        {
            if (GlobalData.IdChannels.TryGetValue(id, out var channelRuntime))
            {
                channelRuntime.Dispose();
                var devs = channelRuntime.DeviceRuntimes.Select(a => a.Value).ToArray();

                //也需要删除设备和变量
                devs.ParallelForEach((a =>
                {
                    var vars = a.VariableRuntimes.Select(b => b.Value);
                    ParallelExtensions.ParallelForEach(vars, (v =>
                    {
                        //需要重启业务线程
                        var deviceRuntimes = GlobalData.IdDevices.Where(a => GlobalData.ContainsVariable(a.Key, v)).Select(a => a.Value);
                        foreach (var deviceRuntime in deviceRuntimes)
                        {
                            if (deviceRuntime.Driver != null)
                            {
                                changedDriver.TryAdd(deviceRuntime.Driver);
                            }
                        }

                        v.Dispose();
                    }
                    ));
                    a.Dispose();
                }));
            }
        }

        GlobalData.ChannelDeviceRuntimeDispatchService.Dispatch(null);
        GlobalData.VariableRuntimeDispatchService.Dispatch(null);

        return changedDriver;
    }

    public static async Task RestartDeviceAsync(List<DeviceRuntime> newDeviceRuntimes)
    {
        var groups = GlobalData.GetDeviceThreadManages(newDeviceRuntimes);
        await groups.ParallelForEachAsync(async (group, token) =>
        {
            if (group.Key != null)
                await group.Key.RestartDeviceAsync(group.Value, false).ConfigureAwait(false);
        }).ConfigureAwait(false);
        await GlobalData.GetAllVariableBusinessDeviceRuntime().Where(a => !newDeviceRuntimes.Contains(a)).Where(a => a.Driver?.DeviceThreadManage != null).GroupBy(a => a.Driver.DeviceThreadManage).ParallelForEachAsync(async (group, token) =>
        {
            if (group.Key != null)
                await group.Key.RestartDeviceAsync(group.ToArray(), false).ConfigureAwait(false);
        }).ConfigureAwait(false);
    }
    public static async Task RemoveDeviceAsync(HashSet<long> newDeciceIds)
    {
        //先找出线程管理器，停止
        var deviceRuntimes = GlobalData.IdDevices.FilterByKeys(newDeciceIds).Select(a => a.Value);
        await RemoveDeviceAsync(deviceRuntimes).ConfigureAwait(false);
    }
    public static async Task RemoveDeviceAsync(IEnumerable<DeviceRuntime> deviceRuntimes)
    {
        var groups = GlobalData.GetDeviceThreadManages(deviceRuntimes);
        await groups.ParallelForEachAsync(async (group, token) =>
         {
             if (group.Key != null)
                 await group.Key.RemoveDeviceAsync(group.Value.Select(a => a.Id).ToArray()).ConfigureAwait(false);
         }).ConfigureAwait(false);

        await GlobalData.GetAllVariableBusinessDeviceRuntime().Where(a => !deviceRuntimes.Contains(a)).Where(a => a.Driver?.DeviceThreadManage != null).GroupBy(a => a.Driver.DeviceThreadManage).ParallelForEachAsync(async (group, token) =>
        {
            if (group.Key != null)
                await group.Key.RestartDeviceAsync(group.ToArray(), false).ConfigureAwait(false);
        }).ConfigureAwait(false);

    }

    public static async Task ChangedDriverAsync(DeviceRuntime[] channelDevice, ILogger logger)
    {


        await channelDevice.ParallelForEachAsync(async (item, token) =>
         {
             try
             {
                 await item.Driver.AfterVariablesChangedAsync(token).ConfigureAwait(false);
             }
             catch (Exception ex)
             {
                 logger.LogWarning(ex, "VariablesChanged");
             }
         }).ConfigureAwait(false);
    }
    public static async Task ChangedDriverAsync(ConcurrentHashSet<IDriver> changedDriver, ILogger logger)
    {
        var drivers = GlobalData.GetAllVariableBusinessDriver();

        var changedDrivers = drivers.Concat(changedDriver).Where(a => a.DisposedValue == false).Distinct().ToArray();
        await changedDrivers.ParallelForEachAsync(async (driver, token) =>
         {
             try
             {
                 await driver.AfterVariablesChangedAsync(token).ConfigureAwait(false);
             }
             catch (Exception ex)
             {
                 logger.LogWarning(ex, "VariablesChanged");
             }
         }).ConfigureAwait(false);
    }

    public static void AddBusinessChangedDriver(HashSet<long> variableIds, ConcurrentHashSet<IDriver> changedDriver)
    {
        var data = GlobalData.IdVariables.FilterByKeys(variableIds).GroupBy(a => a.Value.DeviceRuntime).Where(a => a.Key != null && !a.Key.IsMemory);

        foreach (var group in data)
        {
            //这里改动的可能是旧绑定设备
            //需要改动DeviceRuntim的变量字典
            foreach (var item in group)
            {
                //需要重启业务线程
                var deviceRuntimes = GlobalData.IdDevices.Where(a =>

             GlobalData.ContainsVariable(a.Key, item.Value)

).Select(a => a.Value);
                foreach (var deviceRuntime in deviceRuntimes)
                {
                    if (deviceRuntime.Driver != null)
                    {
                        changedDriver.TryAdd(deviceRuntime.Driver);
                    }
                }
            }
            if (group.Key != null)
            {
                if (group.Key.Driver != null)
                {
                    changedDriver.TryAdd(group.Key.Driver);
                }
            }
        }
    }
    public static void VariableRuntimesDispose(IEnumerable<long> variableIds)
    {
        foreach (var variableId in variableIds)
        {
            if (GlobalData.IdVariables.TryGetValue(variableId, out var variableRuntime))
            {
                variableRuntime.Dispose();
            }
        }

        GlobalData.VariableRuntimeDispatchService.Dispatch(null);
    }

    public static void AddCollectChangedDriver(IEnumerable<VariableRuntime> newVariableRuntimes, ConcurrentHashSet<IDriver> changedDriver)
    {
        //批量修改之后，需要重新加载
        foreach (var newVariableRuntime in newVariableRuntimes)
        {
            if (GlobalData.IdDevices.TryGetValue(newVariableRuntime.DeviceId, out var deviceRuntime))
            {
                newVariableRuntime.Init(deviceRuntime);

                if (deviceRuntime.Driver != null && !changedDriver.Contains(deviceRuntime.Driver))
                {
                    changedDriver.TryAdd(deviceRuntime.Driver);
                }
            }
            if (newVariableRuntime.DeviceId == MemoryVariable.MemoryDeviceId)
            {
                newVariableRuntime.Init(GlobalData.MemoryDeviceRuntime);
            }
        }
        GlobalData.VariableRuntimeDispatchService.Dispatch(null);
    }
}