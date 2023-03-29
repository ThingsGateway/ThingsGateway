using Microsoft.AspNetCore.Components.Forms;

using System.IO;

using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation
{
    /// <summary>
    /// 采集设备服务
    /// </summary>
    public interface ICollectDeviceService : ITransient
    {
        /// <summary>
        /// 添加设备
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task Add(CollectDevice input);
        /// <summary>
        /// 复制设备
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task CopyDev(IEnumerable<CollectDevice> input);
        /// <summary>
        /// 复制设备与变量
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task CopyDevAndVar(IEnumerable<CollectDevice> input);
        /// <summary>
        /// 上传设备
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task Delete(List<BaseIdInput> input);
        /// <summary>
        /// 编辑设备
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task Edit(CollectDeviceEditInput input);
        /// <summary>
        /// 导出Excel
        /// </summary>
        /// <returns></returns>
        Task<MemoryStream> ExportFile();
        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <returns></returns>
        List<CollectDevice> GetCacheList();
        /// <summary>
        /// 获取设备运行状态DTO
        /// </summary>
        /// <param name="devId"></param>
        /// <returns></returns>
        Task<List<CollectDeviceRunTime>> GetCollectDeviceRuntime(long devId = 0);
        /// <summary>
        /// 根据ID获取设备
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        CollectDevice GetDeviceById(long Id);
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
        /// 获取设备组/名称树
        /// </summary>
        /// <returns></returns>
        List<DeviceTree> GetTree();
        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task Import(Dictionary<string, ImportPreviewOutputBase> input);
        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<SqlSugarPagedList<CollectDevice>> Page(CollectDevicePageInput input);
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
    /// <summary>
    /// 设备组/名称树
    /// </summary>
    public class DeviceTree
    {
        /// <summary>
        /// 节点名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 子节点
        /// </summary>
        public List<DeviceTree> Childrens { get; set; } = new();
    }
}