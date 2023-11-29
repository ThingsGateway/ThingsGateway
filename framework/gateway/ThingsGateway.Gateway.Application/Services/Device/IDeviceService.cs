using Microsoft.AspNetCore.Components.Forms;

namespace ThingsGateway.Gateway.Application
{
    public interface IDeviceService<T> where T : Device, new()
    {
        Task AddAsync(T input);
        Task CopyDevAsync(IEnumerable<T> input);
        Task DeleteAsync(params long[] input);
        Task EditAsync(DeviceEditInput input);
        Task<MemoryStream> ExportFileAsync(List<T> devDatas = null);
        Task<MemoryStream> ExportFileAsync(DeviceInput input);
        List<T> GetCacheList(bool isMapster);
        T GetDeviceById(long Id);
        long? GetIdByName(string name);
        string GetNameById(long id);
        List<DeviceTree> GetTree();
        Task ImportAsync(Dictionary<string, ImportPreviewOutputBase> input);
        Task<SqlSugarPagedList<T>> PageAsync(DevicePageInput input);
        Task<Dictionary<string, ImportPreviewOutputBase>> PreviewAsync(IBrowserFile file);
        Task<Dictionary<string, ImportPreviewOutputBase>> PreviewAsync(MemoryStream stream);
        void RemoveCache();
    }
}