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

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 会话管理服务
/// </summary>
public interface ISessionService : ISugarService, ITransient
{
    /// <summary>
    /// 会话统计
    /// </summary>
    /// <returns>统计结果</returns>
    SessionAnalysisOutput Analysis();

    /// <summary>
    /// 强退会话
    /// </summary>
    /// <param name="input">用户ID</param>
    Task ExitSessionAsync(BaseIdInput input);

    /// <summary>
    /// 强退verificat
    /// </summary>
    /// <param name="input">verificat列表</param>
    Task ExitVerificatAsync(ExitVerificatInput input);

    /// <summary>
    /// 会话分页查询
    /// </summary>
    /// <param name="input">查询参数</param>
    /// <returns>会话列表</returns>
    Task<SqlSugarPagedList<SessionOutput>> PageAsync(SessionPageInput input);
}