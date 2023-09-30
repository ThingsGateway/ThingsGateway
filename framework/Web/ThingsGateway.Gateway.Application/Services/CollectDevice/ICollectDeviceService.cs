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

/// <summary>
/// 采集设备服务
/// </summary>
public interface ICollectDeviceService : ITransient
{
    /// <summary>
    /// 添加设备
    /// </summary>
    Task AddAsync(CollectDevice input);
    /// <summary>
    /// 复制设备
    /// </summary>
    Task CopyDevAsync(IEnumerable<CollectDevice> input);
    /// <summary>
    /// 复制设备与变量
    /// </summary>
    Task CopyDevAndVarAsync(IEnumerable<CollectDevice> input);
    /// <summary>
    /// 删除设备
    /// </summary>
    Task DeleteAsync(params long[] input);
    /// <summary>
    /// 编辑设备
    /// </summary>
    Task EditAsync(CollectDeviceEditInput input);
    /// <summary>
    /// 导出Excel
    /// </summary>
    Task<MemoryStream> ExportFileAsync(List<CollectDevice> devDatas = null);
    /// <summary>
    /// 导出Excel
    /// </summary>
    Task<MemoryStream> ExportFileAsync(CollectDeviceInput input);
    /// <summary>
    /// 获取缓存
    /// </summary>
    List<CollectDevice> GetCacheList(bool isMapster = true);
    /// <summary>
    /// 获取设备运行状态
    /// </summary>
    Task<List<CollectDeviceRunTime>> GetCollectDeviceRuntimeAsync(long devId = 0);
    /// <summary>
    /// 根据ID获取设备
    /// </summary>
    CollectDevice GetDeviceById(long Id);
    /// <summary>
    /// 根据名称获取ID
    /// </summary>
    long? GetIdByName(string name);
    /// <summary>
    /// 根据ID获取名称
    /// </summary>
    string GetNameById(long id);
    /// <summary>
    /// 获取设备组或名称的树节点
    /// </summary>
    List<DeviceTree> GetTree();
    /// <summary>
    /// 导入
    /// </summary>
    Task ImportAsync(Dictionary<string, ImportPreviewOutputBase> input);
    /// <summary>
    /// 分页查询
    /// </summary>
    Task<SqlSugarPagedList<CollectDevice>> PageAsync(CollectDevicePageInput input);
    /// <summary>
    /// 导入验证
    /// </summary>
    Task<Dictionary<string, ImportPreviewOutputBase>> PreviewAsync(IBrowserFile file);
}
