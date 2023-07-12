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

using System.IO;

using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation;

/// <summary>
/// RPC日志服务
/// </summary>
public interface IRpcLogService : ITransient
{
    /// <summary>
    /// 删除
    /// </summary>
    /// <returns></returns>
    Task DeleteAsync();
    /// <summary>
    /// 分页查询
    /// </summary>
    Task<SqlSugarPagedList<RpcLog>> PageAsync(RpcLogPageInput input);
    /// <summary>
    /// 导出
    /// </summary>
    Task<MemoryStream> ExportFileAsync(List<RpcLog> input = null);
    /// <summary>
    /// 导出
    /// </summary>
    Task<MemoryStream> ExportFileAsync(RpcLogPageInput input);
}