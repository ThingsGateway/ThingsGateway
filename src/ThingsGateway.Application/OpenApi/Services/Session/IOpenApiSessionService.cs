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

namespace ThingsGateway.Application
{
    /// <summary>
    /// 会话管理服务
    /// </summary>
    public interface IOpenApiSessionService : ITransient
    {
        /// <summary>
        /// 强退会话
        /// </summary>
        /// <param name="input">用户ID</param>
        Task ExitSession(BaseIdInput input);

        /// <summary>
        /// 强退cookie
        /// </summary>
        /// <param name="input">cookie列表</param>
        Task ExitVerificat(OpenApiExitVerificatInput input);

        /// <summary>
        /// B端会话分页查询
        /// </summary>
        /// <param name="input">查询参数</param>
        /// <returns>B端会话列表</returns>
        Task<SqlSugarPagedList<OpenApiSessionOutput>> Page(OpenApiSessionPageInput input);
    }
}