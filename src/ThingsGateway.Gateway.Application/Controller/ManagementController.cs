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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using ThingsGateway.Authentication;

using TouchSocket.Core;
using TouchSocket.Rpc;

namespace ThingsGateway.Gateway.Application;

[ApiDescriptionSettings("ThingsGateway.OpenApi", Order = 200)]
[Route("openApi/management/[action]")]
[RolePermission]
[RequestAudit]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
[TouchSocket.WebApi.Router("/openApi/management/[action]")]
[TouchSocket.WebApi.EnableCors("cors")]
public partial class ManagementController : ControllerBase, IRpcServer
{
    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<QueryData<BackendLog>> BackendLogPageAsync(QueryPageOptions option) => App.GetService<IBackendLogService>().BackendLogPageAsync(option);


    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<List<BackendLogDayStatisticsOutput>> BackendLogStatisticsByDayAsync(int day) => App.GetService<IBackendLogService>().BackendLogStatisticsByDayAsync(day);
    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<bool> BatchEditChannelAsync([FromBody] BatchEditChannelRequest request) =>
    App.GetService<IChannelPageService>()
       .BatchEditChannelAsync(request.Models, request.OldModel, request.Model, request.Restart);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<bool> BatchEditDeviceAsync([FromBody] BatchEditDeviceRequest request) =>
        App.GetService<IDevicePageService>()
           .BatchEditDeviceAsync(request.Models, request.OldModel, request.Model, request.Restart);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<bool> BatchEditVariableAsync([FromBody] BatchEditVariableRequest request) =>
        App.GetService<IVariablePageService>()
           .BatchEditVariableAsync(request.Models, request.OldModel, request.Model, request.Restart);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<bool> BatchSaveVariableAsync([FromBody] BatchSaveVariableRequest request) =>
        App.GetService<IVariablePageService>()
           .BatchSaveVariableAsync(request.Input, request.Type, request.Restart);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<LogLevel> ChannelLogLevelAsync(long id) =>
        App.GetService<IChannelPageService>().ChannelLogLevelAsync(id);
    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<bool> ClearChannelAsync(bool restart) =>
        App.GetService<IChannelPageService>().ClearChannelAsync(restart);
    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<bool> ClearDeviceAsync(bool restart) =>
        App.GetService<IDevicePageService>().ClearDeviceAsync(restart);
    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task ClearRulesAsync() => App.GetService<IRulesPageService>().ClearRulesAsync();
    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<bool> ClearVariableAsync(bool restart) =>
        App.GetService<IVariablePageService>().ClearVariableAsync(restart);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task CopyChannelAsync([FromBody] CopyChannelRequest request) =>
    App.GetService<IChannelPageService>()
       .CopyChannelAsync(request.CopyCount,
                         request.CopyChannelNamePrefix,
                         request.CopyChannelNameSuffixNumber,
                         request.CopyDeviceNamePrefix,
                         request.CopyDeviceNameSuffixNumber,
                         request.ChannelId,
                         request.Restart);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task CopyDeviceAsync([FromBody] CopyDeviceRequest request) =>
        App.GetService<IDevicePageService>()
           .CopyDeviceAsync(request.CopyCount,
                            request.CopyDeviceNamePrefix,
                            request.CopyDeviceNameSuffixNumber,
                            request.DeviceId,
                            request.AutoRestartThread);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task CopyVariableAsync([FromBody] CopyVariableRequest request) =>
        App.GetService<IVariablePageService>()
           .CopyVariableAsync(request.Model,
                              request.CopyCount,
                              request.CopyVariableNamePrefix,
                              request.CopyVariableNameSuffixNumber,
                              request.Restart);
    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task DeleteBackendLogAsync() => App.GetService<IBackendLogService>().DeleteBackendLogAsync();
    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<bool> DeleteChannelAsync([FromBody] DeleteRequest request) =>
    App.GetService<IChannelPageService>().DeleteChannelAsync(request.Ids, request.Restart);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<bool> DeleteDeviceAsync([FromBody] DeleteRequest request) =>
        App.GetService<IDevicePageService>().DeleteDeviceAsync(request.Ids, request.Restart);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<bool> DeleteVariableAsync([FromBody] DeleteRequest request) =>
        App.GetService<IVariablePageService>().DeleteVariableAsync(request.Ids, request.Restart);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task DeleteRpcLogAsync() => App.GetService<IRpcLogService>().DeleteRpcLogAsync();
    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task DeleteRuleRuntimesAsync(List<long> ids) => App.GetService<IRulesEngineHostedService>().DeleteRuleRuntimesAsync(ids);
    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<bool> DeleteRulesAsync(List<long> ids) => App.GetService<IRulesPageService>().DeleteRulesAsync(ids);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<LogLevel> DeviceLogLevelAsync(long id) =>
        App.GetService<IDevicePageService>().DeviceLogLevelAsync(id);
    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task DeviceRedundantThreadAsync(long id) =>
        App.GetService<IDevicePageService>().DeviceRedundantThreadAsync(id);
    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task EditRedundancyOptionAsync(RedundancyOptions input) => App.GetService<IRedundancyService>().EditRedundancyOptionAsync(input);
    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task EditRuleRuntimesAsync(Rules rules) => App.GetService<IRulesEngineHostedService>().EditRuleRuntimesAsync(rules);
    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<USheetDatas> ExportChannelAsync(List<Channel> channels) =>
        App.GetService<IChannelPageService>().ExportChannelAsync(channels);
    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<string> ExportChannelFileAsync(GatewayExportFilter exportFilter) =>
        App.GetService<IChannelPageService>().ExportChannelFileAsync(exportFilter);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<USheetDatas> ExportDeviceAsync(List<Device> devices) =>
        App.GetService<IDevicePageService>().ExportDeviceAsync(devices);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<string> ExportDeviceFileAsync(GatewayExportFilter exportFilter) =>
        App.GetService<IDevicePageService>().ExportDeviceFileAsync(exportFilter);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<USheetDatas> ExportVariableAsync(List<Variable> models, string? sortName, SortOrder sortOrder) =>
        App.GetService<IVariablePageService>().ExportVariableAsync(models, sortName, sortOrder);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<string> ExportVariableFileAsync(GatewayExportFilter exportFilter) => App.GetService<IVariablePageService>().ExportVariableFileAsync(exportFilter);


    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<List<Channel>> GetChannelListAsync([FromBody] GetListRequest<QueryPageOptions> request) =>
    App.GetService<IChannelPageService>().GetChannelListAsync(request.Options, request.Max);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<List<Device>> GetDeviceListAsync([FromBody] GetListRequest<QueryPageOptions> request) =>
        App.GetService<IDevicePageService>().GetDeviceListAsync(request.Options, request.Max);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<List<Variable>> GetVariableListAsync([FromBody] GetListRequest<QueryPageOptions> request) =>
        App.GetService<IVariablePageService>().GetVariableListAsync(request.Options, request.Max);

    [HttpGet]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Get)]
    public Task<string> GetChannelNameAsync(long id) =>
        App.GetService<IChannelPageService>().GetChannelNameAsync(id);
    [HttpGet]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Get)]
    public Task<IEnumerable<SelectedItem>> GetCurrentUserDeviceSelectedItemsAsync(string searchText, int startIndex, int count) => App.GetService<IGlobalDataService>().GetCurrentUserDeviceSelectedItemsAsync(searchText, startIndex, count);
    [HttpGet]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Get)]
    public Task<QueryData<SelectedItem>> GetCurrentUserDeviceVariableSelectedItemsAsync(string deviceText, string searchText, int startIndex, int count) => App.GetService<IGlobalDataService>().GetCurrentUserDeviceVariableSelectedItemsAsync(deviceText, searchText, startIndex, count);
    [HttpGet]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Get)]
    public Task<IEnumerable<AlarmVariable>> GetCurrentUserRealAlarmVariablesAsync() => App.GetService<IRealAlarmService>().GetCurrentUserRealAlarmVariablesAsync();
    [HttpGet]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Get)]
    public Task<Dictionary<long, Tuple<string, string>>> GetDeviceIdNamesAsync() => App.GetService<IDevicePageService>().GetDeviceIdNamesAsync();


    [HttpGet]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Get)]
    public Task<string> GetDeviceNameAsync(long redundantDeviceId) =>
        App.GetService<IDevicePageService>().GetDeviceNameAsync(redundantDeviceId);
    [HttpGet]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Get)]
    public Task<string> GetDevicePluginNameAsync(long id) =>
        App.GetService<IDevicePageService>().GetDevicePluginNameAsync(id);
    [HttpGet]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Get)]
    public Task<OperResult<string[]>> GetLogFilesAsync(string directoryPath) => App.GetService<ITextFileReadService>().GetLogFilesAsync(directoryPath);
    [HttpGet]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Get)]
    public Task<List<BackendLog>> GetNewBackendLogAsync() => App.GetService<IBackendLogService>().GetNewBackendLogAsync();
    [HttpGet]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Get)]
    public Task<List<RpcLog>> GetNewRpcLogAsync() => App.GetService<IRpcLogService>().GetNewRpcLogAsync();
    [HttpGet]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Get)]
    public Task<string> GetPluginNameAsync(long channelId) => App.GetService<IChannelPageService>().GetPluginNameAsync(channelId);
    [HttpGet]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Get)]
    public Task<List<PluginInfo>> GetPluginsAsync(PluginTypeEnum? pluginType = null) => App.GetService<IPluginPageService>().GetPluginsAsync(pluginType);
    [HttpGet]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Get)]
    public Task<RedundancyOptions> GetRedundancyAsync() => App.GetService<IRedundancyService>().GetRedundancyAsync();
    [HttpGet]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Get)]
    public Task<Rules> GetRuleRuntimesAsync(long rulesId) => App.GetService<IRulesEngineHostedService>().GetRuleRuntimesAsync(rulesId);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task ImportChannelDataAsync([FromBody] ImportChannelInput input) =>
       App.GetService<IChannelPageService>().ImportChannelAsync(input.UpData, input.InsertData, input.Restart);

    [HttpPost]
    public async Task<Dictionary<string, ImportPreviewOutputBase>> ImportChannelAsync([FromForm] ImportRequest request)
    {
        return (await App.GetService<IChannelRuntimeService>().ImportChannelAsync(request.File, request.Restart).ConfigureAwait(false)).AdaptImportPreviewOutputBases();

    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    [TouchSocket.WebApi.Router("/miniapi/management/ImportChannel")]
    public async Task<Dictionary<string, ImportPreviewOutputBase>> TSImportChannelAsync(TouchSocket.WebApi.IWebApiCallContext callContext)
    {
        var path = await callContext.HttpContext.Request.StorageLocalExcel().ConfigureAwait(false);

        return (await GlobalData.ChannelRuntimeService.ImportChannelFileAsync(path, true).ConfigureAwait(false)).AdaptImportPreviewOutputBases();

    }
    [ApiExplorerSettings(IgnoreApi = true)]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    [TouchSocket.WebApi.Router("/miniapi/management/ImportDevice")]
    public async Task<Dictionary<string, ImportPreviewOutputBase>> TSImportDeviceAsync(TouchSocket.WebApi.IWebApiCallContext callContext)
    {
        var path = await callContext.HttpContext.Request.StorageLocalExcel().ConfigureAwait(false);

        return (await GlobalData.DeviceRuntimeService.ImportDeviceFileAsync(path, true).ConfigureAwait(false)).AdaptImportPreviewOutputBases();

    }
    [ApiExplorerSettings(IgnoreApi = true)]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    [TouchSocket.WebApi.Router("/miniapi/management/ImportVariable")]
    public async Task<Dictionary<string, ImportPreviewOutputBase>> TSImportVariableAsync(TouchSocket.WebApi.IWebApiCallContext callContext)
    {
        var path = await callContext.HttpContext.Request.StorageLocalExcel().ConfigureAwait(false);

        return (await GlobalData.VariableRuntimeService.ImportVariableFileAsync(path, true).ConfigureAwait(false)).AdaptImportPreviewOutputBases();

    }



    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public async Task<Dictionary<string, ImportPreviewOutputBase>> ImportChannelUSheetDatasAsync([FromBody] ImportUSheetInput input)
    {
        return (await App.GetService<IChannelPageService>().ImportChannelUSheetDatasAsync(input.Input, input.Restart).ConfigureAwait(false)).AdaptImportPreviewOutputBases();

    }

    [HttpPost]
    public async Task<Dictionary<string, ImportPreviewOutputBase>> ImportDeviceAsync([FromForm] ImportRequest request)
    {

        return (await App.GetService<IDeviceRuntimeService>().ImportDeviceAsync(request.File, request.Restart).ConfigureAwait(false)).AdaptImportPreviewOutputBases();
    }


    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public async Task<Dictionary<string, ImportPreviewOutputBase>> ImportDeviceUSheetDatasAsync([FromBody] ImportUSheetInput input)
    {

        return (await App.GetService<IDevicePageService>().ImportDeviceUSheetDatasAsync(input.Input, input.Restart).ConfigureAwait(false)).AdaptImportPreviewOutputBases();
    }

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public async Task<Dictionary<string, ImportPreviewOutputBase>> ImportVariableAsync([FromForm] ImportRequest request)
    {
        return (await App.GetService<IVariableRuntimeService>().ImportVariableAsync(request.File, request.Restart).ConfigureAwait(false)).AdaptImportPreviewOutputBases();

    }


    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public async Task<Dictionary<string, ImportPreviewOutputBase>> ImportVariableUSheetDatasAsync([FromBody] ImportUSheetInput input)
    {
        return (await App.GetService<IVariablePageService>().ImportVariableUSheetDatasAsync(input.Input, input.Restart).ConfigureAwait(false)).AdaptImportPreviewOutputBases();
    }


    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task InsertTestDataAsync([FromBody] InsertTestDataInput input) =>
        App.GetService<IVariablePageService>().InsertTestDataAsync(input.TestVariableCount, input.TestDeviceCount, input.SlaveUrl, input.BusinessEnable, input.Restart);



    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task InsertTestDtuDataAsync([FromBody] InsertTestDtuDataInput input) =>
        App.GetService<IVariablePageService>().InsertTestDtuDataAsync(input.TestDeviceCount, input.SlaveUrl, input.Restart);


    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<bool> IsRedundantDeviceAsync(long id) =>
        App.GetService<IDevicePageService>().IsRedundantDeviceAsync(id);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<OperResult<LogData[]>> LastLogDataAsync([FromBody] LastLogDataInput input) =>
        App.GetService<ITextFileReadService>().LastLogDataAsync(input.File, input.LogLevel, input.LineCount);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<QueryData<ChannelRuntime>> OnChannelQueryAsync([FromBody] QueryPageOptions options) =>
        App.GetService<IChannelPageService>().OnChannelQueryAsync(options);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<QueryData<SelectedItem>> OnChannelSelectedItemQueryAsync([FromBody] VirtualizeQueryOption option) =>
        App.GetService<IChannelPageService>().OnChannelSelectedItemQueryAsync(option);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<QueryData<DeviceRuntime>> OnDeviceQueryAsync([FromBody] QueryPageOptions options) =>
        App.GetService<IDevicePageService>().OnDeviceQueryAsync(options);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<QueryData<SelectedItem>> OnDeviceSelectedItemQueryAsync([FromBody] OnDeviceSelectedItemQueryInput input) =>
        App.GetService<IDevicePageService>().OnDeviceSelectedItemQueryAsync(input.Option, input.IsCollect);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<QueryData<SelectedItem>> OnRedundantDevicesQueryAsync([FromBody] OnRedundantDevicesQueryInput input) =>
        App.GetService<IDevicePageService>().OnRedundantDevicesQueryAsync(input.Option, input.DeviceId, input.ChannelId);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<QueryData<VariableRuntime>> OnVariableQueryAsync([FromBody] QueryPageOptions options) =>
        App.GetService<IVariablePageService>().OnVariableQueryAsync(options);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<OperResult<object>> OnWriteVariableAsync(long id, string writeData) =>
        App.GetService<IVariablePageService>().OnWriteVariableAsync(id, writeData);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task PauseThreadAsync(long id) =>
        App.GetService<IDevicePageService>().PauseThreadAsync(id);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<QueryData<PluginInfo>> PluginPageAsync([FromBody] PluginQueryPageOptions options) =>
        App.GetService<IPluginPageService>().PluginPageAsync(options.Options, options.PluginType);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task RedundancyForcedSync() => App.GetService<IRedundancyHostedService>().RedundancyForcedSync();

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<LogLevel> RedundancyLogLevelAsync() => App.GetService<IRedundancyHostedService>().RedundancyLogLevelAsync();

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<string> RedundancyLogPathAsync() => App.GetService<IRedundancyHostedService>().RedundancyLogPathAsync();

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task ReloadPluginAsync() => App.GetService<IPluginPageService>().ReloadPluginAsync();

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task RestartChannelAsync(long channelId) =>
        App.GetService<IChannelPageService>().RestartChannelAsync(channelId);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task RestartChannelsAsync() =>
        App.GetService<IChannelPageService>().RestartChannelsAsync();

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task RestartDeviceAsync(long id, bool deleteCache) =>
        App.GetService<IDevicePageService>().RestartDeviceAsync(id, deleteCache);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task RestartServerAsync() => App.GetService<IRestartService>().RestartServerAsync();

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<QueryData<RpcLog>> RpcLogPageAsync([FromBody] QueryPageOptions option) =>
        App.GetService<IRpcLogService>().RpcLogPageAsync(option);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<List<RpcLogDayStatisticsOutput>> RpcLogStatisticsByDayAsync(int day) =>
        App.GetService<IRpcLogService>().RpcLogStatisticsByDayAsync(day);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<TouchSocket.Core.LogLevel> RulesLogLevelAsync(long rulesId) =>
        App.GetService<IRulesEngineHostedService>().RulesLogLevelAsync(rulesId);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<string> RulesLogPathAsync(long rulesId) =>
        App.GetService<IRulesEngineHostedService>().RulesLogPathAsync(rulesId);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<QueryData<Rules>> RulesPageAsync([FromBody] KVQueryPageOptions option) =>
        App.GetService<IRulesPageService>().RulesPageAsync(option.Options, option.FilterKeyValueAction);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<bool> SaveChannelAsync([FromBody] SaveChannelInput input) =>
        App.GetService<IChannelPageService>().SaveChannelAsync(input.Input, input.Type, input.Restart);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<bool> SaveDeviceAsync([FromBody] SaveDeviceInput input) =>
        App.GetService<IDevicePageService>().SaveDeviceAsync(input.Input, input.Type, input.Restart);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task SavePluginByPathAsync([FromBody] PluginAddPathInput plugin) =>
        App.GetService<IPluginPageService>().SavePluginByPathAsync(plugin);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<bool> SaveRulesAsync([FromBody] Rules input, ItemChangedType type) =>
        App.GetService<IRulesPageService>().SaveRulesAsync(input, type);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<bool> SaveVariableAsync([FromBody] SaveVariableInput input) =>
        App.GetService<IVariablePageService>().SaveVariableAsync(input.Input, input.Type, input.Restart);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task SetChannelLogLevelAsync([FromBody] SetChannelLogLevelInput input) =>
        App.GetService<IChannelPageService>().SetChannelLogLevelAsync(input.Id, input.LogLevel);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task SetDeviceLogLevelAsync([FromBody] SetDeviceLogLevelInput input) =>
        App.GetService<IDevicePageService>().SetDeviceLogLevelAsync(input.Id, input.LogLevel);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task SetRedundancyLogLevelAsync(LogLevel logLevel) =>
        App.GetService<IRedundancyHostedService>().SetRedundancyLogLevelAsync(logLevel);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task SetRulesLogLevelAsync([FromBody] SetRulesLogLevelInput input) =>
        App.GetService<IRulesEngineHostedService>().SetRulesLogLevelAsync(input.RulesId, input.LogLevel);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<bool> StartBusinessChannelEnableAsync() => App.GetService<IChannelEnableService>().StartBusinessChannelEnableAsync();

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<bool> StartCollectChannelEnableAsync() => App.GetService<IChannelEnableService>().StartCollectChannelEnableAsync();

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task StartRedundancyTaskAsync() => App.GetService<IRedundancyHostedService>().StartRedundancyTaskAsync();

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task StopRedundancyTaskAsync() => App.GetService<IRedundancyHostedService>().StopRedundancyTaskAsync();

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<AuthorizeInfo> TryAuthorizeAsync(string password) => App.GetService<IAuthenticationService>().TryAuthorizeAsync(password);

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<AuthorizeInfo> TryGetAuthorizeInfoAsync() => App.GetService<IAuthenticationService>().TryGetAuthorizeInfoAsync();

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task UnAuthorizeAsync() => App.GetService<IAuthenticationService>().UnAuthorizeAsync();

    [HttpPost]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<string> UUIDAsync() => App.GetService<IAuthenticationService>().UUIDAsync();


}
// 定义请求 DTO
public class BatchEditChannelRequest
{
    public List<Channel> Models { get; set; }
    public Channel OldModel { get; set; }
    public Channel Model { get; set; }
    public bool Restart { get; set; }
}

public class BatchEditDeviceRequest
{
    public List<Device> Models { get; set; }
    public Device OldModel { get; set; }
    public Device Model { get; set; }
    public bool Restart { get; set; }
}

public class BatchEditVariableRequest
{
    public List<Variable> Models { get; set; }
    public Variable OldModel { get; set; }
    public Variable Model { get; set; }
    public bool Restart { get; set; }
}

public class BatchSaveVariableRequest
{
    public List<Variable> Input { get; set; }
    public ItemChangedType Type { get; set; }
    public bool Restart { get; set; }
}
public class CopyChannelRequest
{
    public int CopyCount { get; set; }
    public string CopyChannelNamePrefix { get; set; }
    public int CopyChannelNameSuffixNumber { get; set; }
    public string CopyDeviceNamePrefix { get; set; }
    public int CopyDeviceNameSuffixNumber { get; set; }
    public long ChannelId { get; set; }
    public bool Restart { get; set; }
}

public class CopyDeviceRequest
{
    public int CopyCount { get; set; }
    public string CopyDeviceNamePrefix { get; set; }
    public int CopyDeviceNameSuffixNumber { get; set; }
    public long DeviceId { get; set; }
    public bool AutoRestartThread { get; set; }
}

public class CopyVariableRequest
{
    public List<Variable> Model { get; set; }
    public int CopyCount { get; set; }
    public string CopyVariableNamePrefix { get; set; }
    public int CopyVariableNameSuffixNumber { get; set; }
    public bool Restart { get; set; }
}
public class DeleteRequest
{
    public List<long> Ids { get; set; }
    public bool Restart { get; set; }
}
public class GetListRequest<TOptions>
{
    public TOptions Options { get; set; }
    public int Max { get; set; }
}


public class ImportChannelInput
{
    public List<Channel> UpData { get; set; }
    public List<Channel> InsertData { get; set; }
    public bool Restart { get; set; }
}

public class ImportFileInput
{
    public bool Restart { get; set; }
}


public class ImportUSheetInput
{
    public USheetDatas Input { get; set; }
    public bool Restart { get; set; }
}

public class InsertTestDataInput
{
    public int TestVariableCount { get; set; }
    public int TestDeviceCount { get; set; }
    public string SlaveUrl { get; set; }
    public bool BusinessEnable { get; set; }
    public bool Restart { get; set; }
}
public class InsertTestDtuDataInput
{
    public int TestDeviceCount { get; set; }
    public string SlaveUrl { get; set; }
    public bool Restart { get; set; }
}
public class LastLogDataInput
{
    public string File { get; set; }
    public TouchSocket.Core.LogLevel LogLevel { get; set; }
    public int LineCount { get; set; } = 200;
}

public class OnDeviceSelectedItemQueryInput
{
    public VirtualizeQueryOption Option { get; set; }
    public bool IsCollect { get; set; }
}

public class OnRedundantDevicesQueryInput
{
    public VirtualizeQueryOption Option { get; set; }
    public long DeviceId { get; set; }
    public long ChannelId { get; set; }
}

public class SaveChannelInput
{
    public Channel Input { get; set; }
    public ItemChangedType Type { get; set; }
    public bool Restart { get; set; }
}

public class SaveDeviceInput
{
    public Device Input { get; set; }
    public ItemChangedType Type { get; set; }
    public bool Restart { get; set; }
}

public class SaveVariableInput
{
    public Variable Input { get; set; }
    public ItemChangedType Type { get; set; }
    public bool Restart { get; set; }
}

public class SetChannelLogLevelInput
{
    public long Id { get; set; }
    public LogLevel LogLevel { get; set; }
}

public class SetDeviceLogLevelInput
{
    public long Id { get; set; }
    public LogLevel LogLevel { get; set; }
}

public class SetRulesLogLevelInput
{
    public long RulesId { get; set; }
    public TouchSocket.Core.LogLevel LogLevel { get; set; }
}


public class ImportRequest
{
    public IFormFile File { get; set; }
    public bool Restart { get; set; }
}


public class PluginQueryPageOptions
{
    public QueryPageOptions Options { get; set; }
    public PluginTypeEnum PluginType { get; set; }
}


public class KVQueryPageOptions
{
    public QueryPageOptions Options { get; set; }
    public FilterKeyValueAction FilterKeyValueAction { get; set; }
}