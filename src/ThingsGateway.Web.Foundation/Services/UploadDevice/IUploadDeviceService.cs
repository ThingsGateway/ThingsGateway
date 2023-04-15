using Microsoft.AspNetCore.Components.Forms;

using System.IO;

using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation
{
    /// <summary>
    /// 上传设备服务
    /// </summary>
    public interface IUploadDeviceService : ITransient
    {
        /// <summary>
        /// 添加上传设备
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task AddAsync(UploadDevice input);
        /// <summary>
        /// 复制设备
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task CopyDevAsync(IEnumerable<UploadDevice> input);
        /// <summary>
        /// 删除设备
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task DeleteAsync(List<BaseIdInput> input);
        /// <summary>
        /// 编辑设备
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task EditAsync(UploadDeviceEditInput input);
        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        Task<MemoryStream> ExportFileAsync();
        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <returns></returns>
        List<UploadDevice> GetCacheList();
        /// <summary>
        /// 根据ID获取设备
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        UploadDevice GetDeviceById(long Id);
        /// <summary>
        /// 根据名称获取ID
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        long? GetIdByName(string name);
        /// <summary>
        /// 根据ID获取名称
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        string GetNameById(long id);
        /// <summary>
        /// 获取上传设备运行状态DTO
        /// </summary>
        /// <param name="devId"></param>
        /// <returns></returns>
        List<UploadDeviceRunTime> GetUploadDeviceRuntime(long devId = 0);
        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task ImportAsync(Dictionary<string, ImportPreviewOutputBase> input);
        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<SqlSugarPagedList<UploadDevice>> PageAsync(UploadDevicePageInput input);
        /// <summary>
        /// 导入验证
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        Task<Dictionary<string, ImportPreviewOutputBase>> PreviewAsync(IBrowserFile file);
    }
}