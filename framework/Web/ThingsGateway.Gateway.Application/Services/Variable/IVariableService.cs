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
/// 变量数据服务
/// </summary>
public interface IVariableService : ITransient
{
    /// <summary>
    /// 添加变量
    /// </summary>
    Task AddAsync(DeviceVariable input);
    /// <summary>
    /// 添加变量
    /// </summary>
    Task AddBatchAsync(List<DeviceVariable> input);
    /// <summary>
    /// 清空设备变量
    /// </summary>
    Task ClearDeviceVariableAsync();
    /// <summary>
    /// 清空中间变量
    /// </summary>
    /// <returns></returns>
    Task ClearMemoryVariableAsync();

    /// <summary>
    /// 删除变量
    /// </summary>
    Task DeleteAsync(params long[] input);
    /// <summary>
    /// 删除变量缓存
    /// </summary>
    void DeleteVariableFromCache();
    /// <summary>
    /// 编辑变量
    /// </summary>
    Task EditAsync(DeviceVariable input);

    /// <summary>
    /// 导出
    /// </summary>
    Task<MemoryStream> ExportFileAsync(List<DeviceVariable> collectDeviceVariables = null, string deviceName = null);
    /// <summary>
    /// 导出
    /// </summary>
    Task<MemoryStream> ExportFileAsync(MemoryVariableInput input);

    /// <summary>
    /// 获取变量运行状态
    /// </summary>
    Task<List<DeviceVariableRunTime>> GetDeviceVariableRuntimeAsync(long devId = 0);

    /// <summary>
    /// 获取中间变量运行态
    /// </summary>
    /// <returns></returns>
    Task<List<DeviceVariableRunTime>> GetMemoryVariableRuntimeAsync();

    /// <summary>
    /// 导入
    /// </summary>
    Task ImportAsync(Dictionary<string, ImportPreviewOutputBase> input);
    /// <summary>
    /// 导出
    /// </summary>
    /// <param name="devDatas"></param>
    /// <returns></returns>
    Task<MemoryStream> MemoryVariableExportFileAsync(List<MemoryVariable> devDatas = null);
    /// <summary>
    /// 导入
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    Task<Dictionary<string, ImportPreviewOutputBase>> MemoryVariablePreviewAsync(IBrowserFile file);

    /// <summary>
    /// 分页查询
    /// </summary>
    Task<SqlSugarPagedList<DeviceVariable>> PageAsync(VariablePageInput input);
    /// <summary>
    /// 导入验证
    /// </summary>
    Task<Dictionary<string, ImportPreviewOutputBase>> PreviewAsync(IBrowserFile file);

}