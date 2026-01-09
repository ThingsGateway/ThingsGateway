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

namespace ThingsGateway.Gateway.Application;


public interface IMemoryVariableModelPageService
{
    Task<bool> BatchEditMemoryVariableAsync(List<MemoryVariable> models, MemoryVariable oldModel, MemoryVariable model, bool restart);
    Task<bool> DeleteMemoryVariableAsync(List<long> ids, bool restart);
    Task<bool> ClearMemoryVariableAsync(bool restart);

    Task<bool> BatchSaveMemoryVariableAsync(List<MemoryVariable> input, ItemChangedType type, bool restart);

    Task<bool> SaveMemoryVariableAsync(MemoryVariable input, ItemChangedType type, bool restart);
    Task CopyMemoryVariableAsync(List<MemoryVariable> Model, int CopyCount, string CopyMemoryVariableNamePrefix, int CopyMemoryVariableNameSuffixNumber, bool AutoRestartThread);
    Task<List<MemoryVariable>> GetMemoryVariableListAsync(QueryPageOptions option, int v);
    Task<USheetDatas> ExportMemoryVariableAsync(List<MemoryVariable> models, string? sortName, SortOrder sortOrder);
    Task<Dictionary<string, ImportPreviewOutputBase>> ImportMemoryVariableUSheetDatasAsync(USheetDatas data, bool restart);

    Task<string> ExportMemoryVariableFileAsync(GatewayExportFilter exportFilter);

    Task<OperResult<object>> OnWriteMemoryVariableAsync(string name, string writeData);

    Task<Dictionary<string, ImportPreviewOutputBase>> ImportMemoryVariableAsync(IBrowserFile a, bool restart);
    Task<Dictionary<string, ImportPreviewOutputBase>> ImportMemoryVariableFileAsync(string filePath, bool restart);
    Task<string> ExportMemoryVariableDataFileAsync(List<MemoryVariable> data, string devName);


}