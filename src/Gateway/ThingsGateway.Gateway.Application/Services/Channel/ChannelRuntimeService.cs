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

using CSScripting;

using Mapster;

using Microsoft.AspNetCore.Components.Forms;

using ThingsGateway.Extension.Generic;
using ThingsGateway.NewLife;

namespace ThingsGateway.Gateway.Application;

public class ChannelRuntimeService : IChannelRuntimeService
{
    private WaitLock WaitLock { get; set; } = new WaitLock();
    public async Task<bool> BatchEditAsync(IEnumerable<Channel> models, Channel oldModel, Channel model)
    {
        try
        {
            await WaitLock.WaitAsync().ConfigureAwait(false);

            models = models.Adapt<List<Channel>>();
            oldModel = oldModel.Adapt<Channel>();
            model = model.Adapt<Channel>();
            var result = await GlobalData.ChannelService.BatchEditAsync(models, oldModel, model).ConfigureAwait(false);

            var newChannelRuntimes = (await GlobalData.ChannelService.GetAllAsync().ConfigureAwait(false)).Where(a => models.Select(a => a.Id).ToHashSet().Contains(a.Id)).Adapt<List<ChannelRuntime>>();

            //批量修改之后，需要重新加载通道
            foreach (var newChannelRuntime in newChannelRuntimes)
            {
                if (GlobalData.Channels.TryGetValue(newChannelRuntime.Id, out var channelRuntime))
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

            //根据条件重启通道线程
            await GlobalData.ChannelThreadManage.RestartChannelAsync(newChannelRuntimes).ConfigureAwait(false);

            return true;
        }
        finally
        {
            WaitLock.Release();
        }
    }

    public async Task<bool> DeleteChannelAsync(IEnumerable<long> ids)
    {
        try
        {
            await WaitLock.WaitAsync().ConfigureAwait(false);

            ids = ids.ToHashSet();
            var result = await GlobalData.ChannelService.DeleteChannelAsync(ids).ConfigureAwait(false);

            //批量修改之后，需要重新加载通道
            foreach (var id in ids)
            {
                if (GlobalData.Channels.TryGetValue(id, out var channelRuntime))
                {
                    channelRuntime.Dispose();

                    //也需要删除设备和变量
                    channelRuntime.DeviceRuntimes.ParallelForEach(a =>
                    {

                        a.Value.VariableRuntimes.ParallelForEach(v => v.Value.Dispose());
                        a.Value.Dispose();

                    });
                }

            }

            //根据条件重启通道线程
            await GlobalData.ChannelThreadManage.RemoveChannelAsync(ids).ConfigureAwait(false);

            return true;

        }
        finally
        {
            WaitLock.Release();
        }
    }
    public Task<Dictionary<string, ImportPreviewOutputBase>> PreviewAsync(IBrowserFile browserFile) => GlobalData.ChannelService.PreviewAsync(browserFile);

    public Task<Dictionary<string, object>> ExportChannelAsync(ExportFilter exportFilter) => GlobalData.ChannelService.ExportChannelAsync(exportFilter);
    public Task<MemoryStream> ExportMemoryStream(List<Channel> data) =>
      GlobalData.ChannelService.ExportMemoryStream(data);

    public async Task ImportChannelAsync(Dictionary<string, ImportPreviewOutputBase> input)
    {
        try
        {
            await WaitLock.WaitAsync().ConfigureAwait(false);

            var result = await GlobalData.ChannelService.ImportChannelAsync(input).ConfigureAwait(false);

            var newChannelRuntimes = (await GlobalData.ChannelService.GetAllAsync().ConfigureAwait(false)).Where(a => result.Contains(a.Id)).Adapt<List<ChannelRuntime>>();

            //批量修改之后，需要重新加载通道
            foreach (var newChannelRuntime in newChannelRuntimes)
            {
                if (GlobalData.Channels.TryGetValue(newChannelRuntime.Id, out var channelRuntime))
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

            //根据条件重启通道线程
            await GlobalData.ChannelThreadManage.RestartChannelAsync(newChannelRuntimes).ConfigureAwait(false);

        }

        finally
        {
            WaitLock.Release();
        }
    }
    public async Task<bool> SaveChannelAsync(Channel input, ItemChangedType type)
    {
        try
        {
            input = input.Adapt<Channel>();
            await WaitLock.WaitAsync().ConfigureAwait(false);

            var result = await GlobalData.ChannelService.SaveChannelAsync(input, type).ConfigureAwait(false);

            var newChannelRuntime = (await GlobalData.ChannelService.GetAllAsync().ConfigureAwait(false)).FirstOrDefault(a => a.Id == input.Id)?.Adapt<ChannelRuntime>();

            if (newChannelRuntime == null) return false;
            //批量修改之后，需要重新加载通道
            if (GlobalData.Channels.TryGetValue(newChannelRuntime.Id, out var channelRuntime))
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


            //根据条件重启通道线程
            await GlobalData.ChannelThreadManage.RestartChannelAsync(newChannelRuntime).ConfigureAwait(false);

            return true;
        }
        finally
        {
            WaitLock.Release();
        }
    }
}