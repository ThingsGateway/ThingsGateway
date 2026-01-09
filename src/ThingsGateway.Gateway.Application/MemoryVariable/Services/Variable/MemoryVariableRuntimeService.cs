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

public class MemoryVariableRuntimeService : IMemoryVariableRuntimeService
{
    //private WaitLock WaitLock { get; set; } = new WaitLock();
    private ILogger _logger;
    public MemoryVariableRuntimeService(ILogger<MemoryVariableRuntimeService> logger)
    {
        _logger = logger;
    }


    public Task<QueryData<MemoryVariableRuntime>> OnMemoryVariableQueryAsync(QueryPageOptions options)
    {
        var data = GlobalData.MemoryVariableRuntimes.Select(a => a.Value)
                .WhereIf(!options.SearchText.IsNullOrWhiteSpace(), a => a.Name.Contains(options.SearchText)).Cast<MemoryVariableRuntime>()
                .GetQueryData(options);
        return Task.FromResult(data);
    }

    public Task<List<MemoryVariable>> GetMemoryVariableListAsync(QueryPageOptions option, int max)
    {
        var models = GlobalData.MemoryVariableRuntimes.Select(a => a.Value)
        .WhereIf(!option.SearchText.IsNullOrWhiteSpace(), a => a.Name.Contains(option.SearchText)).Cast<MemoryVariableRuntime>()
        .GetData(option, out var total).AdaptListMemoryVariable();

        if (max > 0 && models.Count > max)
        {
            throw new("online Excel max data count 2000");
        }
        return Task.FromResult(models);

    }

    public Task<USheetDatas> ExportMemoryVariableAsync(List<MemoryVariable> models, string? sortName, SortOrder sortOrder)
    {
        return Task.FromResult(MemoryVariableServiceHelpers.ExportMemoryVariable(models, sortName, sortOrder));
    }



    public async Task<OperResult<object>> OnWriteMemoryVariableAsync(string name, string writeData)
    {
        if (GlobalData.MemoryVariableRuntimes.TryGetValue(name, out var variableRuntime))
        {
            var data = await variableRuntime.RpcAsync(writeData).ConfigureAwait(false);
            return data.GetOperResult();
        }
        return new OperResult<object>($"MemoryVariable with Name {name} not found.");
    }



    public async Task CopyMemoryVariableAsync(List<MemoryVariable> Model, int CopyCount, string CopyMemoryVariableNamePrefix, int CopyMemoryVariableNameSuffixNumber, bool AutoRestartThread)
    {

        List<MemoryVariable> variables = new();
        for (int i = 0; i < CopyCount; i++)
        {
            var variable = Model.AdaptListMemoryVariable();
            foreach (var item in variable)
            {
                item.Id = CommonUtils.GetSingleId();
                item.Name = $"{CopyMemoryVariableNamePrefix}{CopyMemoryVariableNameSuffixNumber + i}";
                variables.Add(item);
            }
        }
        await BatchSaveMemoryVariableAsync(variables, ItemChangedType.Add, AutoRestartThread).ConfigureAwait(false);
    }



    public async Task<bool> BatchSaveMemoryVariableAsync(List<MemoryVariable> input, ItemChangedType type, bool restart)
    {
        try
        {
            // await WaitLock.WaitAsync().ConfigureAwait(false);

            var result = await App.GetService<IMemoryVariableService>().BatchSaveVariableAsync(input.Where(a => !a.DynamicVariable).ToList(), type).ConfigureAwait(false);

            var newMemoryVariableRuntimes = input.AdaptListMemoryVariableRuntime();
            var variableIds = newMemoryVariableRuntimes.Select(a => a.Id).ToHashSet();
            //获取变量，先找到原插件线程，然后修改插件线程内的字典，再改动全局字典，最后刷新插件

            ConcurrentHashSet<IDriver> changedDriver = new();
            RuntimeServiceHelper.VariableRuntimesDispose(variableIds);
            RuntimeServiceHelper.AddCollectChangedDriver(newMemoryVariableRuntimes, changedDriver);
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

    public async Task<bool> BatchEditMemoryVariableAsync(List<MemoryVariable> models, MemoryVariable oldModel, MemoryVariable model, bool restart)
    {
        try
        {
            // await WaitLock.WaitAsync().ConfigureAwait(false);

            var result = await App.GetService<IMemoryVariableService>().BatchEditAsync(models, oldModel, model).ConfigureAwait(false);

            using var db = DbContext.GetDB<MemoryVariable>();
            var ids = models.Select(a => a.Id).ToHashSet();

            var newMemoryVariableRuntimes = (await db.Queryable<MemoryVariable>().Where(a => ids.Contains(a.Id)).ToListAsync().ConfigureAwait(false)).AdaptListMemoryVariableRuntime();

            var variableIds = newMemoryVariableRuntimes.Select(a => a.Id).ToHashSet();

            ConcurrentHashSet<IDriver> changedDriver = new();

            RuntimeServiceHelper.VariableRuntimesDispose(variableIds);
            RuntimeServiceHelper.AddCollectChangedDriver(newMemoryVariableRuntimes, changedDriver);
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

    public async Task<bool> DeleteMemoryVariableAsync(List<long> ids, bool restart)
    {
        try
        {
            // await WaitLock.WaitAsync().ConfigureAwait(false);

            var variableIds = ids.ToHashSet();

            var result = await App.GetService<IMemoryVariableService>().DeleteVariableAsync(variableIds).ConfigureAwait(false);

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

    public async Task<bool> ClearMemoryVariableAsync(bool restart)
    {
        try
        {
            // await WaitLock.WaitAsync().ConfigureAwait(false);

            var result = await App.GetService<IMemoryVariableService>().DeleteVariableAsync(GlobalData.MemoryVariableRuntimes.Select(a => a.Value.Id)).ConfigureAwait(false);

            ConcurrentHashSet<IDriver> changedDriver = new();
            var variableIds = GlobalData.MemoryVariableRuntimes.Select(a => a.Value.Id).ToHashSet();
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

    public Task<Dictionary<string, object>> ExportVariableAsync(GatewayExportFilter exportFilter) => App.GetService<IMemoryVariableService>().ExportVariableAsync(exportFilter);

    public async Task ImportVariableAsync(Dictionary<string, ImportPreviewOutputBase> input, bool restart)
    {
        try
        {
            // await WaitLock.WaitAsync().ConfigureAwait(false);

            var result = await App.GetService<IMemoryVariableService>().ImportVariableAsync(input).ConfigureAwait(false);

            using var db = DbContext.GetDB<MemoryVariable>();
            var newMemoryVariableRuntimes = (await db.Queryable<MemoryVariable>().Where(a => result.Contains(a.Id)).ToListAsync().ConfigureAwait(false)).AdaptListMemoryVariableRuntime();

            var variableIds = newMemoryVariableRuntimes.Select(a => a.Id).ToHashSet();

            ConcurrentHashSet<IDriver> changedDriver = new();
            RuntimeServiceHelper.VariableRuntimesDispose(variableIds);
            RuntimeServiceHelper.AddCollectChangedDriver(newMemoryVariableRuntimes, changedDriver);
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
    public async Task<Dictionary<string, ImportPreviewOutputBase>> ImportMemoryVariableUSheetDatasAsync(USheetDatas uSheetDatas, bool restart)
    {
        try
        {
            // await WaitLock.WaitAsync().ConfigureAwait(false);

            var data = await MemoryVariableServiceHelpers.ImportAsync(uSheetDatas).ConfigureAwait(false);

            if (data.Any(a => a.Value.HasError)) return data;

            var result = await App.GetService<IMemoryVariableService>().ImportVariableAsync((Dictionary<string, ImportPreviewOutputBase>)data).ConfigureAwait(false);

            using var db = DbContext.GetDB<MemoryVariable>();
            var newMemoryVariableRuntimes = (await db.Queryable<MemoryVariable>().Where(a => result.Contains(a.Id)).ToListAsync().ConfigureAwait(false)).AdaptListMemoryVariableRuntime();

            var variableIds = newMemoryVariableRuntimes.Select(a => a.Id).ToHashSet();

            ConcurrentHashSet<IDriver> changedDriver = new();
            RuntimeServiceHelper.VariableRuntimesDispose(variableIds);
            RuntimeServiceHelper.AddCollectChangedDriver(newMemoryVariableRuntimes, changedDriver);
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

    public async Task<Dictionary<string, ImportPreviewOutputBase>> ImportMemoryVariableAsync(IBrowserFile file, bool restart)
    {
        try
        {
            // await WaitLock.WaitAsync().ConfigureAwait(false);

            var data = await App.GetService<IMemoryVariableService>().PreviewAsync(file).ConfigureAwait(false);

            if (data.Any(a => a.Value.HasError)) return data;

            var result = await App.GetService<IMemoryVariableService>().ImportVariableAsync(data).ConfigureAwait(false);

            using var db = DbContext.GetDB<MemoryVariable>();
            var newMemoryVariableRuntimes = (await db.Queryable<MemoryVariable>().Where(a => result.Contains(a.Id)).ToListAsync().ConfigureAwait(false)).AdaptListMemoryVariableRuntime();

            var variableIds = newMemoryVariableRuntimes.Select(a => a.Id).ToHashSet();

            ConcurrentHashSet<IDriver> changedDriver = new();
            RuntimeServiceHelper.VariableRuntimesDispose(variableIds);
            RuntimeServiceHelper.AddCollectChangedDriver(newMemoryVariableRuntimes, changedDriver);
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

            var data = await App.GetService<IMemoryVariableService>().PreviewAsync(file).ConfigureAwait(false);

            if (data.Any(a => a.Value.HasError)) return data;

            var result = await App.GetService<IMemoryVariableService>().ImportVariableAsync(data).ConfigureAwait(false);

            using var db = DbContext.GetDB<MemoryVariable>();
            var newMemoryVariableRuntimes = (await db.Queryable<MemoryVariable>().Where(a => result.Contains(a.Id)).ToListAsync().ConfigureAwait(false)).AdaptListMemoryVariableRuntime();

            var variableIds = newMemoryVariableRuntimes.Select(a => a.Id).ToHashSet();

            ConcurrentHashSet<IDriver> changedDriver = new();
            RuntimeServiceHelper.VariableRuntimesDispose(variableIds);
            RuntimeServiceHelper.AddCollectChangedDriver(newMemoryVariableRuntimes, changedDriver);
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

    public async Task<Dictionary<string, ImportPreviewOutputBase>> ImportMemoryVariableFileAsync(string filePath, bool restart)
    {
        try
        {
            // await WaitLock.WaitAsync().ConfigureAwait(false);

            var data = await App.GetService<IMemoryVariableService>().PreviewAsync(filePath).ConfigureAwait(false);

            if (data.Any(a => a.Value.HasError)) return data;

            var result = await App.GetService<IMemoryVariableService>().ImportVariableAsync(data).ConfigureAwait(false);

            using var db = DbContext.GetDB<MemoryVariable>();
            var newMemoryVariableRuntimes = (await db.Queryable<MemoryVariable>().Where(a => result.Contains(a.Id)).ToListAsync().ConfigureAwait(false)).AdaptListMemoryVariableRuntime();

            var variableIds = newMemoryVariableRuntimes.Select(a => a.Id).ToHashSet();

            ConcurrentHashSet<IDriver> changedDriver = new();
            RuntimeServiceHelper.VariableRuntimesDispose(variableIds);
            RuntimeServiceHelper.AddCollectChangedDriver(newMemoryVariableRuntimes, changedDriver);
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



    public Task<Dictionary<string, ImportPreviewOutputBase>> PreviewAsync(IBrowserFile browserFile)
    {
        return App.GetService<IMemoryVariableService>().PreviewAsync(browserFile);
    }

    public async Task<bool> SaveMemoryVariableAsync(MemoryVariable input, ItemChangedType type, bool restart)
    {
        try
        {
            // await WaitLock.WaitAsync().ConfigureAwait(false);

            var result = await App.GetService<IMemoryVariableService>().SaveVariableAsync(input, type).ConfigureAwait(false);

            using var db = DbContext.GetDB<MemoryVariable>();
            var newMemoryVariableRuntimes = (await db.Queryable<MemoryVariable>().Where(a => a.Id == input.Id).ToListAsync().ConfigureAwait(false)).AdaptListMemoryVariableRuntime();

            var variableIds = newMemoryVariableRuntimes.Select(a => a.Id).ToHashSet();

            ConcurrentHashSet<IDriver> changedDriver = new();

            RuntimeServiceHelper.VariableRuntimesDispose(variableIds);
            RuntimeServiceHelper.AddCollectChangedDriver(newMemoryVariableRuntimes, changedDriver);
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

    public async Task<string> ExportMemoryVariableDataFileAsync(List<MemoryVariable> data, string deviceName)
    {
        var sheets = App.GetService<IMemoryVariableService>().ExportDictionary(data, deviceName);
        return await App.GetService<IImportExportService>().CreateFileAsync<MemoryVariable>(sheets, "MemoryVariable", false).ConfigureAwait(false);
    }
    public async Task<string> ExportMemoryVariableFileAsync(GatewayExportFilter exportFilter)
    {
        var sheets = await App.GetService<IMemoryVariableService>().ExportVariableAsync(exportFilter).ConfigureAwait(false);
        return await App.GetService<IImportExportService>().CreateFileAsync<MemoryVariable>(sheets, "MemoryVariable", false).ConfigureAwait(false);
    }
}