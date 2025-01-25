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

using Mapster;

using Microsoft.AspNetCore.Components.Forms;

using ThingsGateway.Extension.Generic;

namespace ThingsGateway.Gateway.Application;

public class VariableRuntimeService : IVariableRuntimeService
{
    private WaitLock WaitLock { get; set; } = new WaitLock();

    public async Task AddBatchAsync(List<Variable> input)
    {
        try
        {
            await WaitLock.WaitAsync().ConfigureAwait(false);

            await GlobalData.VariableService.AddBatchAsync(input).ConfigureAwait(false);

            var newVariableRuntimes = input.Adapt<List<VariableRuntime>>();

            //获取变量，先找到原插件线程，然后修改插件线程内的字典，再改动全局字典，最后刷新插件
            var data = GlobalData.IdVariables.Where(a => newVariableRuntimes.Select(a => a.Id).ToHashSet().Contains(a.Key)).GroupBy(a => a.Value.DeviceRuntime);

            HashSet<IDriver> changedDriver = new();
            foreach (var group in data)
            {
                //这里改动的可能是旧绑定设备
                //需要改动DeviceRuntim的变量字典
                foreach (var item in group)
                {
                    //需要重启业务线程
                    var deviceRuntimes = GlobalData.Devices.Where(a => item.Value.VariablePropertys?.ContainsKey(a.Key) == true).Select(a => a.Value);
                    foreach (var deviceRuntime in deviceRuntimes)
                    {
                        if (deviceRuntime.Driver != null)
                        {
                            changedDriver.Add(deviceRuntime.Driver);
                        }
                    }

                    item.Value.Dispose();
                }
                if (group.Key != null)
                {
                    if (group.Key.Driver != null)
                    {
                        changedDriver.Add(group.Key.Driver);
                    }
                }
            }

            //批量修改之后，需要重新加载
            foreach (var newVariableRuntime in newVariableRuntimes)
            {
                if (GlobalData.Devices.TryGetValue(newVariableRuntime.DeviceId, out var deviceRuntime))
                {
                    newVariableRuntime.Init(deviceRuntime);

                    if (deviceRuntime.Driver != null && !changedDriver.Contains(deviceRuntime.Driver))
                    {
                        changedDriver.Add(deviceRuntime.Driver);
                    }
                }
            }

            //根据条件重启通道线程
            foreach (var driver in changedDriver)
            {
                driver.AfterVariablesChanged();
            }

        }
        finally
        {
            WaitLock.Release();
        }
    }

    public async Task<bool> BatchEditAsync(IEnumerable<Variable> models, Variable oldModel, Variable model)
    {
        try
        {
            await WaitLock.WaitAsync().ConfigureAwait(false);

            models = models.Adapt<List<Variable>>();
            oldModel = oldModel.Adapt<Variable>();
            model = model.Adapt<Variable>();

            var result = await GlobalData.VariableService.BatchEditAsync(models, oldModel, model).ConfigureAwait(false);

            using var db = DbContext.GetDB<Variable>();
            var newVariableRuntimes = (await db.Queryable<Variable>().Where(a => models.Select(a => a.Id).ToHashSet().Contains(a.Id)).ToListAsync().ConfigureAwait(false)).Adapt<List<VariableRuntime>>();

            //获取变量，先找到原插件线程，然后修改插件线程内的字典，再改动全局字典，最后刷新插件
            var data = GlobalData.IdVariables.Where(a => newVariableRuntimes.Select(a => a.Id).ToHashSet().Contains(a.Key)).GroupBy(a => a.Value.DeviceRuntime);

            HashSet<IDriver> changedDriver = new();
            foreach (var group in data)
            {
                //这里改动的可能是旧绑定设备
                //需要改动DeviceRuntim的变量字典
                foreach (var item in group)
                {
                    //需要重启业务线程
                    var deviceRuntimes = GlobalData.Devices.Where(a => item.Value.VariablePropertys?.ContainsKey(a.Key) == true).Select(a => a.Value);
                    foreach (var deviceRuntime in deviceRuntimes)
                    {
                        if (deviceRuntime.Driver != null)
                        {
                            changedDriver.Add(deviceRuntime.Driver);
                        }
                    }

                    item.Value.Dispose();
                }
                if (group.Key != null)
                {
                    if (group.Key.Driver != null)
                    {
                        changedDriver.Add(group.Key.Driver);
                    }
                }
            }

            //批量修改之后，需要重新加载
            foreach (var newVariableRuntime in newVariableRuntimes)
            {
                if (GlobalData.Devices.TryGetValue(newVariableRuntime.DeviceId, out var deviceRuntime))
                {
                    newVariableRuntime.Init(deviceRuntime);

                    if (deviceRuntime.Driver != null && !changedDriver.Contains(deviceRuntime.Driver))
                    {
                        changedDriver.Add(deviceRuntime.Driver);
                    }
                }
            }

            //根据条件重启通道线程
            foreach (var driver in changedDriver)
            {
                driver.AfterVariablesChanged();
            }

            return true;

        }
        finally
        {
            WaitLock.Release();
        }
    }

    public async Task<bool> DeleteVariableAsync(IEnumerable<long> ids)
    {
        try
        {
            await WaitLock.WaitAsync().ConfigureAwait(false);


            ids = ids.ToHashSet();

            var result = await GlobalData.VariableService.DeleteVariableAsync(ids).ConfigureAwait(false);

            var variableRuntimes = GlobalData.IdVariables.Where(a => ids.Contains(a.Key)).Select(a => a.Value).ToList();

            foreach (var variableRuntime in variableRuntimes)
            {
                variableRuntime.Dispose();
            }
            var data = variableRuntimes.Where(a => a.DeviceRuntime?.Driver != null).GroupBy(a => a.DeviceRuntime);

            HashSet<IDriver> changedDriver = new();
            foreach (var group in data)
            {
                //这里改动的可能是旧绑定设备
                //需要改动DeviceRuntim的变量字典
                foreach (var item in group)
                {
                    //需要重启业务线程
                    var deviceRuntimes = GlobalData.Devices.Where(a => item.VariablePropertys?.ContainsKey(a.Key) == true).Select(a => a.Value);
                    foreach (var deviceRuntime in deviceRuntimes)
                    {
                        if (deviceRuntime.Driver != null)
                        {
                            changedDriver.Add(deviceRuntime.Driver);
                        }
                    }

                    item.Dispose();
                }
                if (group.Key != null)
                {
                    if (group.Key.Driver != null)
                    {
                        changedDriver.Add(group.Key.Driver);
                    }
                }
            }

            foreach (var driver in changedDriver)
            {
                driver.AfterVariablesChanged();
            }



            return true;
        }
        finally
        {
            WaitLock.Release();
        }


    }
    public Task<Dictionary<string, object>> ExportVariableAsync(ExportFilter exportFilter) => GlobalData.VariableService.ExportVariableAsync(exportFilter);

    public async Task ImportVariableAsync(Dictionary<string, ImportPreviewOutputBase> input)
    {

        try
        {
            await WaitLock.WaitAsync().ConfigureAwait(false);

            var result = await GlobalData.VariableService.ImportVariableAsync(input).ConfigureAwait(false);


            using var db = DbContext.GetDB<Variable>();
            var newVariableRuntimes = (await db.Queryable<Variable>().Where(a => result.Contains(a.Id)).ToListAsync().ConfigureAwait(false)).Adapt<List<VariableRuntime>>();

            //先找出线程管理器，停止
            var data = GlobalData.IdVariables.Where(a => newVariableRuntimes.Select(a => a.Id).ToHashSet().Contains(a.Key)).GroupBy(a => a.Value.DeviceRuntime);

            HashSet<IDriver> changedDriver = new();
            foreach (var group in data)
            {
                //这里改动的可能是旧绑定设备
                //需要改动DeviceRuntim的变量字典
                foreach (var item in group)
                {
                    //需要重启业务线程
                    var deviceRuntimes = GlobalData.Devices.Where(a => item.Value.VariablePropertys?.ContainsKey(a.Key) == true).Select(a => a.Value);
                    foreach (var deviceRuntime in deviceRuntimes)
                    {
                        if (deviceRuntime.Driver != null)
                        {
                            changedDriver.Add(deviceRuntime.Driver);
                        }
                    }

                    item.Value.Dispose();
                }
                if (group.Key != null)
                {
                    if (group.Key.Driver != null)
                    {
                        changedDriver.Add(group.Key.Driver);
                    }
                }
            }

            //批量修改之后，需要重新加载
            foreach (var newVariableRuntime in newVariableRuntimes)
            {
                if (GlobalData.Devices.TryGetValue(newVariableRuntime.DeviceId, out var deviceRuntime))
                {
                    newVariableRuntime.Init(deviceRuntime);
                    //添加新变量所在任务
                    if (deviceRuntime.Driver != null && !changedDriver.Contains(deviceRuntime.Driver))
                    {
                        changedDriver.Add(deviceRuntime.Driver);
                    }
                }
            }

            //根据条件重启通道线程

            foreach (var driver in changedDriver)
            {
                driver.AfterVariablesChanged();
            }



        }
        finally
        {
            WaitLock.Release();
        }

    }

    public async Task InsertTestDataAsync(int testVariableCount, int testDeviceCount, string slaveUrl, bool restart = true)
    {
        try
        {
            await WaitLock.WaitAsync().ConfigureAwait(false);



            var datas = await GlobalData.VariableService.InsertTestDataAsync(testVariableCount, testDeviceCount, slaveUrl).ConfigureAwait(false);

            {
                var newChannelRuntimes = (datas.Item1).Adapt<List<ChannelRuntime>>();

                //批量修改之后，需要重新加载通道
                foreach (var newChannelRuntime in newChannelRuntimes)
                {
                    if (GlobalData.Channels.TryGetValue(newChannelRuntime.Id, out var channelRuntime))
                    {
                        channelRuntime.Dispose();
                        newChannelRuntime.Init();
                        newChannelRuntime.DeviceRuntimes.AddRange(channelRuntime.DeviceRuntimes);
                    }
                    else
                    {
                        newChannelRuntime.Init();

                    }
                }

                {

                    var newDeviceRuntimes = (datas.Item2).Adapt<List<DeviceRuntime>>();

                    //批量修改之后，需要重新加载通道
                    foreach (var newDeviceRuntime in newDeviceRuntimes)
                    {
                        if (GlobalData.Devices.TryGetValue(newDeviceRuntime.Id, out var deviceRuntime))
                        {
                            deviceRuntime.Dispose();
                        }
                        if (GlobalData.Channels.TryGetValue(newDeviceRuntime.ChannelId, out var channelRuntime))
                        {
                            newDeviceRuntime.Init(channelRuntime);
                        }
                        if (deviceRuntime != null)
                        {
                            newDeviceRuntime.VariableRuntimes.AddRange(deviceRuntime.VariableRuntimes);
                        }
                    }


                }
                {
                    var newVariableRuntimes = (datas.Item3).Adapt<List<VariableRuntime>>();
                    //获取变量，先找到原插件线程，然后修改插件线程内的字典，再改动全局字典，最后刷新插件
                    var data = GlobalData.IdVariables.Where(a => newVariableRuntimes.Select(a => a.Id).ToHashSet().Contains(a.Key)).GroupBy(a => a.Value.DeviceRuntime);

                    foreach (var group in data)
                    {
                        //这里改动的可能是旧绑定设备
                        //需要改动DeviceRuntim的变量字典
                        foreach (var item in group)
                        {
                            item.Value.Dispose();
                        }
                    }

                    //批量修改之后，需要重新加载
                    foreach (var newVariableRuntime in newVariableRuntimes)
                    {
                        if (GlobalData.Devices.TryGetValue(newVariableRuntime.DeviceId, out var deviceRuntime))
                        {
                            newVariableRuntime.Init(deviceRuntime);
                        }
                    }

                }
                //根据条件重启通道线程

                if (restart)
                    await GlobalData.ChannelThreadManage.RestartChannelAsync(newChannelRuntimes).ConfigureAwait(false);


                App.GetService<IDispatchService<DeviceRuntime>>().Dispatch(null);
            }
        }
        finally
        {
            WaitLock.Release();
        }

    }

    public Task<Dictionary<string, ImportPreviewOutputBase>> PreviewAsync(IBrowserFile browserFile)
    {
        return GlobalData.VariableService.PreviewAsync(browserFile);
    }

    public async Task<bool> SaveVariableAsync(Variable input, ItemChangedType type)
    {
        try
        {
            input = input.Adapt<Variable>();
            await WaitLock.WaitAsync().ConfigureAwait(false);



            var result = await GlobalData.VariableService.SaveVariableAsync(input, type).ConfigureAwait(false);


            using var db = DbContext.GetDB<Variable>();
            var newVariableRuntime = (await db.Queryable<Variable>().Where(a => input.Id == a.Id).FirstAsync().ConfigureAwait(false)).Adapt<VariableRuntime>();

            if (newVariableRuntime == null) return false;

            HashSet<IDriver> changedDriver = new();



            //这里改动的可能是旧绑定设备
            //需要改动DeviceRuntim的变量字典

            if (GlobalData.IdVariables.TryGetValue(newVariableRuntime.Id, out var variableRuntime))
            {
                if (variableRuntime.DeviceRuntime?.Driver != null)
                {
                    changedDriver.Add(variableRuntime.DeviceRuntime.Driver);
                }
                variableRuntime.Dispose();
            }

            //需要重启业务线程
            var deviceRuntimes = GlobalData.Devices.Where(a => newVariableRuntime.VariablePropertys?.ContainsKey(a.Key) == true).Select(a => a.Value);
            foreach (var businessDeviceRuntime in deviceRuntimes)
            {
                if (businessDeviceRuntime.Driver != null)
                {
                    changedDriver.Add(businessDeviceRuntime.Driver);
                }
            }

            //批量修改之后，需要重新加载

            if (GlobalData.Devices.TryGetValue(newVariableRuntime.DeviceId, out var deviceRuntime))
            {
                newVariableRuntime.Init(deviceRuntime);

                if (deviceRuntime.Driver != null && !changedDriver.Contains(deviceRuntime.Driver))
                {
                    changedDriver.Add(deviceRuntime.Driver);
                }

            }

            //根据条件重启通道线程
            foreach (var driver in changedDriver)
            {
                driver.AfterVariablesChanged();
            }


            return true;
        }
        finally
        {
            WaitLock.Release();
        }
    }

    public void PreheatCache() => GlobalData.VariableService.PreheatCache();


    public Task<MemoryStream> ExportMemoryStream(List<Variable> data, string deviceName) => GlobalData.VariableService.ExportMemoryStream(data, deviceName);

}