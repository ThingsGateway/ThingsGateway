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