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

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 访问日志服务
/// </summary>

public interface IVisitLogService : ITransient
{
    /// <summary>
    /// 根据分类删除
    /// </summary>
    /// <param name="category">分类名称</param>
    /// <returns></returns>
    Task DeleteAsync(params string[] category);
    /// <summary>
    /// 导出访问日志
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<MemoryStream> ExportFileAsync(VisitLogInput input);
    /// <summary>
    /// 导出访问日志
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<MemoryStream> ExportFileAsync(List<SysVisitLog> input = null);

    /// <summary>
    /// 访问日志分页查询
    /// </summary>
    /// <param name="input">查询参数</param>
    /// <returns>日志列表</returns>
    Task<ISqlSugarPagedList<SysVisitLog>> PageAsync(VisitLogPageInput input);
}