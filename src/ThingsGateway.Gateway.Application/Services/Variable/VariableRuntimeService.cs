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

public class VariableRuntimeService : IVariableRuntimeService
{
    //private WaitLock WaitLock { get; set; } = new WaitLock();
    private ILogger _logger;
    public VariableRuntimeService(ILogger<VariableRuntimeService> logger)
    {
        _logger = logger;
    }
    public Task<VariableRuntime> GetVariableAsync(string devName, string varName)
    {
        return Task.FromResult(GlobalData.GetVariable(devName, varName));
    }


    public Task<QueryData<VariableRuntime>> OnVariableQueryAsync(QueryPageOptions options)
    {
        var data = GlobalData.IdVariables.Select(a => a.Value).Where(a => !a.IsMemory)
                .WhereIf(!options.SearchText.IsNullOrWhiteSpace(), a => a.Name.Contains(options.SearchText))
                .GetQueryData(options);
        return Task.FromResult(data);
    }

    public Task<List<Variable>> GetVariableListAsync(QueryPageOptions option, int max)
    {
        var models = GlobalData.IdVariables.Select(a => a.Value)
        .WhereIf(!option.SearchText.IsNullOrWhiteSpace(), a => a.Name.Contains(option.SearchText))
        .GetData(option, out var total).Cast<Variable>().ToList();

        if (max > 0 && models.Count > max)
        {
            throw new("online Excel max data count 2000");
        }
        return Task.FromResult(models);

    }

    public Task<USheetDatas> ExportVariableAsync(List<Variable> models, string? sortName, SortOrder sortOrder)
    {
        return Task.FromResult(VariableServiceHelpers.ExportVariable(models, sortName, sortOrder));
    }



    public async Task<OperResult<object>> OnWriteVariableAsync(long id, string writeData)
    {
        if (GlobalData.IdVariables.TryGetValue(id, out var variableRuntime))
        {
            var data = await variableRuntime.RpcAsync(writeData).ConfigureAwait(false);
            return data.GetOperResult();
        }
        return new OperResult<object>($"Variable with ID {id} not found.");
    }



    public async Task CopyVariableAsync(List<Variable> Model, int CopyCount, string CopyVariableNamePrefix, int CopyVariableNameSuffixNumber, bool AutoRestartThread)
    {

        List<Variable> variables = new();
        for (int i = 0; i < CopyCount; i++)
        {
            var variable = Model.AdaptListVariable();
            foreach (var item in variable)
            {
                item.Id = CommonUtils.GetSingleId();
                item.Name = $"{CopyVariableNamePrefix}{CopyVariableNameSuffixNumber + i}";
                variables.Add(item);
            }
        }
        await BatchSaveVariableAsync(variables, ItemChangedType.Add, AutoRestartThread).ConfigureAwait(false);
    }



    public async Task<bool> BatchSaveVariableAsync(List<Variable> input, ItemChangedType type, bool restart)
    {
        try
        {
            // await WaitLock.WaitAsync().ConfigureAwait(false);

            var result = await GlobalData.VariableService.BatchSaveVariableAsync(input.Where(a => !a.DynamicVariable).ToList(), type).ConfigureAwait(false);

            var newVariableRuntimes = input.AdaptListVariableRuntime();
            var variableIds = newVariableRuntimes.Select(a => a.Id).ToHashSet();
            //获取变量，先找到原插件线程，然后修改插件线程内的字典，再改动全局字典，最后刷新插件

            ConcurrentHashSet<IDriver> changedDriver = new();
            RuntimeServiceHelper.VariableRuntimesDispose(variableIds);
            RuntimeServiceHelper.AddCollectChangedDriver(newVariableRuntimes, changedDriver);
            RuntimeServiceHelper.AddBusinessChangedDriver(variableIds, changedDriver);

            if (restart)
            {
                //根据条件重启通道线程
                await RuntimeServiceHelper.ChangedDriverAsync(changedDriver, _logger).ConfigureAwait(false);
            }
            return true;
        }
        finally
        {
            //WaitLock.Release();
        }
    }

    public async Task<bool> BatchEditVariableAsync(List<Variable> models, Variable oldModel, Variable model, bool restart)
    {
        try
        {
            // await WaitLock.WaitAsync().ConfigureAwait(false);

            var result = await GlobalData.VariableService.BatchEditAsync(models, oldModel, model).ConfigureAwait(false);

            using var db = DbContext.GetDB<Variable>();
            var ids = models.Select(a => a.Id).ToHashSet();

            var newVariableRuntimes = (await db.Queryable<Variable>().Where(a => ids.Contains(a.Id)).ToListAsync().ConfigureAwait(false)).AdaptListVariableRuntime();

            var variableIds = newVariableRuntimes.Select(a => a.Id).ToHashSet();

            ConcurrentHashSet<IDriver> changedDriver = new();

            RuntimeServiceHelper.VariableRuntimesDispose(variableIds);
            RuntimeServiceHelper.AddCollectChangedDriver(newVariableRuntimes, changedDriver);
            RuntimeServiceHelper.AddBusinessChangedDriver(variableIds, changedDriver);

            if (restart)
            {
                //根据条件重启通道线程
                await RuntimeServiceHelper.ChangedDriverAsync(changedDriver, _logger).ConfigureAwait(false);
            }

            return true;
        }
        finally
        {
            //WaitLock.Release();
        }
    }

    public async Task<bool> DeleteVariableAsync(List<long> ids, bool restart)
    {
        try
        {
            // await WaitLock.WaitAsync().ConfigureAwait(false);

            var variableIds = ids.ToHashSet();

            var result = await GlobalData.VariableService.DeleteVariableAsync(variableIds).ConfigureAwait(false);

            ConcurrentHashSet<IDriver> changedDriver = new();

            RuntimeServiceHelper.AddBusinessChangedDriver(variableIds, changedDriver);
            RuntimeServiceHelper.VariableRuntimesDispose(variableIds);

            if (restart)
            {
                await RuntimeServiceHelper.ChangedDriverAsync(changedDriver, _logger).ConfigureAwait(false);
            }

            return true;
        }
        finally
        {
            //WaitLock.Release();
        }
    }

    public async Task<bool> ClearVariableAsync(bool restart)
    {
        try
        {
            // await WaitLock.WaitAsync().ConfigureAwait(false);

            var result = await GlobalData.VariableService.DeleteVariableAsync(GlobalData.IdVariables.Keys).ConfigureAwait(false);

            ConcurrentHashSet<IDriver> changedDriver = new();
            var variableIds = GlobalData.IdVariables.Select(a => a.Key).ToHashSet();
            RuntimeServiceHelper.AddBusinessChangedDriver(variableIds, changedDriver);
            RuntimeServiceHelper.VariableRuntimesDispose(variableIds);

            if (restart)
            {
                await RuntimeServiceHelper.ChangedDriverAsync(changedDriver, _logger).ConfigureAwait(false);
            }

            return true;
        }
        finally
        {
            //WaitLock.Release();
        }
    }

    public Task<Dictionary<string, object>> ExportVariableAsync(GatewayExportFilter exportFilter) => GlobalData.VariableService.ExportVariableAsync(exportFilter);

    public async Task ImportVariableAsync(Dictionary<string, ImportPreviewOutputBase> input, bool restart)
    {
        try
        {
            // await WaitLock.WaitAsync().ConfigureAwait(false);

            var result = await GlobalData.VariableService.ImportVariableAsync(input).ConfigureAwait(false);

            using var db = DbContext.GetDB<Variable>();
            var newVariableRuntimes = (await db.Queryable<Variable>().Where(a => result.Contains(a.Id)).ToListAsync().ConfigureAwait(false)).AdaptListVariableRuntime();

            var variableIds = newVariableRuntimes.Select(a => a.Id).ToHashSet();

            ConcurrentHashSet<IDriver> changedDriver = new();
            RuntimeServiceHelper.VariableRuntimesDispose(variableIds);
            RuntimeServiceHelper.AddCollectChangedDriver(newVariableRuntimes, changedDriver);
            RuntimeServiceHelper.AddBusinessChangedDriver(variableIds, changedDriver);

            if (restart)
            {
                //根据条件重启通道线程
                await RuntimeServiceHelper.ChangedDriverAsync(changedDriver, _logger).ConfigureAwait(false);
            }

        }
        finally
        {
            //WaitLock.Release();
        }
    }
    public async Task<Dictionary<string, ImportPreviewOutputBase>> ImportVariableUSheetDatasAsync(USheetDatas uSheetDatas, bool restart)
    {
        try
        {
            // await WaitLock.WaitAsync().ConfigureAwait(false);

            var data = await VariableServiceHelpers.ImportAsync(uSheetDatas).ConfigureAwait(false);

            if (data.Any(a => a.Value.HasError)) return data;

            var result = await GlobalData.VariableService.ImportVariableAsync((Dictionary<string, ImportPreviewOutputBase>)data).ConfigureAwait(false);

            using var db = DbContext.GetDB<Variable>();
            var newVariableRuntimes = (await db.Queryable<Variable>().Where(a => result.Contains(a.Id)).ToListAsync().ConfigureAwait(false)).AdaptListVariableRuntime();

            var variableIds = newVariableRuntimes.Select(a => a.Id).ToHashSet();

            ConcurrentHashSet<IDriver> changedDriver = new();
            RuntimeServiceHelper.VariableRuntimesDispose(variableIds);
            RuntimeServiceHelper.AddCollectChangedDriver(newVariableRuntimes, changedDriver);
            RuntimeServiceHelper.AddBusinessChangedDriver(variableIds, changedDriver);

            if (restart)
            {
                //根据条件重启通道线程
                await RuntimeServiceHelper.ChangedDriverAsync(changedDriver, _logger).ConfigureAwait(false);
            }
            return data;
        }
        finally
        {
            //WaitLock.Release();
        }


    }

    public async Task<Dictionary<string, ImportPreviewOutputBase>> ImportVariableAsync(IBrowserFile file, bool restart)
    {
        try
        {
            // await WaitLock.WaitAsync().ConfigureAwait(false);

            var data = await GlobalData.VariableService.PreviewAsync(file).ConfigureAwait(false);

            if (data.Any(a => a.Value.HasError)) return data;

            var result = await GlobalData.VariableService.ImportVariableAsync(data).ConfigureAwait(false);

            using var db = DbContext.GetDB<Variable>();
            var newVariableRuntimes = (await db.Queryable<Variable>().Where(a => result.Contains(a.Id)).ToListAsync().ConfigureAwait(false)).AdaptListVariableRuntime();

            var variableIds = newVariableRuntimes.Select(a => a.Id).ToHashSet();

            ConcurrentHashSet<IDriver> changedDriver = new();
            RuntimeServiceHelper.VariableRuntimesDispose(variableIds);
            RuntimeServiceHelper.AddCollectChangedDriver(newVariableRuntimes, changedDriver);
            RuntimeServiceHelper.AddBusinessChangedDriver(variableIds, changedDriver);

            if (restart)
            {
                //根据条件重启通道线程
                await RuntimeServiceHelper.ChangedDriverAsync(changedDriver, _logger).ConfigureAwait(false);
            }
            return data;
        }
        finally
        {
            //WaitLock.Release();
        }
    }

    public async Task<Dictionary<string, ImportPreviewOutputBase>> ImportVariableAsync(IFormFile file, bool restart)
    {
        try
        {
            // await WaitLock.WaitAsync().ConfigureAwait(false);

            var data = await GlobalData.VariableService.PreviewAsync(file).ConfigureAwait(false);

            if (data.Any(a => a.Value.HasError)) return data;

            var result = await GlobalData.VariableService.ImportVariableAsync(data).ConfigureAwait(false);

            using var db = DbContext.GetDB<Variable>();
            var newVariableRuntimes = (await db.Queryable<Variable>().Where(a => result.Contains(a.Id)).ToListAsync().ConfigureAwait(false)).AdaptListVariableRuntime();

            var variableIds = newVariableRuntimes.Select(a => a.Id).ToHashSet();

            ConcurrentHashSet<IDriver> changedDriver = new();
            RuntimeServiceHelper.VariableRuntimesDispose(variableIds);
            RuntimeServiceHelper.AddCollectChangedDriver(newVariableRuntimes, changedDriver);
            RuntimeServiceHelper.AddBusinessChangedDriver(variableIds, changedDriver);

            if (restart)
            {
                //根据条件重启通道线程
                await RuntimeServiceHelper.ChangedDriverAsync(changedDriver, _logger).ConfigureAwait(false);
            }
            return data;
        }
        finally
        {
            //WaitLock.Release();
        }
    }

    public async Task<Dictionary<string, ImportPreviewOutputBase>> ImportVariableFileAsync(string filePath, bool restart)
    {
        try
        {
            // await WaitLock.WaitAsync().ConfigureAwait(false);

            var data = await GlobalData.VariableService.PreviewAsync(filePath).ConfigureAwait(false);

            if (data.Any(a => a.Value.HasError)) return data;

            var result = await GlobalData.VariableService.ImportVariableAsync(data).ConfigureAwait(false);

            using var db = DbContext.GetDB<Variable>();
            var newVariableRuntimes = (await db.Queryable<Variable>().Where(a => result.Contains(a.Id)).ToListAsync().ConfigureAwait(false)).AdaptListVariableRuntime();

            var variableIds = newVariableRuntimes.Select(a => a.Id).ToHashSet();

            ConcurrentHashSet<IDriver> changedDriver = new();
            RuntimeServiceHelper.VariableRuntimesDispose(variableIds);
            RuntimeServiceHelper.AddCollectChangedDriver(newVariableRuntimes, changedDriver);
            RuntimeServiceHelper.AddBusinessChangedDriver(variableIds, changedDriver);

            if (restart)
            {
                //根据条件重启通道线程
                await RuntimeServiceHelper.ChangedDriverAsync(changedDriver, _logger).ConfigureAwait(false);
            }
            return data;
        }
        finally
        {
            //WaitLock.Release();
        }
    }


    public async Task InsertTestDataAsync(int testVariableCount, int testDeviceCount, string slaveUrl, bool businessEnable, bool restart)
    {
        try
        {
            // await WaitLock.WaitAsync().ConfigureAwait(false);

            var datas = await GlobalData.VariableService.InsertTestDataAsync(testVariableCount, testDeviceCount, slaveUrl, businessEnable).ConfigureAwait(false);

            {
                var newChannelRuntimes = datas.Item1.AdaptListChannelRuntime();

                //批量修改之后，需要重新加载通道
                RuntimeServiceHelper.Init(newChannelRuntimes);


                var newDeviceRuntimes = datas.Item2.AdaptListDeviceRuntime();

                RuntimeServiceHelper.Init(newDeviceRuntimes);


                var newVariableRuntimes = datas.Item3.AdaptListVariableRuntime();
                RuntimeServiceHelper.Init(newVariableRuntimes);

                //根据条件重启通道线程

                if (restart)
                {
                    await GlobalData.ChannelThreadManage.RestartChannelAsync(newChannelRuntimes).ConfigureAwait(false);

                    await RuntimeServiceHelper.ChangedDriverAsync(GlobalData.GetAllVariableBusinessDeviceRuntime().Where(a => !newDeviceRuntimes.Contains(a)).ToArray(), _logger).ConfigureAwait(false);
                }
            }
        }
        finally
        {
            //WaitLock.Release();
        }
    }

    public async Task InsertTestDtuDataAsync(int deviceCount, string slaveUrl, bool restart)
    {
        try
        {
            // await WaitLock.WaitAsync().ConfigureAwait(false);

            var datas = await GlobalData.VariableService.InsertTestDtuDataAsync(deviceCount, slaveUrl).ConfigureAwait(false);

            {
                var newChannelRuntimes = datas.Item1.AdaptListChannelRuntime();

                //批量修改之后，需要重新加载通道
                RuntimeServiceHelper.Init(newChannelRuntimes);


                var newDeviceRuntimes = datas.Item2.AdaptListDeviceRuntime();

                RuntimeServiceHelper.Init(newDeviceRuntimes);

                var newVariableRuntimes = datas.Item3.AdaptListVariableRuntime();
                RuntimeServiceHelper.Init(newVariableRuntimes);

                //根据条件重启通道线程

                if (restart)
                {
                    await GlobalData.ChannelThreadManage.RestartChannelAsync(newChannelRuntimes).ConfigureAwait(false);

                    await RuntimeServiceHelper.ChangedDriverAsync(GlobalData.GetAllVariableBusinessDeviceRuntime().Where(a => !newDeviceRuntimes.Contains(a)).ToArray(), _logger).ConfigureAwait(false);
                }
            }
        }
        finally
        {
            //WaitLock.Release();
        }
    }

    public Task<Dictionary<string, ImportPreviewOutputBase>> PreviewAsync(IBrowserFile browserFile)
    {
        return GlobalData.VariableService.PreviewAsync(browserFile);
    }

    public async Task<bool> SaveVariableAsync(Variable input, ItemChangedType type, bool restart)
    {
        try
        {
            // await WaitLock.WaitAsync().ConfigureAwait(false);

            var result = await GlobalData.VariableService.SaveVariableAsync(input, type).ConfigureAwait(false);

            using var db = DbContext.GetDB<Variable>();
            var newVariableRuntimes = (await db.Queryable<Variable>().Where(a => a.Id == input.Id).ToListAsync().ConfigureAwait(false)).AdaptListVariableRuntime();

            var variableIds = newVariableRuntimes.Select(a => a.Id).ToHashSet();

            ConcurrentHashSet<IDriver> changedDriver = new();

            RuntimeServiceHelper.VariableRuntimesDispose(variableIds);
            RuntimeServiceHelper.AddCollectChangedDriver(newVariableRuntimes, changedDriver);
            RuntimeServiceHelper.AddBusinessChangedDriver(variableIds, changedDriver);

            if (restart)
            {
                //根据条件重启通道线程
                await RuntimeServiceHelper.ChangedDriverAsync(changedDriver, _logger).ConfigureAwait(false);
            }

            return true;
        }
        finally
        {
            //WaitLock.Release();
        }
    }

    public async Task<string> ExportVariableDataFileAsync(List<Variable> data, string deviceName)
    {
        var sheets = GlobalData.VariableService.ExportDictionary(data, deviceName);
        return await App.GetService<IImportExportService>().CreateFileAsync<Variable>(sheets, "Variable", false).ConfigureAwait(false);
    }
    public async Task<string> ExportVariableFileAsync(GatewayExportFilter exportFilter)
    {
        var sheets = await GlobalData.VariableService.ExportVariableAsync(exportFilter).ConfigureAwait(false);
        return await App.GetService<IImportExportService>().CreateFileAsync<Variable>(sheets, "Variable", false).ConfigureAwait(false);
    }
}