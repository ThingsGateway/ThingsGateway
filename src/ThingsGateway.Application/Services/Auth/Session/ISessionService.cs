namespace ThingsGateway.Application
{
    /// <summary>
    /// 会话管理服务
    /// </summary>
    public interface ISessionService : ITransient
    {
        /// <summary>
        /// 强退会话
        /// </summary>
        /// <param name="input">用户ID</param>
        Task ExitSession(BaseIdInput input);

        /// <summary>
        /// 强退verificat
        /// </summary>
        /// <param name="input">verificat列表</param>
        Task ExitVerificat(ExitVerificatInput input);

        /// <summary>
        /// 会话分页查询
        /// </summary>
        /// <param name="input">查询参数</param>
        /// <returns>会话列表</returns>
        Task<SqlSugarPagedList<SessionOutput>> Page(SessionPageInput input);
    }
}