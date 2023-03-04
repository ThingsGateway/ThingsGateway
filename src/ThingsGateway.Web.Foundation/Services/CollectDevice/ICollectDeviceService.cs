﻿using Microsoft.AspNetCore.Components.Forms;

using System.IO;

using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation
{
    public interface ICollectDeviceService : ITransient
    {
        Task Add(CollectDevice input);
        Task Delete(List<BaseIdInput> input);
        Task Edit(CollectDeviceEditInput input);
        Task<MemoryStream> ExportFile();
        List<CollectDevice> GetCacheListAsync();
        Task<List<CollectDeviceRunTime>> GetCollectDeviceRuntime(long devId = 0);
        CollectDevice GetDeviceById(long Id);
        long? GetIdByName(string name);
        string GetNameById(long id);
        Task Import(Dictionary<string, ImportPreviewOutputBase> input);
        Task<SqlSugarPagedList<CollectDevice>> Page(CollectDevicePageInput input);
        Task<Dictionary<string, ImportPreviewOutputBase>> Preview(IBrowserFile file);
        Task<MemoryStream> Template();

    }
}