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
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace ThingsGateway.Gateway.Application;

public partial class ManagementRpcServer : IRpcServer, IManagementRpcServer, IHardwarePageService, IBackendLogService, IRpcLogService, IRestartService, IAuthenticationService, IChannelEnableService, IRedundancyHostedService, IRedundancyService, ITextFileReadService, IPluginPageService, IRealAlarmService, IChannelPageService, IDevicePageService, IVariablePageService, IMemoryVariablePageService
{
    public Task<HardwareInfo> GetRealTimeHardwareInfo() => App.GetService<IHardwarePageService>().GetRealTimeHardwareInfo();
    public Task<List<HistoryHardwareInfo>> GetHistoryHardwareInfos() => App.GetService<IHardwarePageService>().GetHistoryHardwareInfos();

    public Task<QueryData<BackendLog>> BackendLogPageAsync(QueryPageOptions option) => App.GetService<IBackendLogService>().BackendLogPageAsync(option);

    public Task<List<BackendLogDayStatisticsOutput>> BackendLogStatisticsByDayAsync(int day) => App.GetService<IBackendLogService>().BackendLogStatisticsByDayAsync(day);

    public Task<bool> BatchEditChannelAsync(List<Channel> models, Channel oldModel, Channel model, bool restart) =>
        App.GetService<IChannelPageService>().BatchEditChannelAsync(models, oldModel, model, restart);

    public Task<bool> BatchEditDeviceAsync(List<Device> models, Device oldModel, Device model, bool restart) =>
        App.GetService<IDevicePageService>().BatchEditDeviceAsync(models, oldModel, model, restart);

    public Task<bool> BatchEditVariableAsync(List<Variable> models, Variable oldModel, Variable model, bool restart) =>
        App.GetService<IVariablePageService>().BatchEditVariableAsync(models, oldModel, model, restart);

    public Task<bool> BatchSaveVariableAsync(List<Variable> input, ItemChangedType type, bool restart) =>
        App.GetService<IVariablePageService>().BatchSaveVariableAsync(input, type, restart);

    public Task<LogLevel> ChannelLogLevelAsync(long id) =>
        App.GetService<IChannelPageService>().ChannelLogLevelAsync(id);

    public Task<bool> ClearChannelAsync(bool restart) =>
        App.GetService<IChannelPageService>().ClearChannelAsync(restart);

    public Task<bool> ClearDeviceAsync(bool restart) =>
        App.GetService<IDevicePageService>().ClearDeviceAsync(restart);

    public Task ClearRulesAsync() => App.GetService<IRulesPageService>().ClearRulesAsync();

    public Task<bool> ClearVariableAsync(bool restart) =>
        App.GetService<IVariablePageService>().ClearVariableAsync(restart);

    public Task CopyChannelAsync(int copyCount, string copyChannelNamePrefix, int copyChannelNameSuffixNumber,
        string copyDeviceNamePrefix, int copyDeviceNameSuffixNumber, long channelId, bool restart) =>
        App.GetService<IChannelPageService>().CopyChannelAsync(copyCount, copyChannelNamePrefix, copyChannelNameSuffixNumber,
            copyDeviceNamePrefix, copyDeviceNameSuffixNumber, channelId, restart);

    public Task CopyDeviceAsync(int CopyCount, string CopyDeviceNamePrefix, int CopyDeviceNameSuffixNumber, long deviceId, bool AutoRestartThread) =>
    App.GetService<IDevicePageService>().CopyDeviceAsync(CopyCount, CopyDeviceNamePrefix, CopyDeviceNameSuffixNumber, deviceId, AutoRestartThread);

    public Task CopyVariableAsync(List<Variable> model, int copyCount, string copyVariableNamePrefix, int copyVariableNameSuffixNumber, bool restart) =>
        App.GetService<IVariablePageService>().CopyVariableAsync(model, copyCount, copyVariableNamePrefix, copyVariableNameSuffixNumber, restart);

    public Task DeleteBackendLogAsync() => App.GetService<IBackendLogService>().DeleteBackendLogAsync();
    public Task<bool> DeleteChannelAsync(List<long> ids, bool restart) =>
        App.GetService<IChannelPageService>().DeleteChannelAsync(ids, restart);

    public Task<bool> DeleteDeviceAsync(List<long> ids, bool restart) =>
        App.GetService<IDevicePageService>().DeleteDeviceAsync(ids, restart);

    public Task DeleteRpcLogAsync() => App.GetService<IRpcLogService>().DeleteRpcLogAsync();

    public Task DeleteRuleRuntimesAsync(List<long> ids) => App.GetService<IRulesEngineHostedService>().DeleteRuleRuntimesAsync(ids);

    public Task<bool> DeleteRulesAsync(List<long> ids) => App.GetService<IRulesPageService>().DeleteRulesAsync(ids);

    public Task<bool> DeleteVariableAsync(List<long> ids, bool restart) =>
    App.GetService<IVariablePageService>().DeleteVariableAsync(ids, restart);

    public Task<LogLevel> DeviceLogLevelAsync(long id) =>
        App.GetService<IDevicePageService>().DeviceLogLevelAsync(id);

    public Task DeviceRedundantThreadAsync(long id) =>
        App.GetService<IDevicePageService>().DeviceRedundantThreadAsync(id);

    public Task EditRedundancyOptionAsync(RedundancyOptions input) => App.GetService<IRedundancyService>().EditRedundancyOptionAsync(input);

    public Task EditRuleRuntimesAsync(Rules rules) => App.GetService<IRulesEngineHostedService>().EditRuleRuntimesAsync(rules);

    public Task<USheetDatas> ExportChannelAsync(List<Channel> channels) =>
        App.GetService<IChannelPageService>().ExportChannelAsync(channels);

    public Task<string> ExportChannelFileAsync(GatewayExportFilter exportFilter) =>
        App.GetService<IChannelPageService>().ExportChannelFileAsync(exportFilter);

    public Task<USheetDatas> ExportDeviceAsync(List<Device> devices) =>
        App.GetService<IDevicePageService>().ExportDeviceAsync(devices);

    public Task<string> ExportDeviceFileAsync(GatewayExportFilter exportFilter) =>
        App.GetService<IDevicePageService>().ExportDeviceFileAsync(exportFilter);

    public Task<USheetDatas> ExportVariableAsync(List<Variable> models, string? sortName, SortOrder sortOrder) =>
        App.GetService<IVariablePageService>().ExportVariableAsync(models, sortName, sortOrder);

    public Task<string> ExportVariableFileAsync(GatewayExportFilter exportFilter) => App.GetService<IVariablePageService>().ExportVariableFileAsync(exportFilter);

    public Task<List<Channel>> GetChannelListAsync(QueryPageOptions options, int max = 0) =>
        App.GetService<IChannelPageService>().GetChannelListAsync(options, max);

    public Task<string> GetChannelNameAsync(long id) =>
        App.GetService<IChannelPageService>().GetChannelNameAsync(id);

    public Task<IEnumerable<SelectedItem>> GetCurrentUserDeviceSelectedItemsAsync(string searchText, int startIndex, int count) => App.GetService<IGlobalDataService>().GetCurrentUserDeviceSelectedItemsAsync(searchText, startIndex, count);

    public Task<QueryData<SelectedItem>> GetCurrentUserDeviceVariableSelectedItemsAsync(string deviceText, string searchText, int startIndex, int count) => App.GetService<IGlobalDataService>().GetCurrentUserDeviceVariableSelectedItemsAsync(deviceText, searchText, startIndex, count);

    public Task<IEnumerable<AlarmVariable>> GetCurrentUserRealAlarmVariablesAsync() => App.GetService<IRealAlarmService>().GetCurrentUserRealAlarmVariablesAsync();

    public Task<Dictionary<long, Tuple<string, string>>> GetDeviceIdNamesAsync() => App.GetService<IDevicePageService>().GetDeviceIdNamesAsync();

    public Task<List<Device>> GetDeviceListAsync(QueryPageOptions option, int v) =>
        App.GetService<IDevicePageService>().GetDeviceListAsync(option, v);

    public Task<string> GetDeviceNameAsync(long redundantDeviceId) =>
        App.GetService<IDevicePageService>().GetDeviceNameAsync(redundantDeviceId);

    public Task<string> GetDevicePluginNameAsync(long id) =>
        App.GetService<IDevicePageService>().GetDevicePluginNameAsync(id);

    public Task<OperResult<string[]>> GetLogFilesAsync(string directoryPath) => App.GetService<ITextFileReadService>().GetLogFilesAsync(directoryPath);

    public Task<List<BackendLog>> GetNewBackendLogAsync() => App.GetService<IBackendLogService>().GetNewBackendLogAsync();
    public Task<List<RpcLog>> GetNewRpcLogAsync() => App.GetService<IRpcLogService>().GetNewRpcLogAsync();

    public Task<string> GetPluginNameAsync(long channelId) => App.GetService<IChannelPageService>().GetPluginNameAsync(channelId);

    public Task<List<PluginInfo>> GetPluginsAsync(PluginTypeEnum? pluginType = null) => App.GetService<IPluginPageService>().GetPluginsAsync(pluginType);
    public Task<List<ChannelTypeEnum>> OnChannelTypeQueryAsync(string pluginName) => App.GetService<IPluginPageService>().OnChannelTypeQueryAsync(pluginName);

    public Task<RedundancyOptions> GetRedundancyAsync() => App.GetService<IRedundancyService>().GetRedundancyAsync();

    public Task<Rules> GetRuleRuntimesAsync(long rulesId) => App.GetService<IRulesEngineHostedService>().GetRuleRuntimesAsync(rulesId);

    public Task<List<Variable>> GetVariableListAsync(QueryPageOptions option, int v) =>
        App.GetService<IVariablePageService>().GetVariableListAsync(option, v);

    public Task ImportChannelAsync(List<Channel> upData, List<Channel> insertData, bool restart) =>
        App.GetService<IChannelPageService>().ImportChannelAsync(upData, insertData, restart);

    public Task<Dictionary<string, ImportPreviewOutputBase>> ImportChannelAsync(IBrowserFile file, bool restart) =>
        App.GetService<IChannelPageService>().ImportChannelAsync(file, restart);

    public Task<Dictionary<string, ImportPreviewOutputBase>> ImportChannelFileAsync(string filePath, bool restart) =>
        App.GetService<IChannelPageService>().ImportChannelFileAsync(filePath, restart);

    public Task<Dictionary<string, ImportPreviewOutputBase>> ImportChannelUSheetDatasAsync(USheetDatas input, bool restart) =>
        App.GetService<IChannelPageService>().ImportChannelUSheetDatasAsync(input, restart);

    public Task<Dictionary<string, ImportPreviewOutputBase>> ImportDeviceAsync(IBrowserFile file, bool restart) =>
        App.GetService<IDevicePageService>().ImportDeviceAsync(file, restart);

    public Task<Dictionary<string, ImportPreviewOutputBase>> ImportDeviceFileAsync(string filePath, bool restart) =>
        App.GetService<IDevicePageService>().ImportDeviceFileAsync(filePath, restart);

    public Task<Dictionary<string, ImportPreviewOutputBase>> ImportDeviceUSheetDatasAsync(USheetDatas input, bool restart) =>
        App.GetService<IDevicePageService>().ImportDeviceUSheetDatasAsync(input, restart);

    public Task<Dictionary<string, ImportPreviewOutputBase>> ImportVariableAsync(IBrowserFile a, bool restart) =>
        App.GetService<IVariablePageService>().ImportVariableAsync(a, restart);

    public Task<Dictionary<string, ImportPreviewOutputBase>> ImportVariableFileAsync(string filePath, bool restart) =>
        App.GetService<IVariablePageService>().ImportVariableFileAsync(filePath, restart);

    public Task<Dictionary<string, ImportPreviewOutputBase>> ImportVariableUSheetDatasAsync(USheetDatas data, bool restart) =>
        App.GetService<IVariablePageService>().ImportVariableUSheetDatasAsync(data, restart);

    public Task InsertTestDataAsync(int testVariableCount, int testDeviceCount, string slaveUrl, bool businessEnable, bool restart) =>
        App.GetService<IVariablePageService>().InsertTestDataAsync(testVariableCount, testDeviceCount, slaveUrl, businessEnable, restart);

    public Task InsertTestDtuDataAsync(int testDeviceCount, string slaveUrl, bool restart) =>
        App.GetService<IVariablePageService>().InsertTestDtuDataAsync(testDeviceCount, slaveUrl, restart);


    public Task<bool> IsRedundantDeviceAsync(long id) =>
        App.GetService<IDevicePageService>().IsRedundantDeviceAsync(id);

    public Task<OperResult<LogData[]>> LastLogDataAsync(string file, TouchSocket.Core.LogLevel logLevel, int lineCount = 200) => App.GetService<ITextFileReadService>().LastLogDataAsync(file, logLevel, lineCount);
    public Task DeleteLogDataAsync(string path) => App.GetService<ITextFileReadService>().DeleteLogDataAsync(path);
    public Task<QueryData<ChannelRuntime>> OnChannelQueryAsync(QueryPageOptions options) =>
        App.GetService<IChannelPageService>().OnChannelQueryAsync(options);

    public Task<QueryData<SelectedItem>> OnChannelSelectedItemQueryAsync(VirtualizeQueryOption option) =>
        App.GetService<IChannelPageService>().OnChannelSelectedItemQueryAsync(option);

    public Task<QueryData<DeviceRuntime>> OnDeviceQueryAsync(QueryPageOptions options) =>
        App.GetService<IDevicePageService>().OnDeviceQueryAsync(options);

    public Task<QueryData<SelectedItem>> OnDeviceSelectedItemQueryAsync(VirtualizeQueryOption option, bool isCollect) =>
        App.GetService<IDevicePageService>().OnDeviceSelectedItemQueryAsync(option, isCollect);

    public Task<QueryData<SelectedItem>> OnRedundantDevicesQueryAsync(VirtualizeQueryOption option, long deviceId, long channelId) =>
        App.GetService<IDevicePageService>().OnRedundantDevicesQueryAsync(option, deviceId, channelId);

    public Task<QueryData<VariableRuntime>> OnVariableQueryAsync(QueryPageOptions options) =>
        App.GetService<IVariablePageService>().OnVariableQueryAsync(options);

    public Task<OperResult<object>> OnWriteVariableAsync(long id, string writeData) =>
        App.GetService<IVariablePageService>().OnWriteVariableAsync(id, writeData);

    public Task PauseThreadAsync(long id) =>
        App.GetService<IDevicePageService>().PauseThreadAsync(id);

    public Task<QueryData<PluginInfo>> PluginPageAsync(QueryPageOptions options, PluginTypeEnum? pluginTypeEnum = null) => App.GetService<IPluginPageService>().PluginPageAsync(options, pluginTypeEnum);

    public Task RedundancyForcedSync() => App.GetService<IRedundancyHostedService>().RedundancyForcedSync();

    public Task<LogLevel> RedundancyLogLevelAsync() => App.GetService<IRedundancyHostedService>().RedundancyLogLevelAsync();

    public Task<string> RedundancyLogPathAsync() => App.GetService<IRedundancyHostedService>().RedundancyLogPathAsync();

    public Task ReloadPluginAsync() => App.GetService<IPluginPageService>().ReloadPluginAsync();

    public Task RestartChannelAsync(long channelId) =>
    App.GetService<IChannelPageService>().RestartChannelAsync(channelId);

    public Task RestartChannelsAsync() =>
    App.GetService<IChannelPageService>().RestartChannelsAsync();

    public Task RestartDeviceAsync(long id, bool deleteCache) =>
        App.GetService<IDevicePageService>().RestartDeviceAsync(id, deleteCache);

    public Task RestartServerAsync() => App.GetService<IRestartService>().RestartServerAsync();

    public Task<IDictionary<string, IDictionary<string, OperResult<object>>>> RpcAsync(ICallContext callContext, Dictionary<string, Dictionary<string, string>> deviceDatas)
    {
        return GlobalData.RpcService.InvokeDeviceMethodAsync($"Management[{(callContext.Caller is IIdClient idClient ? idClient.Id : string.Empty)}]", deviceDatas, callContext.Token);
    }
    public Task<QueryData<RpcLog>> RpcLogPageAsync(QueryPageOptions option) => App.GetService<IRpcLogService>().RpcLogPageAsync(option);

    public Task<List<RpcLogDayStatisticsOutput>> RpcLogStatisticsByDayAsync(int day) => App.GetService<IRpcLogService>().RpcLogStatisticsByDayAsync(day);
    public Task<TouchSocket.Core.LogLevel> RulesLogLevelAsync(long rulesId) => App.GetService<IRulesEngineHostedService>().RulesLogLevelAsync(rulesId);

    public Task<string> RulesLogPathAsync(long rulesId) => App.GetService<IRulesEngineHostedService>().RulesLogPathAsync(rulesId);

    public Task<QueryData<Rules>> RulesPageAsync(QueryPageOptions option, FilterKeyValueAction filterKeyValueAction = null) => App.GetService<IRulesPageService>().RulesPageAsync(option, filterKeyValueAction);

    public Task<bool> SaveChannelAsync(Channel input, ItemChangedType type, bool restart) =>
        App.GetService<IChannelPageService>().SaveChannelAsync(input, type, restart);

    public Task<bool> SaveDeviceAsync(Device input, ItemChangedType type, bool restart) =>
        App.GetService<IDevicePageService>().SaveDeviceAsync(input, type, restart);

    public Task SavePluginByPathAsync(PluginAddPathInput plugin) => App.GetService<IPluginPageService>().SavePluginByPathAsync(plugin);

    public Task<bool> SaveRulesAsync(Rules input, ItemChangedType type) => App.GetService<IRulesPageService>().SaveRulesAsync(input, type);

    public Task<bool> SaveVariableAsync(Variable input, ItemChangedType type, bool restart) =>
        App.GetService<IVariablePageService>().SaveVariableAsync(input, type, restart);

    public Task SetChannelLogLevelAsync(long id, LogLevel logLevel) =>
        App.GetService<IChannelPageService>().SetChannelLogLevelAsync(id, logLevel);

    public Task SetDeviceLogLevelAsync(long id, LogLevel logLevel) =>
        App.GetService<IDevicePageService>().SetDeviceLogLevelAsync(id, logLevel);

    public Task SetRedundancyLogLevelAsync(LogLevel logLevel) => App.GetService<IRedundancyHostedService>().SetRedundancyLogLevelAsync(logLevel);

    public Task SetRulesLogLevelAsync(long rulesId, TouchSocket.Core.LogLevel logLevel) => App.GetService<IRulesEngineHostedService>().SetRulesLogLevelAsync(rulesId, logLevel);

    public Task<bool> StartBusinessChannelEnableAsync() => App.GetService<IChannelEnableService>().StartBusinessChannelEnableAsync();

    public Task<bool> StartCollectChannelEnableAsync() => App.GetService<IChannelEnableService>().StartCollectChannelEnableAsync();

    public Task StartRedundancyTaskAsync() => App.GetService<IRedundancyHostedService>().StartRedundancyTaskAsync();

    public Task StopRedundancyTaskAsync() => App.GetService<IRedundancyHostedService>().StopRedundancyTaskAsync();

    public Task<AuthorizeInfo> TryAuthorizeAsync(string password) => App.GetService<IAuthenticationService>().TryAuthorizeAsync(password);

    public Task<AuthorizeInfo> TryGetAuthorizeInfoAsync() => App.GetService<IAuthenticationService>().TryGetAuthorizeInfoAsync();

    public Task UnAuthorizeAsync() => App.GetService<IAuthenticationService>().UnAuthorizeAsync();

    public Task<string> UUIDAsync() => App.GetService<IAuthenticationService>().UUIDAsync();

    public Task RestartRuleRuntimeAsync() => App.GetService<IRulesEngineHostedService>().RestartRuleRuntimeAsync();

    public Task<string> ExportChannelDataFileAsync(List<Channel> data) => App.GetService<IChannelPageService>().ExportChannelDataFileAsync(data);


    public Task<string> ExportDeviceDataFileAsync(List<Device> data, string channelName, string plugin) => App.GetService<IDevicePageService>().ExportDeviceDataFileAsync(data, channelName, plugin);


    public Task<string> ExportVariableDataFileAsync(List<Variable> data, string devName) => App.GetService<IVariablePageService>().ExportVariableDataFileAsync(data, devName);

    public Task<QueryData<MemoryVariableRuntime>> OnMemoryVariableQueryAsync(QueryPageOptions options)
=> App.GetService<IMemoryVariablePageService>().OnMemoryVariableQueryAsync(options);

    public Task<bool> BatchEditMemoryVariableAsync(List<MemoryVariable> models, MemoryVariable oldModel, MemoryVariable model, bool restart)
=> App.GetService<IMemoryVariablePageService>().BatchEditMemoryVariableAsync(models, oldModel, model, restart);

    public Task<bool> BatchSaveMemoryVariableAsync(List<MemoryVariable> input, ItemChangedType type, bool restart)
=> App.GetService<IMemoryVariablePageService>().BatchSaveMemoryVariableAsync(input, type, restart);


    public Task<bool> SaveMemoryVariableAsync(MemoryVariable input, ItemChangedType type, bool restart)
=> App.GetService<IMemoryVariablePageService>().SaveMemoryVariableAsync(input, type, restart);


    public Task CopyMemoryVariableAsync(List<MemoryVariable> Model, int CopyCount, string CopyMemoryVariableNamePrefix, int CopyMemoryVariableNameSuffixNumber, bool AutoRestartThread)
=> App.GetService<IMemoryVariablePageService>().CopyMemoryVariableAsync(Model, CopyCount, CopyMemoryVariableNamePrefix, CopyMemoryVariableNameSuffixNumber, AutoRestartThread);


    public Task<List<MemoryVariable>> GetMemoryVariableListAsync(QueryPageOptions option, int v)
=> App.GetService<IMemoryVariablePageService>().GetMemoryVariableListAsync(option, v);



    public Task<USheetDatas> ExportMemoryVariableAsync(List<MemoryVariable> models, string? sortName, SortOrder sortOrder)
=> App.GetService<IMemoryVariablePageService>().ExportMemoryVariableAsync(models, sortName, sortOrder);

    public Task<OperResult<object>> OnWriteMemoryVariableAsync(string name, string writeData)
=> App.GetService<IMemoryVariablePageService>().OnWriteMemoryVariableAsync(name, writeData);

    public Task<string> ExportMemoryVariableDataFileAsync(List<MemoryVariable> data, string devName)
=> App.GetService<IMemoryVariablePageService>().ExportMemoryVariableDataFileAsync(data, devName);

    public Task<bool> DeleteMemoryVariableAsync(List<long> ids, bool restart)
=> App.GetService<IMemoryVariablePageService>().DeleteMemoryVariableAsync(ids, restart);


    public Task<bool> ClearMemoryVariableAsync(bool restart)
=> App.GetService<IMemoryVariablePageService>().ClearMemoryVariableAsync(restart);


    public Task<Dictionary<string, ImportPreviewOutputBase>> ImportMemoryVariableUSheetDatasAsync(USheetDatas data, bool restart)
=> App.GetService<IMemoryVariablePageService>().ImportMemoryVariableUSheetDatasAsync(data, restart);


    public Task<string> ExportMemoryVariableFileAsync(GatewayExportFilter exportFilter)
=> App.GetService<IMemoryVariablePageService>().ExportMemoryVariableFileAsync(exportFilter);


    public Task<Dictionary<string, ImportPreviewOutputBase>> ImportMemoryVariableFileAsync(string filePath, bool restart)
=> App.GetService<IMemoryVariablePageService>().ImportMemoryVariableFileAsync(filePath, restart);


    public Task<Dictionary<string, ImportPreviewOutputBase>> ImportMemoryVariableAsync(IBrowserFile a, bool restart)
=> App.GetService<IMemoryVariablePageService>().ImportMemoryVariableAsync(a, restart);

    public Task<VariableRuntime> GetVariableAsync(string devName, string varName)
=> App.GetService<IVariablePageService>().GetVariableAsync(devName, varName);
}
