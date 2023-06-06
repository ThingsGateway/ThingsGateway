#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
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
/// 变量数据服务
/// </summary>
public interface IVariableService : ITransient
{
    /// <summary>
    /// 数据库DB
    /// </summary>
    ISqlSugarClient Context { get; set; }
    /// <summary>
    /// 添加变量
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task AddAsync(CollectDeviceVariable input);
    /// <summary>
    /// 清空变量
    /// </summary>
    /// <returns></returns>
    Task ClearAsync();
    /// <summary>
    /// 删除变量
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task DeleteAsync(List<BaseIdInput> input);
    /// <summary>
    /// 删除变量缓存
    /// </summary>
    /// <param name="ids"></param>
    void DeleteVariableFromCache(List<long> ids = null);
    /// <summary>
    /// 删除变量缓存
    /// </summary>
    void DeleteVariableFromCache(long id);
    /// <summary>
    /// 编辑变量
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task EditAsync(CollectDeviceVariable input);
    /// <summary>
    /// 导出
    /// </summary>
    /// <returns></returns>
    Task<MemoryStream> ExportFileAsync();
    /// <summary>
    /// 导出
    /// </summary>
    /// <param name="collectDeviceVariables"></param>
    /// <returns></returns>
    Task<MemoryStream> ExportFileAsync(List<CollectDeviceVariable> collectDeviceVariables);
    /// <summary>
    /// 获取变量运行状态DTO
    /// </summary>
    /// <param name="devId"></param>
    /// <returns></returns>
    Task<List<CollectVariableRunTime>> GetCollectDeviceVariableRuntimeAsync(long devId = 0);
    /// <summary>
    /// 根据名称获取ID
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    long GetIdByName(string name);
    /// <summary>
    /// 根据ID获取名称
    /// </summary>
    /// <param name="Id"></param>
    /// <returns></returns>
    string GetNameById(long Id);
    /// <summary>
    /// 导入
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task ImportAsync(Dictionary<string, ImportPreviewOutputBase> input);
    /// <summary>
    /// 分页查询
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<SqlSugarPagedList<CollectDeviceVariable>> PageAsync(VariablePageInput input);
    /// <summary>
    /// 导入验证
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    Task<Dictionary<string, ImportPreviewOutputBase>> PreviewAsync(IBrowserFile file);

}