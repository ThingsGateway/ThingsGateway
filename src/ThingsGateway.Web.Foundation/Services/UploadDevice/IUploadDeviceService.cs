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
        Task Add(UploadDevice input);
        /// <summary>
        /// 复制设备
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task CopyDev(IEnumerable<UploadDevice> input);
        /// <summary>
        /// 删除设备
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task Delete(List<BaseIdInput> input);
        /// <summary>
        /// 编辑设备
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task Edit(UploadDeviceEditInput input);
        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        Task<MemoryStream> ExportFile();
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
        Task Import(Dictionary<string, ImportPreviewOutputBase> input);
        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<SqlSugarPagedList<UploadDevice>> Page(UploadDevicePageInput input);
        /// <summary>
        /// 导入验证
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        Task<Dictionary<string, ImportPreviewOutputBase>> Preview(IBrowserFile file);
        /// <summary>
        /// 导出模板
        /// </summary>
        /// <returns></returns>
        Task<MemoryStream> Template();
    }
}