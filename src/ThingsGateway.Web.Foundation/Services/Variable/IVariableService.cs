using Microsoft.AspNetCore.Components.Forms;

using System.IO;

using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation
{
    public interface IVariableService : ITransient
    {
        ISqlSugarClient Context { get; set; }
        Task Add(CollectDeviceVariable input);
        Task Clear();
        Task Delete(List<BaseIdInput> input);
        void DeleteVariableFromCache(List<long> ids = null);
        void DeleteVariableFromCache(long userId);
        Task Edit(CollectDeviceVariable input);
        Task<MemoryStream> ExportFile();
        Task<MemoryStream> ExportFile(List<CollectDeviceVariable> collectDeviceVariables);
        Task<List<CollectVariableRunTime>> GetCollectDeviceVariableRuntime(long devId = 0);
        long GetIdByName(string name);
        string GetNameById(long Id);
        Task Import(Dictionary<string, ImportPreviewOutputBase> input);
        Task<SqlSugarPagedList<CollectDeviceVariable>> Page(VariablePageInput input);
        Task<Dictionary<string, ImportPreviewOutputBase>> Preview(IBrowserFile file);
        Task<MemoryStream> Template();
    }
}