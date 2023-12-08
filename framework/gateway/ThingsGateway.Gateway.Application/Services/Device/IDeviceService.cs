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