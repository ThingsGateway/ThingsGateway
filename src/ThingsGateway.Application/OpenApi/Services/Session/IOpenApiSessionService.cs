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