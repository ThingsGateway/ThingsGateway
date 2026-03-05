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

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ThingsGateway.Gateway.Application;

public class DeviceRuntimeService : IDeviceRuntimeService
{
    private ILogger _logger;
    public DeviceRuntimeService(ILogger<DeviceRuntimeService> logger)
    {
        _logger = logger;
    }

    public Task<Dictionary<long, Tuple<string, string>>> GetDeviceIdNamesAsync()
    {

        return Task.FromResult(GlobalData.ReadOnlyIdDevices.ToDictionary(a => a.Key, a => Tuple.Create(a.Value.Name, a.Value.PluginName)));
    }
    public async Task<QueryData<SelectedItem>> OnDeviceSelectedItemQueryAsync(VirtualizeQueryOption option, bool isCollect)
    {
        var devices = await GlobalData.GetCurrentUserDevices().ConfigureAwait(false);

        return devices.Where(a => a.IsCollect == isCollect).WhereIf(!option.SearchText.IsNullOrWhiteSpace(), a => a.Name.Contains(option.SearchText)).GetQueryData(option, GatewayResourceUtil.BuildDeviceSelectList);

    }


    public Task<string> GetDevicePluginNameAsync(long id)
    {
        return Task.FromResult(GlobalData.TryGetDeviceRuntime(id, out var deviceRuntime) ? deviceRuntime.PluginName : string.Empty);
    }
    public Task<string> GetDeviceNameAsync(long redundantDeviceId)
    {
        return Task.FromResult(GlobalData.TryGetDeviceRuntime(redundantDeviceId, out var deviceRuntime) ? deviceRuntime.Name : string.Empty);
    }

    public Task<bool> IsRedundantDeviceAsync(long id)
    {
        return Task.FromResult(GlobalData.IsRedundant(id));
    }

    public Task<QueryData<DeviceRuntime>> OnDeviceQueryAsync(QueryPageOptions options)
    {
        var data = GlobalData.IdDevices.Select(a => a.Value)
                .WhereIf(!options.SearchText.IsNullOrWhiteSpace(), a => a.Name.Contains(options.SearchText))
                .GetQueryData(options);
        return Task.FromResult(data);
    }
    public Task<List<Device>> GetDeviceListAsync(QueryPageOptions options, int max = 0)
    {
        var models = GlobalData.IdDevices.Select(a => a.Value)
        .WhereIf(!options.SearchText.IsNullOrWhiteSpace(), a => a.Name.Contains(options.SearchText))
        .GetData(options, out var total).Cast<Device>().ToList();

        if (max > 0 && models.Count > max)
        {
            throw new("online Excel max data count 2000");
        }
        return Task.FromResult(models);
    }

    public Task<bool> ClearDeviceAsync(bool restart)
    {
        return DeleteDeviceAsync(GlobalData.IdChannels.Keys.ToList(), restart);
    }



    public async Task DeviceRedundantThreadAsync(long id)
    {
        if (GlobalData.TryGetDeviceRuntime(id, out var deviceRuntime) && GlobalData.TryGetDeviceThreadManage(deviceRuntime, out var deviceThreadManage))
        {
            await deviceThreadManage.DeviceRedundantThreadAsync(id, default).ConfigureAwait(false);
        }
    }
    public async Task RestartDeviceAsync(long id, bool deleteCache)
    {
        if (GlobalData.TryGetDeviceRuntime(id, out var deviceRuntime) && GlobalData.TryGetDeviceThreadManage(deviceRuntime, out var deviceThreadManage))
        {
            await deviceThreadManage.RestartDeviceAsync(deviceRuntime, deleteCache).ConfigureAwait(false);
        }
    }

    public Task PauseThreadAsync(long id)
    {
        if (GlobalData.TryGetDeviceRuntime(id, out var deviceRuntime))
        {
            deviceRuntime.Driver?.PauseThread(!deviceRuntime.Pause);
        }
        return Task.CompletedTask;
    }

    public Task<USheetDatas> ExportDeviceAsync(List<Device> devices)
    {
        return Task.FromResult(DeviceServiceHelpers.ExportDevice(devices));
    }


    private WaitLock WaitLock { get; set; } = new WaitLock(nameof(DeviceRuntimeService));

    public async Task<QueryData<SelectedItem>> OnRedundantDevicesQueryAsync(VirtualizeQueryOption option, long deviceId, long channelId)
    {

        var devices = await GlobalData.GetCurrentUserDevices().ConfigureAwait(false);
        var pluginName = GlobalData.ReadOnlyIdChannels.TryGetValue(channelId, out var channel) ? channel.PluginName : string.Empty;
        var ret = devices.WhereIf(!option.SearchText.IsNullOrWhiteSpace(), a => a.Name.Contains(option.SearchText))
            .Where(a => a.PluginName == pluginName && a.Id != deviceId).GetQueryData(option, GatewayResourceUtil.BuildDeviceSelectList
            );


        return ret;
    }

    public Task<TouchSocket.Core.LogLevel> DeviceLogLevelAsync(long id)
    {
        GlobalData.TryGetDeviceRuntime(id, out var DeviceRuntime);
        var data = DeviceRuntime?.Driver?.LogMessage?.LogLevel ?? TouchSocket.Core.LogLevel.Trace;
        return Task.FromResult(data);
    }


    public async Task SetDeviceLogLevelAsync(long id, TouchSocket.Core.LogLevel logLevel)
    {
        if (GlobalData.TryGetDeviceRuntime(id, out var DeviceRuntime))
        {
            if (DeviceRuntime.Driver != null)
            {
                await DeviceRuntime.Driver.SetLogAsync(logLevel).ConfigureAwait(false);
            }
        }
    }




    public async Task CopyDeviceAsync(int CopyCount, string CopyDeviceNamePrefix, int CopyDeviceNameSuffixNumber, long deviceId, bool AutoRestartThread)
    {
        if (!GlobalData.TryGetDeviceRuntime(deviceId, out var deviceRuntime))
        {
            return;
        }

        Device Model = deviceRuntime.AdaptDevice();
        Model.Id = 0;
        var Variables = deviceRuntime.ReadOnlyVariableRuntimes.Select(a => a.Value).OrderBy(a => a.Id).AdaptListVariable();


        Dictionary<Device, List<Variable>> devices = new();
        for (int i = 0; i < CopyCount; i++)
        {
            Device device = Model.AdaptDevice();
            device.Id = CommonUtils.GetSingleId();
            device.Name = $"{CopyDeviceNamePrefix}{CopyDeviceNameSuffixNumber + i}";
            List<Variable> variables = new();

            foreach (var item in Variables)
            {
                Variable v = item.AdaptVariable();
                v.Id = CommonUtils.GetSingleId();
                v.DeviceId = device.Id;
                variables.Add(v);
            }
            devices.Add(device, variables);
        }

        await GlobalData.DeviceRuntimeService.CopyAsync(devices, AutoRestartThread).ConfigureAwait(false);
    }




    public async Task<bool> CopyAsync(Dictionary<Device, List<Variable>> devices, bool restart)
    {
        try
        {
            await WaitLock.WaitAsync().ConfigureAwait(false);

            var result = await GlobalData.DeviceService.CopyAsync(devices).ConfigureAwait(false);

            var deviceids = devices.Select(a => a.Key.Id).ToHashSet();
            var newDeviceRuntimes = await RuntimeServiceHelper.GetNewDeviceRuntimesAsync(deviceids).ConfigureAwait(false);

            await RuntimeServiceHelper.InitAsync(newDeviceRuntimes, _logger).ConfigureAwait(false);

            //根据条件重启通道线程
            if (restart)
            {
                await RuntimeServiceHelper.RestartDeviceAsync(newDeviceRuntimes).ConfigureAwait(false);
                await RuntimeServiceHelper.ChangedDriverAsync(GlobalData.GetAllVariableBusinessDeviceRuntime().Where(a => !newDeviceRuntimes.Contains(a)).ToArray(), _logger).ConfigureAwait(false);
            }

            return true;
        }
        finally
        {
            WaitLock.Release();
        }
    }





    public async Task<bool> BatchEditDeviceAsync(List<Device> models, Device oldModel, Device model, bool restart)
    {
        try
        {
            await WaitLock.WaitAsync().ConfigureAwait(false);

            var result = await GlobalData.DeviceService.BatchEditAsync(models, oldModel, model).ConfigureAwait(false);
            var deviceids = models.Select(a => a.Id).ToHashSet();

            var newDeviceRuntimes = await RuntimeServiceHelper.GetNewDeviceRuntimesAsync(deviceids).ConfigureAwait(false);

            if (restart)
            {
                var newDeciceIds = newDeviceRuntimes.Select(a => a.Id).ToHashSet();

                await RuntimeServiceHelper.RemoveDeviceAsync(newDeciceIds).ConfigureAwait(false);
            }

            RuntimeServiceHelper.Init(newDeviceRuntimes);

            //根据条件重启通道线程

            if (restart)
            {
                await RuntimeServiceHelper.RestartDeviceAsync(newDeviceRuntimes).ConfigureAwait(false);
            }

            return true;
        }
        finally
        {
            WaitLock.Release();
        }
    }

    public async Task<bool> DeleteDeviceAsync(List<long> ids, bool restart)
    {
        try
        {
            await WaitLock.WaitAsync().ConfigureAwait(false);

            var devids = ids.ToHashSet();

            var result = await GlobalData.DeviceService.DeleteDeviceAsync(devids).ConfigureAwait(false);

            //根据条件重启通道线程
            var deviceRuntimes = GlobalData.IdDevices.FilterByKeys(devids).Select(a => a.Value).ToList();

            ConcurrentHashSet<IDriver> changedDriver = RuntimeServiceHelper.DeleteDeviceRuntime(deviceRuntimes);

            if (restart)
            {
                await RuntimeServiceHelper.RemoveDeviceAsync(deviceRuntimes).ConfigureAwait(false);

                await RuntimeServiceHelper.ChangedDriverAsync(changedDriver, _logger).ConfigureAwait(false);
            }

            return true;
        }
        finally
        {
            WaitLock.Release();
        }
    }

    public Task<Dictionary<string, object>> ExportDeviceAsync(GatewayExportFilter exportFilter) => GlobalData.DeviceService.ExportDeviceAsync(exportFilter);
    public Task<Dictionary<string, ImportPreviewOutputBase>> PreviewAsync(IBrowserFile browserFile) => GlobalData.DeviceService.PreviewAsync(browserFile);


    public async Task ImportDeviceAsync(Dictionary<string, ImportPreviewOutputBase> input, bool restart)
    {
        try
        {
            await WaitLock.WaitAsync().ConfigureAwait(false);

            var deviceids = await GlobalData.DeviceService.ImportDeviceAsync(input).ConfigureAwait(false);

            var newDeviceRuntimes = await RuntimeServiceHelper.GetNewDeviceRuntimesAsync(deviceids).ConfigureAwait(false);

            if (restart)
            {
                var newDeciceIds = newDeviceRuntimes.Select(a => a.Id).ToHashSet();
                await RuntimeServiceHelper.RemoveDeviceAsync(newDeciceIds).ConfigureAwait(false);
            }

            //批量修改之后，需要重新加载通道
            RuntimeServiceHelper.Init(newDeviceRuntimes);

            //根据条件重启通道线程
            if (restart)
            {
                await RuntimeServiceHelper.RestartDeviceAsync(newDeviceRuntimes).ConfigureAwait(false);
            }
        }
        finally
        {
            WaitLock.Release();
        }
    }

    public async Task<Dictionary<string, ImportPreviewOutputBase>> ImportDeviceAsync(IBrowserFile file, bool restart)
    {
        try
        {
            await WaitLock.WaitAsync().ConfigureAwait(false);

            var data = await GlobalData.DeviceService.PreviewAsync(file).ConfigureAwait(false);

            if (data.Any(a => a.Value.HasError)) return data;

            var deviceids = await GlobalData.DeviceService.ImportDeviceAsync(data).ConfigureAwait(false);

            var newDeviceRuntimes = await RuntimeServiceHelper.GetNewDeviceRuntimesAsync(deviceids).ConfigureAwait(false);

            if (restart)
            {
                var newDeciceIds = newDeviceRuntimes.Select(a => a.Id).ToHashSet();
                await RuntimeServiceHelper.RemoveDeviceAsync(newDeciceIds).ConfigureAwait(false);
            }

            //批量修改之后，需要重新加载通道
            RuntimeServiceHelper.Init(newDeviceRuntimes);

            //根据条件重启通道线程
            if (restart)
            {
                await RuntimeServiceHelper.RestartDeviceAsync(newDeviceRuntimes).ConfigureAwait(false);
            }

            return data;
        }
        finally
        {
            WaitLock.Release();
        }
    }
    public async Task<Dictionary<string, ImportPreviewOutputBase>> ImportDeviceAsync(IFormFile file, bool restart)
    {
        try
        {
            await WaitLock.WaitAsync().ConfigureAwait(false);

            var data = await GlobalData.DeviceService.PreviewAsync(file).ConfigureAwait(false);

            if (data.Any(a => a.Value.HasError)) return data;

            var deviceids = await GlobalData.DeviceService.ImportDeviceAsync(data).ConfigureAwait(false);

            var newDeviceRuntimes = await RuntimeServiceHelper.GetNewDeviceRuntimesAsync(deviceids).ConfigureAwait(false);

            if (restart)
            {
                var newDeciceIds = newDeviceRuntimes.Select(a => a.Id).ToHashSet();
                await RuntimeServiceHelper.RemoveDeviceAsync(newDeciceIds).ConfigureAwait(false);
            }

            //批量修改之后，需要重新加载通道
            RuntimeServiceHelper.Init(newDeviceRuntimes);

            //根据条件重启通道线程
            if (restart)
            {
                await RuntimeServiceHelper.RestartDeviceAsync(newDeviceRuntimes).ConfigureAwait(false);
            }

            return data;
        }
        finally
        {
            WaitLock.Release();
        }
    }

    public async Task<Dictionary<string, ImportPreviewOutputBase>> ImportDeviceUSheetDatasAsync(USheetDatas input, bool restart)
    {
        try
        {
            await WaitLock.WaitAsync().ConfigureAwait(false);


            var data = await DeviceServiceHelpers.ImportAsync(input).ConfigureAwait(false);

            if (data.Any(a => a.Value.HasError)) return data;


            var deviceids = await GlobalData.DeviceService.ImportDeviceAsync(data).ConfigureAwait(false);

            var newDeviceRuntimes = await RuntimeServiceHelper.GetNewDeviceRuntimesAsync(deviceids).ConfigureAwait(false);

            if (restart)
            {
                var newDeciceIds = newDeviceRuntimes.Select(a => a.Id).ToHashSet();
                await RuntimeServiceHelper.RemoveDeviceAsync(newDeciceIds).ConfigureAwait(false);
            }

            //批量修改之后，需要重新加载通道
            RuntimeServiceHelper.Init(newDeviceRuntimes);

            //根据条件重启通道线程
            if (restart)
            {
                await RuntimeServiceHelper.RestartDeviceAsync(newDeviceRuntimes).ConfigureAwait(false);
            }
            return data;
        }
        finally
        {
            WaitLock.Release();
        }
    }

    public async Task<Dictionary<string, ImportPreviewOutputBase>> ImportDeviceFileAsync(string filePath, bool restart)
    {
        try
        {
            await WaitLock.WaitAsync().ConfigureAwait(false);

            var data = await GlobalData.DeviceService.PreviewAsync(filePath).ConfigureAwait(false);

            if (data.Any(a => a.Value.HasError)) return data;

            var deviceids = await GlobalData.DeviceService.ImportDeviceAsync(data).ConfigureAwait(false);

            var newDeviceRuntimes = await RuntimeServiceHelper.GetNewDeviceRuntimesAsync(deviceids).ConfigureAwait(false);

            if (restart)
            {
                var newDeciceIds = newDeviceRuntimes.Select(a => a.Id).ToHashSet();
                await RuntimeServiceHelper.RemoveDeviceAsync(newDeciceIds).ConfigureAwait(false);
            }

            //批量修改之后，需要重新加载通道
            RuntimeServiceHelper.Init(newDeviceRuntimes);

            //根据条件重启通道线程
            if (restart)
            {
                await RuntimeServiceHelper.RestartDeviceAsync(newDeviceRuntimes).ConfigureAwait(false);
            }
            return data;
        }
        finally
        {
            WaitLock.Release();
        }
    }



    public async Task<bool> SaveDeviceAsync(Device input, ItemChangedType type, bool restart)
    {
        try
        {
            await WaitLock.WaitAsync().ConfigureAwait(false);

            var result = await GlobalData.DeviceService.SaveDeviceAsync(input, type).ConfigureAwait(false);

            var newDeviceRuntimes = await RuntimeServiceHelper.GetNewDeviceRuntimesAsync(new HashSet<long>() { input.Id }).ConfigureAwait(false);

            RuntimeServiceHelper.Init(newDeviceRuntimes);

            if (restart)
            {
                //根据条件重启通道线程
                await RuntimeServiceHelper.RestartDeviceAsync(newDeviceRuntimes).ConfigureAwait(false);
            }

            return true;
        }
        finally
        {
            WaitLock.Release();
        }
    }

    public async Task<bool> BatchSaveDeviceAsync(List<Device> input, ItemChangedType type, bool restart)
    {
        try
        {
            await WaitLock.WaitAsync().ConfigureAwait(false);

            var result = await GlobalData.DeviceService.BatchSaveDeviceAsync(input, type).ConfigureAwait(false);

            var newDeviceRuntimes = await RuntimeServiceHelper.GetNewDeviceRuntimesAsync(input.Select(a => a.Id).ToHashSet()).ConfigureAwait(false);

            RuntimeServiceHelper.Init(newDeviceRuntimes);

            if (restart)
            {
                //根据条件重启通道线程
                await RuntimeServiceHelper.RestartDeviceAsync(newDeviceRuntimes).ConfigureAwait(false);
            }

            return true;
        }
        finally
        {
            WaitLock.Release();
        }
    }

    public async Task<string> ExportDeviceFileAsync(GatewayExportFilter exportFilter)
    {
        var sheets = await GlobalData.DeviceService.ExportDeviceAsync(exportFilter).ConfigureAwait(false);
        return await App.GetService<IImportExportService>().CreateFileAsync<Device>(sheets, "Device", false).ConfigureAwait(false);

    }

    public async Task<string> ExportDeviceDataFileAsync(List<Device> data, string channelName, string plugin)
    {
        var sheets = GlobalData.DeviceService.ExportDictionary(data, channelName, plugin);
        return await App.GetService<IImportExportService>().CreateFileAsync<Device>(sheets, "Device", false).ConfigureAwait(false);

    }

}