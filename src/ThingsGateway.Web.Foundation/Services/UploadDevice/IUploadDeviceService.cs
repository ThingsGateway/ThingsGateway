using Microsoft.AspNetCore.Components.Forms;

using System.IO;

using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation
{
    public interface IUploadDeviceService : ITransient
    {
        Task Add(UploadDevice input);
        Task Delete(List<BaseIdInput> input);
        Task Edit(UploadDeviceEditInput input);
        Task<MemoryStream> ExportFile();
        List<UploadDevice> GetCacheListAsync();
        UploadDevice GetDeviceById(long Id);
        long? GetIdByName(string name);
        string GetNameById(long id);
        List<UploadDeviceRunTime> GetUploadDeviceRuntime(long devId = 0);
        Task Import(Dictionary<string, ImportPreviewOutputBase> input);
        Task<SqlSugarPagedList<UploadDevice>> Page(UploadDevicePageInput input);
        Task<Dictionary<string, ImportPreviewOutputBase>> Preview(IBrowserFile file);
        Task<MemoryStream> Template();
    }
}