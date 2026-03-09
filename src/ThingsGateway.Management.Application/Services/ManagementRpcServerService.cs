//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using Microsoft.AspNetCore.Components.Forms;

using ThingsGateway.Authentication;

using TouchSocket.Core;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Rpc.DmtpRpc.Generators;

namespace ThingsGateway.Management.Application;

public partial class ManagementRpcServerService(DmtpActorContext dmtpActorContext) : IManagementRpcServer, IManagementUpgradeRpcServer, IHardwarePageService, IBackendLogService, IUpgradeRpcServer, IRpcLogService, IRestartService, IAuthenticationService, IChannelEnableService, IRedundancyHostedService, IRedundancyService, ITextFileReadService, IPluginPageService, IRealAlarmService, IGlobalDataService, IRulesEngineHostedService, IRulesPageService, IChannelPageService, IDevicePageService, IVariablePageService, IMemoryVariablePageService
{
    public Task<HardwareInfo> GetRealTimeHardwareInfo() => dmtpActorContext.Current.GetDmtpRpcActor().GetRealTimeHardwareInfoAsync(invokeOption);
    public Task<List<HistoryHardwareInfo>> GetHistoryHardwareInfos() => dmtpActorContext.Current.GetDmtpRpcActor().GetHistoryHardwareInfosAsync(invokeOption);


    private DmtpInvokeOption invokeOption = new DmtpInvokeOption(60000)//调用配置
    {
        FeedbackType = FeedbackType.WaitInvoke,//调用反馈类型
        SerializationType = SerializationType.Json,//序列化类型
    };

    public Task DeleteBackendLogAsync() => dmtpActorContext.Current.GetDmtpRpcActor().DeleteBackendLogAsync(invokeOption);

    public Task DeleteRpcLogAsync() => dmtpActorContext.Current.GetDmtpRpcActor().DeleteRpcLogAsync(invokeOption);


    public Task<List<BackendLog>> GetNewBackendLogAsync() => dmtpActorContext.Current.GetDmtpRpcActor().GetNewBackendLogAsync(invokeOption);

    public Task<QueryData<BackendLog>> BackendLogPageAsync(QueryPageOptions option) => dmtpActorContext.Current.GetDmtpRpcActor().BackendLogPageAsync(option, invokeOption);

    public Task<IDictionary<string, IDictionary<string, OperResult<object>>>> RpcAsync(ICallContext callContext, Dictionary<string, Dictionary<string, string>> deviceDatas) => TouchSocket.Rpc.DmtpRpc.Generators.ManagementRpcServerExtensions.RpcAsync(dmtpActorContext.Current.GetDmtpRpcActor(), deviceDatas, invokeOption);

    public Task<List<BackendLogDayStatisticsOutput>> BackendLogStatisticsByDayAsync(int day) => dmtpActorContext.Current.GetDmtpRpcActor().BackendLogStatisticsByDayAsync(day, invokeOption);



    public Task<List<RpcLog>> GetNewRpcLogAsync() => dmtpActorContext.Current.GetDmtpRpcActor().GetNewRpcLogAsync(invokeOption);

    public Task<QueryData<RpcLog>> RpcLogPageAsync(QueryPageOptions option) => dmtpActorContext.Current.GetDmtpRpcActor().RpcLogPageAsync(option, invokeOption);

    public Task<List<RpcLogDayStatisticsOutput>> RpcLogStatisticsByDayAsync(int day) => dmtpActorContext.Current.GetDmtpRpcActor().RpcLogStatisticsByDayAsync(day, invokeOption);

    public Task RestartServerAsync() => dmtpActorContext.Current.GetDmtpRpcActor().RestartServerAsync(invokeOption);

    public Task<string> UUIDAsync() => dmtpActorContext.Current.GetDmtpRpcActor().UUIDAsync(invokeOption);


    public Task<AuthorizeInfo> TryAuthorizeAsync(string password) => dmtpActorContext.Current.GetDmtpRpcActor().TryAuthorizeAsync(password, invokeOption);


    public Task<AuthorizeInfo> TryGetAuthorizeInfoAsync() => dmtpActorContext.Current.GetDmtpRpcActor().TryGetAuthorizeInfoAsync(invokeOption);


    public Task UnAuthorizeAsync() => dmtpActorContext.Current.GetDmtpRpcActor().UnAuthorizeAsync(invokeOption);

    public Task<bool> StartCollectChannelEnableAsync() => dmtpActorContext.Current.GetDmtpRpcActor().StartCollectChannelEnableAsync(invokeOption);


    public Task<bool> StartBusinessChannelEnableAsync() => dmtpActorContext.Current.GetDmtpRpcActor().StartBusinessChannelEnableAsync(invokeOption);

    public Task StartRedundancyTaskAsync() => dmtpActorContext.Current.GetDmtpRpcActor().StartRedundancyTaskAsync(invokeOption);

    public Task StopRedundancyTaskAsync() => dmtpActorContext.Current.GetDmtpRpcActor().StopRedundancyTaskAsync(invokeOption);

    public Task RedundancyForcedSync() => dmtpActorContext.Current.GetDmtpRpcActor().RedundancyForcedSyncAsync(invokeOption);

    public Task<LogLevel> RedundancyLogLevelAsync() => dmtpActorContext.Current.GetDmtpRpcActor().RedundancyLogLevelAsync(invokeOption);

    public Task SetRedundancyLogLevelAsync(LogLevel logLevel) => dmtpActorContext.Current.GetDmtpRpcActor().SetRedundancyLogLevelAsync(logLevel, invokeOption);

    public Task<string> RedundancyLogPathAsync() => dmtpActorContext.Current.GetDmtpRpcActor().RedundancyLogPathAsync(invokeOption);

    public Task EditRedundancyOptionAsync(RedundancyOptions input) => dmtpActorContext.Current.GetDmtpRpcActor().EditRedundancyOptionAsync(input, invokeOption);


    public Task<RedundancyOptions> GetRedundancyAsync() => dmtpActorContext.Current.GetDmtpRpcActor().GetRedundancyAsync(invokeOption);

    public Task<OperResult<string[]>> GetLogFilesAsync(string directoryPath) => dmtpActorContext.Current.GetDmtpRpcActor().GetLogFilesAsync(directoryPath, invokeOption);

    public Task<OperResult<LogData[]>> LastLogDataAsync(string file, TouchSocket.Core.LogLevel logLevel, int lineCount = 200) => dmtpActorContext.Current.GetDmtpRpcActor().LastLogDataAsync(file, logLevel, lineCount, invokeOption);
    public Task DeleteLogDataAsync(string path) => dmtpActorContext.Current.GetDmtpRpcActor().DeleteLogDataAsync(path, invokeOption);

    public Task<List<PluginInfo>> GetPluginsAsync(PluginTypeEnum? pluginType = null) => dmtpActorContext.Current.GetDmtpRpcActor().GetPluginsAsync(pluginType, invokeOption);
    public Task<List<ChannelTypeEnum>> OnChannelTypeQueryAsync(string pluginName) => dmtpActorContext.Current.GetDmtpRpcActor().OnChannelTypeQueryAsync(pluginName);

    public Task<QueryData<PluginInfo>> PluginPageAsync(QueryPageOptions options, PluginTypeEnum? pluginTypeEnum = null) => dmtpActorContext.Current.GetDmtpRpcActor().PluginPageAsync(options, pluginTypeEnum, invokeOption);

    public Task ReloadPluginAsync() => dmtpActorContext.Current.GetDmtpRpcActor().ReloadPluginAsync(invokeOption);



    public async Task SavePluginByPathAsync(PluginAddPathInput plugin)
    {
        //传递文件到远端
        PluginAddPathInput pluginAddPathInput = new PluginAddPathInput();
        pluginAddPathInput.MainFilePath = PathHelper.CombinePathReplace(PluginInfoUtil.TempDirName, Path.GetFileName(plugin.MainFilePath));
        foreach (var item in plugin.OtherFilePaths)
        {
            pluginAddPathInput.OtherFilePaths.Add(PathHelper.CombinePathReplace(PluginInfoUtil.TempDirName, Path.GetFileName(item)));
        }

        await FileServerHelpers.ClientPushFileFromService(dmtpActorContext.Current, plugin.MainFilePath, pluginAddPathInput.MainFilePath).ConfigureAwait(false);
        for (int i = 0; i < plugin.OtherFilePaths.Count; i++)
        {
            await FileServerHelpers.ClientPushFileFromService(dmtpActorContext.Current, plugin.OtherFilePaths[i], pluginAddPathInput.OtherFilePaths[i]).ConfigureAwait(false);
        }

        await dmtpActorContext.Current.GetDmtpRpcActor().SavePluginByPathAsync(pluginAddPathInput, invokeOption).ConfigureAwait(false);


    }

    public Task<IEnumerable<AlarmVariable>> GetCurrentUserRealAlarmVariablesAsync() => dmtpActorContext.Current.GetDmtpRpcActor().GetCurrentUserRealAlarmVariablesAsync(invokeOption);

    public Task<IEnumerable<SelectedItem>> GetCurrentUserDeviceSelectedItemsAsync(string searchText, int startIndex, int count) => dmtpActorContext.Current.GetDmtpRpcActor().GetCurrentUserDeviceSelectedItemsAsync(searchText, startIndex, count, invokeOption);


    public Task<QueryData<SelectedItem>> GetCurrentUserDeviceVariableSelectedItemsAsync(string deviceText, string searchText, int startIndex, int count) => dmtpActorContext.Current.GetDmtpRpcActor().GetCurrentUserDeviceVariableSelectedItemsAsync(deviceText, searchText, startIndex, count, invokeOption);


    public Task<TouchSocket.Core.LogLevel> RulesLogLevelAsync(long rulesId) => dmtpActorContext.Current.GetDmtpRpcActor().RulesLogLevelAsync(rulesId, invokeOption);
    public Task SetRulesLogLevelAsync(long rulesId, TouchSocket.Core.LogLevel logLevel) => dmtpActorContext.Current.GetDmtpRpcActor().SetRulesLogLevelAsync(rulesId, logLevel, invokeOption);
    public Task<string> RulesLogPathAsync(long rulesId) => dmtpActorContext.Current.GetDmtpRpcActor().RulesLogPathAsync(rulesId, invokeOption);
    public Task<Rules> GetRuleRuntimesAsync(long rulesId) => dmtpActorContext.Current.GetDmtpRpcActor().GetRuleRuntimesAsync(rulesId, invokeOption);


    public Task DeleteRuleRuntimesAsync(List<long> ids) => dmtpActorContext.Current.GetDmtpRpcActor().DeleteRuleRuntimesAsync(ids, invokeOption);

    public Task EditRuleRuntimesAsync(Rules rules) => dmtpActorContext.Current.GetDmtpRpcActor().EditRuleRuntimesAsync(rules, invokeOption);

    public Task ClearRulesAsync() => dmtpActorContext.Current.GetDmtpRpcActor().ClearRulesAsync(invokeOption);

    public Task<bool> DeleteRulesAsync(List<long> ids) => dmtpActorContext.Current.GetDmtpRpcActor().DeleteRulesAsync(ids, invokeOption);


    public Task<QueryData<Rules>> RulesPageAsync(QueryPageOptions option, FilterKeyValueAction filterKeyValueAction = null) => dmtpActorContext.Current.GetDmtpRpcActor().RulesPageAsync(option, filterKeyValueAction, invokeOption);

    public Task<bool> SaveRulesAsync(Rules input, ItemChangedType type) => dmtpActorContext.Current.GetDmtpRpcActor().SaveRulesAsync(input, type, invokeOption);

    public Task<string> GetPluginNameAsync(long channelId) => dmtpActorContext.Current.GetDmtpRpcActor().GetPluginNameAsync(channelId, invokeOption);

    public Task RestartChannelAsync(long channelId) =>
    dmtpActorContext.Current.GetDmtpRpcActor().RestartChannelAsync(channelId, invokeOption);
    public Task RestartChannelsAsync() =>
    dmtpActorContext.Current.GetDmtpRpcActor().RestartChannelsAsync(invokeOption);

    public Task<LogLevel> ChannelLogLevelAsync(long id) =>
        dmtpActorContext.Current.GetDmtpRpcActor().ChannelLogLevelAsync(id, invokeOption);

    public Task SetChannelLogLevelAsync(long id, LogLevel logLevel) =>
        dmtpActorContext.Current.GetDmtpRpcActor().SetChannelLogLevelAsync(id, logLevel, invokeOption);

    public Task CopyChannelAsync(int copyCount, string copyChannelNamePrefix, int copyChannelNameSuffixNumber,
        string copyDeviceNamePrefix, int copyDeviceNameSuffixNumber, long channelId, bool restart) =>
        dmtpActorContext.Current.GetDmtpRpcActor().CopyChannelAsync(copyCount, copyChannelNamePrefix, copyChannelNameSuffixNumber,
            copyDeviceNamePrefix, copyDeviceNameSuffixNumber, channelId, restart, invokeOption);

    public Task<QueryData<ChannelRuntime>> OnChannelQueryAsync(QueryPageOptions options) =>
        dmtpActorContext.Current.GetDmtpRpcActor().OnChannelQueryAsync(options, invokeOption);

    public Task<List<Channel>> GetChannelListAsync(QueryPageOptions options, int max = 0) =>
        dmtpActorContext.Current.GetDmtpRpcActor().GetChannelListAsync(options, max, invokeOption);

    public Task<bool> SaveChannelAsync(Channel input, ItemChangedType type, bool restart) =>
        dmtpActorContext.Current.GetDmtpRpcActor().SaveChannelAsync(input, type, restart, invokeOption);

    public Task<bool> BatchEditChannelAsync(List<Channel> models, Channel oldModel, Channel model, bool restart) =>
        dmtpActorContext.Current.GetDmtpRpcActor().BatchEditChannelAsync(models, oldModel, model, restart, invokeOption);


    public Task<bool> DeleteChannelAsync(List<long> ids, bool restart) =>
    dmtpActorContext.Current.GetDmtpRpcActor().DeleteChannelAsync(ids, restart, invokeOption);

    public Task<bool> ClearChannelAsync(bool restart) =>
        dmtpActorContext.Current.GetDmtpRpcActor().ClearChannelAsync(restart, invokeOption);

    public Task ImportChannelAsync(List<Channel> upData, List<Channel> insertData, bool restart) =>
        dmtpActorContext.Current.GetDmtpRpcActor().ImportChannelAsync(upData, insertData, restart, invokeOption);

    public Task<Dictionary<string, ImportPreviewOutputBase>> ImportChannelUSheetDatasAsync(USheetDatas input, bool restart) =>
        dmtpActorContext.Current.GetDmtpRpcActor().ImportChannelUSheetDatasAsync(input, restart, invokeOption);

    public Task<Dictionary<string, ImportPreviewOutputBase>> ImportChannelFileAsync(string filePath, bool restart) =>
        dmtpActorContext.Current.GetDmtpRpcActor().ImportChannelFileAsync(filePath, restart, invokeOption);

    public Task<USheetDatas> ExportChannelAsync(List<Channel> channels) =>
        dmtpActorContext.Current.GetDmtpRpcActor().ExportChannelAsync(channels, invokeOption);

    public Task<QueryData<SelectedItem>> OnChannelSelectedItemQueryAsync(VirtualizeQueryOption option) =>
        dmtpActorContext.Current.GetDmtpRpcActor().OnChannelSelectedItemQueryAsync(option, invokeOption);

    public async Task<Dictionary<string, ImportPreviewOutputBase>> ImportChannelAsync(IBrowserFile browserFile, bool restart)
    {


        //传递文件到远端
        var path = await browserFile.StorageLocal().ConfigureAwait(false);


        await FileServerHelpers.ClientPushFileFromService(dmtpActorContext.Current, path, path).ConfigureAwait(false);

        return await dmtpActorContext.Current.GetDmtpRpcActor().ImportChannelFileAsync(path, restart, invokeOption).ConfigureAwait(false);


    }

    public async Task<string> ExportChannelFileAsync(GatewayExportFilter exportFilter)
    {
        //传递文件到远端

        var path = await dmtpActorContext.Current.GetDmtpRpcActor().ExportChannelFileAsync(exportFilter, invokeOption).ConfigureAwait(false);

        string savePath = PathHelper.CombinePathReplace("wwwroot", dmtpActorContext.Current.Id.SanitizeFileName(), "exports", Path.GetFileName(path));

        await FileServerHelpers.ClientPullFileFromService(dmtpActorContext.Current, path, savePath).ConfigureAwait(false);

        return savePath;

    }

    public async Task<Dictionary<string, ImportPreviewOutputBase>> ImportDeviceAsync(IBrowserFile browserFile, bool restart)
    {


        //传递文件到远端
        var path = await browserFile.StorageLocal().ConfigureAwait(false);

        await FileServerHelpers.ClientPushFileFromService(dmtpActorContext.Current, path, path).ConfigureAwait(false);

        return await dmtpActorContext.Current.GetDmtpRpcActor().ImportDeviceFileAsync(path, restart, invokeOption).ConfigureAwait(false);

    }


    public async Task<string> ExportDeviceFileAsync(GatewayExportFilter exportFilter)
    {
        //传递文件到远端

        var path = await dmtpActorContext.Current.GetDmtpRpcActor().ExportDeviceFileAsync(exportFilter, invokeOption).ConfigureAwait(false);

        string savePath = PathHelper.CombinePathReplace("wwwroot", dmtpActorContext.Current.Id.SanitizeFileName(), "exports", Path.GetFileName(path));

        await FileServerHelpers.ClientPullFileFromService(dmtpActorContext.Current, path, savePath).ConfigureAwait(false);

        return savePath;

    }

    public async Task<Dictionary<string, ImportPreviewOutputBase>> ImportVariableAsync(IBrowserFile browserFile, bool restart)
    {

        //传递文件到远端
        var path = await browserFile.StorageLocal().ConfigureAwait(false);

        await FileServerHelpers.ClientPushFileFromService(dmtpActorContext.Current, path, path).ConfigureAwait(false);

        return await dmtpActorContext.Current.GetDmtpRpcActor().ImportVariableFileAsync(path, restart, invokeOption).ConfigureAwait(false);
    }

    public async Task<string> ExportVariableFileAsync(GatewayExportFilter exportFilter)
    {
        //传递文件到远端
        var path = await dmtpActorContext.Current.GetDmtpRpcActor().ExportVariableFileAsync(exportFilter, invokeOption).ConfigureAwait(false);

        string savePath = PathHelper.CombinePathReplace("wwwroot", dmtpActorContext.Current.Id.SanitizeFileName(), "exports", Path.GetFileName(path));

        await FileServerHelpers.ClientPullFileFromService(dmtpActorContext.Current, path, savePath).ConfigureAwait(false);

        return savePath;
    }

    public Task<string> GetChannelNameAsync(long channelId) =>
        dmtpActorContext.Current.GetDmtpRpcActor().GetChannelNameAsync(channelId, invokeOption);

    public Task SetDeviceLogLevelAsync(long id, LogLevel logLevel) =>
        dmtpActorContext.Current.GetDmtpRpcActor().SetDeviceLogLevelAsync(id, logLevel, invokeOption);

    public Task CopyDeviceAsync(int CopyCount, string CopyDeviceNamePrefix, int CopyDeviceNameSuffixNumber, long deviceId, bool AutoRestartThread) =>
        dmtpActorContext.Current.GetDmtpRpcActor().CopyDeviceAsync(CopyCount, CopyDeviceNamePrefix, CopyDeviceNameSuffixNumber, deviceId, AutoRestartThread, invokeOption);

    public Task<LogLevel> DeviceLogLevelAsync(long id) =>
        dmtpActorContext.Current.GetDmtpRpcActor().DeviceLogLevelAsync(id, invokeOption);

    public Task<bool> BatchEditDeviceAsync(List<Device> models, Device oldModel, Device model, bool restart) =>
        dmtpActorContext.Current.GetDmtpRpcActor().BatchEditDeviceAsync(models, oldModel, model, restart, invokeOption);

    public Task<bool> SaveDeviceAsync(Device input, ItemChangedType type, bool restart) =>
        dmtpActorContext.Current.GetDmtpRpcActor().SaveDeviceAsync(input, type, restart, invokeOption);

    public Task<bool> DeleteDeviceAsync(List<long> ids, bool restart) =>
        dmtpActorContext.Current.GetDmtpRpcActor().DeleteDeviceAsync(ids, restart, invokeOption);

    public Task<Dictionary<string, ImportPreviewOutputBase>> ImportDeviceUSheetDatasAsync(USheetDatas input, bool restart) =>
        dmtpActorContext.Current.GetDmtpRpcActor().ImportDeviceUSheetDatasAsync(input, restart, invokeOption);

    public Task<USheetDatas> ExportDeviceAsync(List<Device> devices) =>
        dmtpActorContext.Current.GetDmtpRpcActor().ExportDeviceAsync(devices, invokeOption);


    public Task<QueryData<SelectedItem>> OnRedundantDevicesQueryAsync(VirtualizeQueryOption option, long deviceId, long channelId) =>
        dmtpActorContext.Current.GetDmtpRpcActor().OnRedundantDevicesQueryAsync(option, deviceId, channelId, invokeOption);

    public Task<Dictionary<string, ImportPreviewOutputBase>> ImportDeviceFileAsync(string filePath, bool restart) =>
        dmtpActorContext.Current.GetDmtpRpcActor().ImportDeviceFileAsync(filePath, restart, invokeOption);

    public Task DeviceRedundantThreadAsync(long id) =>
        dmtpActorContext.Current.GetDmtpRpcActor().DeviceRedundantThreadAsync(id, invokeOption);

    public Task RestartDeviceAsync(long id, bool deleteCache) =>
        dmtpActorContext.Current.GetDmtpRpcActor().RestartDeviceAsync(id, deleteCache, invokeOption);

    public Task PauseThreadAsync(long id) =>
        dmtpActorContext.Current.GetDmtpRpcActor().PauseThreadAsync(id, invokeOption);

    public Task<QueryData<DeviceRuntime>> OnDeviceQueryAsync(QueryPageOptions options) =>
        dmtpActorContext.Current.GetDmtpRpcActor().OnDeviceQueryAsync(options, invokeOption);

    public Task<List<Device>> GetDeviceListAsync(QueryPageOptions option, int v) =>
        dmtpActorContext.Current.GetDmtpRpcActor().GetDeviceListAsync(option, v, invokeOption);

    public Task<bool> ClearDeviceAsync(bool restart) =>
        dmtpActorContext.Current.GetDmtpRpcActor().ClearDeviceAsync(restart, invokeOption);

    public Task<bool> IsRedundantDeviceAsync(long id) =>
        dmtpActorContext.Current.GetDmtpRpcActor().IsRedundantDeviceAsync(id, invokeOption);

    public Task<string> GetDeviceNameAsync(long redundantDeviceId) =>
        dmtpActorContext.Current.GetDmtpRpcActor().GetDeviceNameAsync(redundantDeviceId, invokeOption);

    public Task<bool> BatchEditVariableAsync(List<Variable> models, Variable oldModel, Variable model, bool restart) =>
    dmtpActorContext.Current.GetDmtpRpcActor().BatchEditVariableAsync(models, oldModel, model, restart, invokeOption);

    public Task<bool> DeleteVariableAsync(List<long> ids, bool restart) =>
        dmtpActorContext.Current.GetDmtpRpcActor().DeleteVariableAsync(ids, restart, invokeOption);

    public Task<bool> ClearVariableAsync(bool restart) =>
        dmtpActorContext.Current.GetDmtpRpcActor().ClearVariableAsync(restart, invokeOption);

    public Task InsertTestDataAsync(int testVariableCount, int testDeviceCount, string slaveUrl, bool businessEnable, bool restart) =>
        dmtpActorContext.Current.GetDmtpRpcActor().InsertTestDataAsync(testVariableCount, testDeviceCount, slaveUrl, businessEnable, restart, invokeOption);
    public Task InsertTestDtuDataAsync(int testDeviceCount, string slaveUrl, bool restart) =>
    dmtpActorContext.Current.GetDmtpRpcActor().InsertTestDtuDataAsync(testDeviceCount, slaveUrl, restart, invokeOption);

    public Task<bool> BatchSaveVariableAsync(List<Variable> input, ItemChangedType type, bool restart) =>
        dmtpActorContext.Current.GetDmtpRpcActor().BatchSaveVariableAsync(input, type, restart, invokeOption);

    public Task<bool> SaveVariableAsync(Variable input, ItemChangedType type, bool restart) =>
        dmtpActorContext.Current.GetDmtpRpcActor().SaveVariableAsync(input, type, restart, invokeOption);

    public Task CopyVariableAsync(List<Variable> Model, int CopyCount, string CopyVariableNamePrefix, int CopyVariableNameSuffixNumber, bool AutoRestartThread) =>
        dmtpActorContext.Current.GetDmtpRpcActor().CopyVariableAsync(Model, CopyCount, CopyVariableNamePrefix, CopyVariableNameSuffixNumber, AutoRestartThread, invokeOption);

    public Task<QueryData<ThingsGateway.Management.Application.VariableRuntime>> OnVariableQueryAsync(QueryPageOptions options) =>
        dmtpActorContext.Current.GetDmtpRpcActor().OnVariableQueryAsync(options, invokeOption);

    public Task<List<Variable>> GetVariableListAsync(QueryPageOptions option, int v) =>
        dmtpActorContext.Current.GetDmtpRpcActor().GetVariableListAsync(option, v, invokeOption);

    public Task<USheetDatas> ExportVariableAsync(List<Variable> models, string? sortName, SortOrder sortOrder) =>
        dmtpActorContext.Current.GetDmtpRpcActor().ExportVariableAsync(models, sortName, sortOrder, invokeOption);

    public Task<Dictionary<string, ImportPreviewOutputBase>> ImportVariableUSheetDatasAsync(USheetDatas data, bool restart) =>
        dmtpActorContext.Current.GetDmtpRpcActor().ImportVariableUSheetDatasAsync(data, restart, invokeOption);

    public Task<OperResult<object>> OnWriteVariableAsync(long id, string writeData) =>
        dmtpActorContext.Current.GetDmtpRpcActor().OnWriteVariableAsync(id, writeData, invokeOption);


    public Task<Dictionary<string, ImportPreviewOutputBase>> ImportVariableFileAsync(string filePath, bool restart) =>
        dmtpActorContext.Current.GetDmtpRpcActor().ImportVariableFileAsync(filePath, restart, invokeOption);

    public Task<QueryData<SelectedItem>> OnDeviceSelectedItemQueryAsync(VirtualizeQueryOption option, bool isCollect) =>
        dmtpActorContext.Current.GetDmtpRpcActor().OnDeviceSelectedItemQueryAsync(option, isCollect, invokeOption);

    public Task<string> GetDevicePluginNameAsync(long id) =>
        dmtpActorContext.Current.GetDmtpRpcActor().GetDevicePluginNameAsync(id, invokeOption);

    public Task<Dictionary<long, Tuple<string, string>>> GetDeviceIdNamesAsync() =>
        dmtpActorContext.Current.GetDmtpRpcActor().GetDeviceIdNamesAsync(invokeOption);

    public Task UpgradeAsync(ICallContext callContext, UpdateZipFile updateZipFile) => dmtpActorContext.Current.GetDmtpRpcActor().UpgradeAsync(updateZipFile, invokeOption);
    public Task<UpdateZipFileInput> GetUpdateZipFileInputAsync() => dmtpActorContext.Current.GetDmtpRpcActor().GetUpdateZipFileInputAsync(invokeOption);

    public Task RestartRuleRuntimeAsync() => dmtpActorContext.Current.GetDmtpRpcActor().RestartRuleRuntimeAsync(invokeOption);

    public async Task<string> ExportChannelDataFileAsync(List<Channel> data)
    {
        //传递文件到远端
        var path = await dmtpActorContext.Current.GetDmtpRpcActor().ExportChannelDataFileAsync(data, invokeOption).ConfigureAwait(false);

        string savePath = PathHelper.CombinePathReplace("wwwroot", dmtpActorContext.Current.Id.SanitizeFileName(), "exports", Path.GetFileName(path));

        await FileServerHelpers.ClientPullFileFromService(dmtpActorContext.Current, path, savePath).ConfigureAwait(false);

        return savePath;
    }

    public async Task<string> ExportDeviceDataFileAsync(List<Device> data, string channelName, string plugin)
    {
        //传递文件到远端
        var path = await dmtpActorContext.Current.GetDmtpRpcActor().ExportDeviceDataFileAsync(data, channelName, plugin, invokeOption).ConfigureAwait(false);

        string savePath = PathHelper.CombinePathReplace("wwwroot", dmtpActorContext.Current.Id.SanitizeFileName(), "exports", Path.GetFileName(path));

        await FileServerHelpers.ClientPullFileFromService(dmtpActorContext.Current, path, savePath).ConfigureAwait(false);

        return savePath;
    }

    public async Task<string> ExportVariableDataFileAsync(List<Variable> data, string devName)
    {
        //传递文件到远端
        var path = await dmtpActorContext.Current.GetDmtpRpcActor().ExportVariableDataFileAsync(data, devName, invokeOption).ConfigureAwait(false);

        string savePath = PathHelper.CombinePathReplace("wwwroot", dmtpActorContext.Current.Id.SanitizeFileName(), "exports", Path.GetFileName(path));

        await FileServerHelpers.ClientPullFileFromService(dmtpActorContext.Current, path, savePath).ConfigureAwait(false);

        return savePath;
    }

    public Task<QueryData<ThingsGateway.Management.Application.MemoryVariableRuntime>> OnMemoryVariableQueryAsync(QueryPageOptions options)
=> dmtpActorContext.Current.GetDmtpRpcActor().OnMemoryVariableQueryAsync(options, invokeOption);


    public Task<bool> BatchEditMemoryVariableAsync(List<MemoryVariable> models, MemoryVariable oldModel, MemoryVariable model, bool restart)
=> dmtpActorContext.Current.GetDmtpRpcActor().BatchEditMemoryVariableAsync(models, oldModel, model, restart, invokeOption);


    public Task<bool> DeleteMemoryVariableAsync(List<long> ids, bool restart)
=> dmtpActorContext.Current.GetDmtpRpcActor().DeleteMemoryVariableAsync(ids, restart, invokeOption);


    public Task<bool> ClearMemoryVariableAsync(bool restart)
=> dmtpActorContext.Current.GetDmtpRpcActor().ClearMemoryVariableAsync(restart, invokeOption);


    public Task<bool> BatchSaveMemoryVariableAsync(List<MemoryVariable> input, ItemChangedType type, bool restart)
=> dmtpActorContext.Current.GetDmtpRpcActor().BatchSaveMemoryVariableAsync(input, type, restart, invokeOption);


    public Task<bool> SaveMemoryVariableAsync(MemoryVariable input, ItemChangedType type, bool restart)
=> dmtpActorContext.Current.GetDmtpRpcActor().SaveMemoryVariableAsync(input, type, restart, invokeOption);


    public Task CopyMemoryVariableAsync(List<MemoryVariable> Model, int CopyCount, string CopyMemoryVariableNamePrefix, int CopyMemoryVariableNameSuffixNumber, bool AutoRestartThread)
=> dmtpActorContext.Current.GetDmtpRpcActor().CopyMemoryVariableAsync(Model, CopyCount, CopyMemoryVariableNamePrefix, CopyMemoryVariableNameSuffixNumber, AutoRestartThread, invokeOption);


    public Task<List<MemoryVariable>> GetMemoryVariableListAsync(QueryPageOptions option, int v)
=> dmtpActorContext.Current.GetDmtpRpcActor().GetMemoryVariableListAsync(option, v, invokeOption);


    public Task<USheetDatas> ExportMemoryVariableAsync(List<MemoryVariable> models, string? sortName, SortOrder sortOrder)
=> dmtpActorContext.Current.GetDmtpRpcActor().ExportMemoryVariableAsync(models, sortName, sortOrder, invokeOption);


    public Task<Dictionary<string, ImportPreviewOutputBase>> ImportMemoryVariableUSheetDatasAsync(USheetDatas data, bool restart)
=> dmtpActorContext.Current.GetDmtpRpcActor().ImportMemoryVariableUSheetDatasAsync(data, restart, invokeOption);


    public Task<OperResult<object>> OnWriteMemoryVariableAsync(string name, string writeData)
=> dmtpActorContext.Current.GetDmtpRpcActor().OnWriteMemoryVariableAsync(name, writeData, invokeOption);


    public async Task<Dictionary<string, ImportPreviewOutputBase>> ImportMemoryVariableAsync(IBrowserFile a, bool restart)
    {

        //传递文件到远端
        var path = await a.StorageLocal().ConfigureAwait(false);


        await FileServerHelpers.ClientPushFileFromService(dmtpActorContext.Current, path, path).ConfigureAwait(false);

        return await dmtpActorContext.Current.GetDmtpRpcActor().ImportMemoryVariableFileAsync(path, restart, invokeOption).ConfigureAwait(false);

    }


    public Task<Dictionary<string, ImportPreviewOutputBase>> ImportMemoryVariableFileAsync(string filePath, bool restart)
=> dmtpActorContext.Current.GetDmtpRpcActor().ImportMemoryVariableFileAsync(filePath, restart, invokeOption);



    public async Task<string> ExportMemoryVariableFileAsync(GatewayExportFilter exportFilter)
    {
        //传递文件到远端

        var path = await dmtpActorContext.Current.GetDmtpRpcActor().ExportMemoryVariableFileAsync(exportFilter, invokeOption).ConfigureAwait(false);

        string savePath = PathHelper.CombinePathReplace("wwwroot", dmtpActorContext.Current.Id.SanitizeFileName(), "exports", Path.GetFileName(path));

        await FileServerHelpers.ClientPullFileFromService(dmtpActorContext.Current, path, savePath).ConfigureAwait(false);

        return savePath;

    }

    public async Task<string> ExportMemoryVariableDataFileAsync(List<MemoryVariable> data, string devName)
    {
        //传递文件到远端
        var path = await dmtpActorContext.Current.GetDmtpRpcActor().ExportMemoryVariableDataFileAsync(data, devName, invokeOption).ConfigureAwait(false);

        string savePath = PathHelper.CombinePathReplace("wwwroot", dmtpActorContext.Current.Id.SanitizeFileName(), "exports", Path.GetFileName(path));

        await FileServerHelpers.ClientPullFileFromService(dmtpActorContext.Current, path, savePath).ConfigureAwait(false);

        return savePath;
    }

    public Task<VariableRuntime> GetVariableAsync(string devName, string varName)
=> dmtpActorContext.Current.GetDmtpRpcActor().GetVariableAsync(devName, varName, invokeOption);

}
