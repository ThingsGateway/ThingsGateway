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

using ThingsGateway.Authentication;

using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;


namespace ThingsGateway.Gateway.Application;

#if Management
[GeneratorRpcProxy(GeneratorFlag = GeneratorFlag.ExtensionAsync)]
#endif
internal interface IManagementRpcServer : IRpcServer
{
    [DmtpRpc]
    Task<HardwareInfo> GetRealTimeHardwareInfo();
    [DmtpRpc]
    Task<List<HistoryHardwareInfo>> GetHistoryHardwareInfos();

    [DmtpRpc]
    Task<QueryData<BackendLog>> BackendLogPageAsync(QueryPageOptions option);

    [DmtpRpc]
    Task<List<BackendLogDayStatisticsOutput>> BackendLogStatisticsByDayAsync(int day);

    [DmtpRpc]
    Task<bool> BatchEditChannelAsync(List<Channel> models, Channel oldModel, Channel model, bool restart);

    [DmtpRpc]
    Task<bool> BatchEditDeviceAsync(List<Device> models, Device oldModel, Device model, bool restart);

    [DmtpRpc]
    Task<bool> BatchEditVariableAsync(List<Variable> models, Variable oldModel, Variable model, bool restart);

    [DmtpRpc]
    Task<bool> BatchSaveVariableAsync(List<Variable> input, ItemChangedType type, bool restart);

    [DmtpRpc]
    Task<TouchSocket.Core.LogLevel> ChannelLogLevelAsync(long id);

    [DmtpRpc]
    Task<bool> ClearChannelAsync(bool restart);

    [DmtpRpc]
    Task<bool> ClearDeviceAsync(bool restart);

    /// <summary>
    /// 清除所有规则
    /// </summary>
    [DmtpRpc]
    Task ClearRulesAsync();

    [DmtpRpc]
    Task<bool> ClearVariableAsync(bool restart);

    [DmtpRpc]
    Task CopyChannelAsync(int CopyCount, string CopyChannelNamePrefix, int CopyChannelNameSuffixNumber, string CopyDeviceNamePrefix, int CopyDeviceNameSuffixNumber, long channelId, bool AutoRestartThread);

    [DmtpRpc]
    Task CopyDeviceAsync(int CopyCount, string CopyDeviceNamePrefix, int CopyDeviceNameSuffixNumber, long deviceId, bool AutoRestartThread);

    [DmtpRpc]
    Task CopyVariableAsync(List<Variable> Model, int CopyCount, string CopyVariableNamePrefix, int CopyVariableNameSuffixNumber, bool AutoRestartThread);

    [DmtpRpc]
    Task DeleteBackendLogAsync();

    [DmtpRpc]
    Task<bool> DeleteChannelAsync(List<long> ids, bool restart);

    [DmtpRpc]
    Task<bool> DeleteDeviceAsync(List<long> ids, bool restart);
    [DmtpRpc]
    Task DeleteLogDataAsync(string path);

    /// <summary>
    /// 删除 RpcLog 表中的所有记录
    /// </summary>
    /// <remarks>
    /// 调用此方法会删除 RpcLog 表中的所有记录。
    /// </remarks>
    [DmtpRpc]
    Task DeleteRpcLogAsync();

    [DmtpRpc]
    Task DeleteRuleRuntimesAsync(List<long> ids);

    /// <summary>
    /// 删除规则
    /// </summary>
    /// <param name="ids">待删除规则的ID列表</param>
    [DmtpRpc]
    Task<bool> DeleteRulesAsync(List<long> ids);

    [DmtpRpc]
    Task<bool> DeleteVariableAsync(List<long> ids, bool restart);

    [DmtpRpc]
    Task<TouchSocket.Core.LogLevel> DeviceLogLevelAsync(long id);

    [DmtpRpc]
    Task DeviceRedundantThreadAsync(long id);

    /// <summary>
    /// 修改冗余设置
    /// </summary>
    /// <param name="input"></param>
    [DmtpRpc]
    Task EditRedundancyOptionAsync(RedundancyOptions input);

    [DmtpRpc]
    Task EditRuleRuntimesAsync(Rules rules);

    [DmtpRpc]
    Task<USheetDatas> ExportChannelAsync(List<Channel> channels);

    [DmtpRpc]
    Task<string> ExportChannelFileAsync(GatewayExportFilter exportFilter);

    [DmtpRpc]
    Task<USheetDatas> ExportDeviceAsync(List<Device> devices);

    [DmtpRpc]
    Task<string> ExportDeviceFileAsync(GatewayExportFilter exportFilter);

    [DmtpRpc]
    Task<USheetDatas> ExportVariableAsync(List<Variable> models, string? sortName, SortOrder sortOrder);

    [DmtpRpc]
    Task<string> ExportVariableFileAsync(GatewayExportFilter exportFilter);


    [DmtpRpc]
    Task<List<Channel>> GetChannelListAsync(QueryPageOptions options, int max = 0);

    [DmtpRpc]
    Task<string> GetChannelNameAsync(long channelId);

    [DmtpRpc]
    Task<IEnumerable<SelectedItem>> GetCurrentUserDeviceSelectedItemsAsync(string searchText, int startIndex, int count);

    [DmtpRpc]
    Task<QueryData<SelectedItem>> GetCurrentUserDeviceVariableSelectedItemsAsync(string deviceText, string searchText, int startIndex, int count);

    [DmtpRpc]
    Task<IEnumerable<AlarmVariable>> GetCurrentUserRealAlarmVariablesAsync();

    [DmtpRpc]
    Task<Dictionary<long, Tuple<string, string>>> GetDeviceIdNamesAsync();

    [DmtpRpc]
    Task<List<Device>> GetDeviceListAsync(QueryPageOptions option, int v);

    [DmtpRpc]
    Task<string> GetDeviceNameAsync(long redundantDeviceId);

    [DmtpRpc]
    Task<string> GetDevicePluginNameAsync(long id);

    [DmtpRpc]
    Task<OperResult<string[]>> GetLogFilesAsync(string directoryPath);

    [DmtpRpc]
    Task<List<BackendLog>> GetNewBackendLogAsync();

    /// <summary>
    /// 获取最新的十条 RpcLog 记录
    /// </summary>
    /// <returns>最新的十条记录</returns>
    [DmtpRpc]
    Task<List<RpcLog>> GetNewRpcLogAsync();

    [DmtpRpc]
    Task<string> GetPluginNameAsync(long channelId);

    /// <summary>
    /// 根据插件类型获取信息
    /// </summary>
    /// <param name="pluginType"></param>
    /// <returns></returns>
    [DmtpRpc]
    Task<List<PluginInfo>> GetPluginsAsync(PluginTypeEnum? pluginType = null);

    /// <summary>
    /// 获取冗余设置
    /// </summary>
    [DmtpRpc]
    Task<RedundancyOptions> GetRedundancyAsync();

    [DmtpRpc]
    Task<Rules> GetRuleRuntimesAsync(long rulesId);

    [DmtpRpc]
    Task<List<Variable>> GetVariableListAsync(QueryPageOptions option, int v);

    [DmtpRpc]
    Task ImportChannelAsync(List<Channel> upData, List<Channel> insertData, bool restart);

    [DmtpRpc]
    Task<Dictionary<string, ImportPreviewOutputBase>> ImportChannelFileAsync(string filePath, bool restart);

    [DmtpRpc]
    Task<Dictionary<string, ImportPreviewOutputBase>> ImportChannelUSheetDatasAsync(USheetDatas input, bool restart);

    [DmtpRpc]
    Task<Dictionary<string, ImportPreviewOutputBase>> ImportDeviceFileAsync(string filePath, bool restart);

    [DmtpRpc]
    Task<Dictionary<string, ImportPreviewOutputBase>> ImportDeviceUSheetDatasAsync(USheetDatas input, bool restart);

    [DmtpRpc]
    Task<Dictionary<string, ImportPreviewOutputBase>> ImportVariableFileAsync(string filePath, bool restart);

    [DmtpRpc]
    Task<Dictionary<string, ImportPreviewOutputBase>> ImportVariableUSheetDatasAsync(USheetDatas data, bool restart);

    [DmtpRpc]
    Task InsertTestDataAsync(int testVariableCount, int testDeviceCount, string slaveUrl, bool businessEnable, bool restart);
    [DmtpRpc]
    Task InsertTestDtuDataAsync(int testDeviceCount, string slaveUrl, bool restart);
    [DmtpRpc]
    Task<bool> IsRedundantDeviceAsync(long id);

    [DmtpRpc]
    Task<OperResult<LogData[]>> LastLogDataAsync(string file, TouchSocket.Core.LogLevel logLevel, int lineCount = 200);

#if !Management
    [DmtpRpc]
    Task<QueryData<ThingsGateway.Gateway.Application.ChannelRuntime>> OnChannelQueryAsync(QueryPageOptions options);
#else
    [DmtpRpc]
    Task<QueryData<ThingsGateway.Management.Application.ChannelRuntime>> OnChannelQueryAsync(QueryPageOptions options);
#endif
    [DmtpRpc]
    Task<QueryData<SelectedItem>> OnChannelSelectedItemQueryAsync(VirtualizeQueryOption option);

#if !Management
    [DmtpRpc]
    Task<QueryData<ThingsGateway.Gateway.Application.DeviceRuntime>> OnDeviceQueryAsync(QueryPageOptions options);
#else
    [DmtpRpc]
    Task<QueryData<ThingsGateway.Management.Application.DeviceRuntime>> OnDeviceQueryAsync(QueryPageOptions options);
#endif

    [DmtpRpc]
    Task<QueryData<SelectedItem>> OnDeviceSelectedItemQueryAsync(VirtualizeQueryOption option, bool isCollect);

    [DmtpRpc]
    Task<QueryData<SelectedItem>> OnRedundantDevicesQueryAsync(VirtualizeQueryOption option, long deviceId, long channelId);

#if !Management
    [DmtpRpc]
    Task<QueryData<ThingsGateway.Gateway.Application.VariableRuntime>> OnVariableQueryAsync(QueryPageOptions options);
#else
    [DmtpRpc]
    Task<QueryData<ThingsGateway.Management.Application.VariableRuntime>> OnVariableQueryAsync(QueryPageOptions options);
#endif

    [DmtpRpc]
    Task<OperResult<object>> OnWriteVariableAsync(long id, string writeData);

    [DmtpRpc]
    Task PauseThreadAsync(long id);

    /// <summary>
    /// 分页显示插件
    /// </summary>
    [DmtpRpc]
    Task<QueryData<PluginInfo>> PluginPageAsync(QueryPageOptions options, PluginTypeEnum? pluginTypeEnum = null);

    [DmtpRpc]
    Task RedundancyForcedSync();

    [DmtpRpc]
    Task<TouchSocket.Core.LogLevel> RedundancyLogLevelAsync();

    [DmtpRpc]
    Task<string> RedundancyLogPathAsync();

    /// <summary>
    /// 重载插件
    /// </summary>
    [DmtpRpc]
    Task ReloadPluginAsync();

    [DmtpRpc]
    Task RestartChannelAsync(long channelId);

    [DmtpRpc]
    Task RestartChannelsAsync();

    [DmtpRpc]
    Task RestartDeviceAsync(long id, bool deleteCache);
    [DmtpRpc]
    Task RestartRuleRuntimeAsync();

    [DmtpRpc]
    Task RestartServerAsync();

    [DmtpRpc]
    Task<IDictionary<string, IDictionary<string, OperResult<object>>>> RpcAsync(ICallContext callContext, Dictionary<string, Dictionary<string, string>> deviceDatas);
    /// <summary>
    /// 分页查询 RpcLog 数据
    /// </summary>
    /// <param name="option">查询选项</param>
    /// <returns>查询到的数据</returns>
    [DmtpRpc]
    Task<QueryData<RpcLog>> RpcLogPageAsync(QueryPageOptions option);

    /// <summary>
    /// 按天统计 RpcLog 数据
    /// </summary>
    /// <param name="day">统计的天数</param>
    /// <returns>按天统计的结果列表</returns>
    [DmtpRpc]
    Task<List<RpcLogDayStatisticsOutput>> RpcLogStatisticsByDayAsync(int day);
    [DmtpRpc]
    Task<TouchSocket.Core.LogLevel> RulesLogLevelAsync(long rulesId);

    [DmtpRpc]
    Task<string> RulesLogPathAsync(long rulesId);

    /// <summary>
    /// 报表查询
    /// </summary>
    /// <param name="option">查询条件</param>
    /// <param name="filterKeyValueAction">查询条件</param>
    [DmtpRpc]
    Task<QueryData<Rules>> RulesPageAsync(QueryPageOptions option, FilterKeyValueAction filterKeyValueAction = null);

    [DmtpRpc]
    Task<bool> SaveChannelAsync(Channel input, ItemChangedType type, bool restart);

    [DmtpRpc]
    Task<bool> SaveDeviceAsync(Device input, ItemChangedType type, bool restart);

    /// <summary>
    /// 添加插件
    /// </summary>
    /// <param name="plugin"></param>
    /// <returns></returns>
    [DmtpRpc]
    Task SavePluginByPathAsync(PluginAddPathInput plugin);

    /// <summary>
    /// 保存规则
    /// </summary>
    /// <param name="input">规则对象</param>
    /// <param name="type">保存类型</param>
    [DmtpRpc]
    Task<bool> SaveRulesAsync(Rules input, ItemChangedType type);

    [DmtpRpc]
    Task<bool> SaveVariableAsync(Variable input, ItemChangedType type, bool restart);

    [DmtpRpc]
    Task SetChannelLogLevelAsync(long id, TouchSocket.Core.LogLevel logLevel);

    [DmtpRpc]
    Task SetDeviceLogLevelAsync(long id, TouchSocket.Core.LogLevel logLevel);

    [DmtpRpc]
    Task SetRedundancyLogLevelAsync(TouchSocket.Core.LogLevel logLevel);

    [DmtpRpc]
    Task SetRulesLogLevelAsync(long rulesId, TouchSocket.Core.LogLevel logLevel);

    [DmtpRpc]
    Task<bool> StartBusinessChannelEnableAsync();

    [DmtpRpc]
    Task<bool> StartCollectChannelEnableAsync();

    [DmtpRpc]
    Task StartRedundancyTaskAsync();

    [DmtpRpc]
    Task StopRedundancyTaskAsync();

    [DmtpRpc]
    Task<AuthorizeInfo> TryAuthorizeAsync(string password);

    [DmtpRpc]
    Task<AuthorizeInfo> TryGetAuthorizeInfoAsync();

    [DmtpRpc]
    Task UnAuthorizeAsync();

    [DmtpRpc]
    Task<string> UUIDAsync();



    [DmtpRpc]
    Task<string> ExportChannelDataFileAsync(List<Channel> data);


    [DmtpRpc]
    Task<string> ExportDeviceDataFileAsync(List<Device> data, string channelName, string plugin);


    [DmtpRpc]
    Task<string> ExportVariableDataFileAsync(List<Variable> data, string devName);


    [DmtpRpc]
    Task<bool> BatchEditMemoryVariableAsync(List<MemoryVariable> models, MemoryVariable oldModel, MemoryVariable model, bool restart);
    [DmtpRpc]
    Task<bool> DeleteMemoryVariableAsync(List<long> ids, bool restart);
    [DmtpRpc]
    Task<bool> ClearMemoryVariableAsync(bool restart);

    [DmtpRpc]
    Task<bool> BatchSaveMemoryVariableAsync(List<MemoryVariable> input, ItemChangedType type, bool restart);

    [DmtpRpc]
    Task<bool> SaveMemoryVariableAsync(MemoryVariable input, ItemChangedType type, bool restart);
    [DmtpRpc]
    Task CopyMemoryVariableAsync(List<MemoryVariable> Model, int CopyCount, string CopyMemoryVariableNamePrefix, int CopyMemoryVariableNameSuffixNumber, bool AutoRestartThread);
    [DmtpRpc]
    Task<List<MemoryVariable>> GetMemoryVariableListAsync(QueryPageOptions option, int v);
#if !Management
    [DmtpRpc]
    Task<ThingsGateway.Gateway.Application.VariableRuntime> GetVariableAsync(string devName, string varName);
#else
    [DmtpRpc]
    Task<ThingsGateway.Management.Application.VariableRuntime> GetVariableAsync(string devName, string varName);
#endif
    [DmtpRpc]
    Task<USheetDatas> ExportMemoryVariableAsync(List<MemoryVariable> models, string? sortName, SortOrder sortOrder);
    [DmtpRpc]
    Task<Dictionary<string, ImportPreviewOutputBase>> ImportMemoryVariableUSheetDatasAsync(USheetDatas data, bool restart);

    [DmtpRpc]
    Task<string> ExportMemoryVariableFileAsync(GatewayExportFilter exportFilter);

    [DmtpRpc]
    Task<OperResult<object>> OnWriteMemoryVariableAsync(string name, string writeData);

    [DmtpRpc]
    Task<Dictionary<string, ImportPreviewOutputBase>> ImportMemoryVariableFileAsync(string filePath, bool restart);
    [DmtpRpc]
    Task<string> ExportMemoryVariableDataFileAsync(List<MemoryVariable> data, string devName);
#if !Management
    [DmtpRpc]
    Task<QueryData<ThingsGateway.Gateway.Application.MemoryVariableRuntime>> OnMemoryVariableQueryAsync(QueryPageOptions options);
#else
    [DmtpRpc]
    Task<QueryData<ThingsGateway.Management.Application.MemoryVariableRuntime>> OnMemoryVariableQueryAsync(QueryPageOptions options);
#endif

}