#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/dotnetchina/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using Microsoft.AspNetCore.Components.Forms;

using System.IO;

using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 上传设备服务
/// </summary>
public interface IUploadDeviceService : ITransient
{
    /// <summary>
    /// Sql连接对象
    /// </summary>
    public ISqlSugarClient Context { get; set; }

    /// <summary>
    /// 添加上传设备
    /// </summary>
    Task AddAsync(UploadDevice input);
    /// <summary>
    /// 复制设备
    /// </summary>
    Task CopyDevAsync(IEnumerable<UploadDevice> input);
    /// <summary>
    /// 删除设备
    /// </summary>
    Task DeleteAsync(List<BaseIdInput> input);
    /// <summary>
    /// 编辑设备
    /// </summary>
    Task EditAsync(UploadDeviceEditInput input);
    /// <summary>
    /// 导出
    /// </summary>
    Task<MemoryStream> ExportFileAsync(List<UploadDevice> devDatas = null);
    /// <summary>
    /// 导出
    /// </summary>
    Task<MemoryStream> ExportFileAsync(UploadDevicePageInput input);
    /// <summary>
    /// 获取缓存
    /// </summary>
    List<UploadDevice> GetCacheList();
    /// <summary>
    /// 根据ID获取设备
    /// </summary>
    UploadDevice GetDeviceById(long Id);
    /// <summary>
    /// 根据名称获取ID
    /// </summary>
    long? GetIdByName(string name);
    /// <summary>
    /// 根据ID获取名称
    /// </summary>
    string GetNameById(long id);
    /// <summary>
    /// 获取上传设备运行状态
    /// </summary>
    List<UploadDeviceRunTime> GetUploadDeviceRuntime(long devId = 0);
    /// <summary>
    /// 导入
    /// </summary>
    Task ImportAsync(Dictionary<string, ImportPreviewOutputBase> input);
    /// <summary>
    /// 分页
    /// </summary>
    Task<SqlSugarPagedList<UploadDevice>> PageAsync(UploadDevicePageInput input);
    /// <summary>
    /// 导入验证
    /// </summary>
    Task<Dictionary<string, ImportPreviewOutputBase>> PreviewAsync(IBrowserFile file);
}