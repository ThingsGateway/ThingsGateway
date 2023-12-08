#region copyright

//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

#endregion

using Furion.DependencyInjection;

using Microsoft.AspNetCore.Components.Forms;

namespace ThingsGateway.Gateway.Application;

public interface IVariableService : ITransient
{
    Task AddAsync(DeviceVariable input);

    Task AddBatchAsync(List<DeviceVariable> input);

    Task ClearDeviceVariableAsync();

    Task DeleteAsync(params long[] input);

    void DeleteVariableFromCache();

    Task EditAsync(DeviceVariable input);

    Task<MemoryStream> ExportFileAsync(List<DeviceVariable> deviceVariables = null, string deviceName = null);

    Task<MemoryStream> ExportFileAsync(VariableInput input);

    Task<List<DeviceVariableRunTime>> GetDeviceVariableRuntimeAsync(long devId = 0);

    Task<List<DeviceVariableRunTime>> GetMemoryVariableRuntimeAsync();

    Task ImportAsync(Dictionary<string, ImportPreviewOutputBase> input);

    Task<SqlSugarPagedList<DeviceVariable>> PageAsync(VariablePageInput input);

    Task<Dictionary<string, ImportPreviewOutputBase>> PreviewAsync(IBrowserFile file);

    Task<Dictionary<string, ImportPreviewOutputBase>> PreviewAsync(MemoryStream stream, List<CollectDevice> memCollectDevices = null, List<Device> memUploadDevices = null);
}