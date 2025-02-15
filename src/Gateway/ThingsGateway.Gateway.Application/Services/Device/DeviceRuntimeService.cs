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

using ThingsGateway.NewLife;

namespace ThingsGateway.Gateway.Application;

public class DeviceRuntimeService : IDeviceRuntimeService
{
    private WaitLock WaitLock { get; set; } = new WaitLock();
    public async Task<bool> BatchEditAsync(IEnumerable<Device> models, Device oldModel, Device model, bool restart = true)
    {
        try
        {
            models = models.Adapt<List<Device>>();
            oldModel = oldModel.Adapt<Device>();
            model = model.Adapt<Device>();
            await WaitLock.WaitAsync().ConfigureAwait(false);

            var result = await GlobalData.DeviceService.BatchEditAsync(models, oldModel, model).ConfigureAwait(false);

            var newDeviceRuntimes = (await GlobalData.DeviceService.GetAllAsync().ConfigureAwait(false)).Where(a => models.Select(a => a.Id).ToHashSet().Contains(a.Id)).Adapt<List<DeviceRuntime>>();

            if (restart)
            {
                //先找出线程管理器，停止
                var data = GlobalData.Devices.Where(a => newDeviceRuntimes.Select(a => a.Id).ToHashSet().Contains(a.Key)).GroupBy(a => a.Value.ChannelRuntime?.DeviceThreadManage);
                foreach (var group in data)
                {
                    if (group.Key != null)
                        await group.Key.RemoveDeviceAsync(group.Select(a => a.Value.Id)).ConfigureAwait(false);
                }
            }

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
                    deviceRuntime.VariableRuntimes.ParallelForEach(a => a.Value.Init(newDeviceRuntime));
                }
            }

            //根据条件重启通道线程

            if (restart)
            {
                foreach (var group in newDeviceRuntimes.Where(a => a.ChannelRuntime?.DeviceThreadManage != null).GroupBy(a => a.ChannelRuntime))
                {
                    if (group.Key?.DeviceThreadManage != null)
                        await group.Key.DeviceThreadManage.RestartDeviceAsync(group, false).ConfigureAwait(false);
                }
            }

            return true;
        }
        finally
        {
            WaitLock.Release();
        }
    }

    public async Task<bool> DeleteDeviceAsync(IEnumerable<long> ids, bool restart = true)
    {
        try
        {
            await WaitLock.WaitAsync().ConfigureAwait(false);


            ids = ids.ToHashSet();

            var result = await GlobalData.DeviceService.DeleteDeviceAsync(ids).ConfigureAwait(false);

            //根据条件重启通道线程
            var deviceRuntimes = GlobalData.Devices.Where(a => ids.Contains(a.Key)).Select(a => a.Value).ToList();



            foreach (var deviceRuntime in deviceRuntimes)
            {
                //也需要删除变量
                deviceRuntime.VariableRuntimes.ParallelForEach(a =>
                {
                    a.Value.Dispose();
                });
                deviceRuntime.Dispose();
            }

            if (restart)
            {
                var groups = GlobalData.GetDeviceThreadManages(deviceRuntimes);
                foreach (var group in groups)
                {
                    if (group.Key != null)
                        await group.Key.RemoveDeviceAsync(group.Value.Select(a => a.Id)).ConfigureAwait(false);
                }
            }

            return true;

        }
        finally
        {
            WaitLock.Release();
        }
    }
    public Task<Dictionary<string, object>> ExportDeviceAsync(ExportFilter exportFilter) => GlobalData.DeviceService.ExportDeviceAsync(exportFilter);
    public Task<Dictionary<string, ImportPreviewOutputBase>> PreviewAsync(IBrowserFile browserFile) => GlobalData.DeviceService.PreviewAsync(browserFile);
    public Task<MemoryStream> ExportMemoryStream(List<Device> data, string channelName) =>
          GlobalData.DeviceService.ExportMemoryStream(data, channelName);


    public async Task ImportDeviceAsync(Dictionary<string, ImportPreviewOutputBase> input, bool restart = true)
    {
        try
        {
            await WaitLock.WaitAsync().ConfigureAwait(false);

            var result = await GlobalData.DeviceService.ImportDeviceAsync(input).ConfigureAwait(false);

            var newDeviceRuntimes = (await GlobalData.DeviceService.GetAllAsync().ConfigureAwait(false)).Where(a => result.Contains(a.Id)).Adapt<List<DeviceRuntime>>();

            if (restart)
            {
                //先找出线程管理器，停止
                var data = GlobalData.Devices.Where(a => newDeviceRuntimes.Select(a => a.Id).ToHashSet().Contains(a.Key)).GroupBy(a => a.Value.ChannelRuntime?.DeviceThreadManage);
                foreach (var group in data)
                {
                    if (group.Key != null)
                        await group.Key.RemoveDeviceAsync(group.Select(a => a.Value.Id)).ConfigureAwait(false);
                }
            }

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
                    deviceRuntime.VariableRuntimes.ParallelForEach(a => a.Value.Init(newDeviceRuntime));
                }
            }

            //根据条件重启通道线程
            if (restart)
            {
                foreach (var group in newDeviceRuntimes.Where(a => a.ChannelRuntime?.DeviceThreadManage != null).GroupBy(a => a.ChannelRuntime))
                {
                    if (group.Key?.DeviceThreadManage != null)
                        await group.Key.DeviceThreadManage.RestartDeviceAsync(group, false).ConfigureAwait(false);
                }
            }


        }
        finally
        {
            WaitLock.Release();
        }

    }

    public async Task<bool> SaveDeviceAsync(Device input, ItemChangedType type, bool restart = true)
    {
        try
        {
            input = input.Adapt<Device>();

            await WaitLock.WaitAsync().ConfigureAwait(false);

            var result = await GlobalData.DeviceService.SaveDeviceAsync(input, type).ConfigureAwait(false);

            var newDeviceRuntime = (await GlobalData.DeviceService.GetAllAsync().ConfigureAwait(false)).FirstOrDefault(a => a.Id == input.Id)?.Adapt<DeviceRuntime>();

            if (newDeviceRuntime == null) return false;


            //批量修改之后，需要重新加载通道
            if (GlobalData.Devices.TryGetValue(newDeviceRuntime.Id, out var deviceRuntime))
            {
                if (restart)
                {

                    if (GlobalData.TryGetDeviceThreadManage(deviceRuntime, out var deviceThreadManage))
                        await deviceThreadManage.RemoveDeviceAsync(deviceRuntime.Id).ConfigureAwait(false);
                }
                deviceRuntime.Dispose();
                deviceRuntime.VariableRuntimes.ParallelForEach(a => a.Value.Init(newDeviceRuntime));
            }

            if (GlobalData.Channels.TryGetValue(newDeviceRuntime.ChannelId, out var channelRuntime))
            {
                newDeviceRuntime.Init(channelRuntime);
            }
            if (restart)
            {
                //根据条件重启通道线程
                await channelRuntime.DeviceThreadManage.RestartDeviceAsync(newDeviceRuntime, false).ConfigureAwait(false);
            }

            return true;
        }
        finally
        {
            WaitLock.Release();
        }
    }
}