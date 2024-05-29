//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

namespace ThingsGateway.Admin.Application;

public interface ISessionService
{
    /// <summary>
    /// 异步分页查询会话信息
    /// </summary>
    /// <param name="option">查询条件</param>
    /// <returns>查询结果</returns>
    Task<QueryData<SessionOutput>> PageAsync(QueryPageOptions option);

    /// <summary>
    /// 强制退出用户会话
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>异步操作结果</returns>
    Task ExitSession(long userId);

    /// <summary>
    /// 强制退出用户令牌
    /// </summary>
    /// <param name="input">参数</param>
    /// <returns>异步操作结果</returns>
    Task ExitVerificat(ExitVerificatInput input);
}
