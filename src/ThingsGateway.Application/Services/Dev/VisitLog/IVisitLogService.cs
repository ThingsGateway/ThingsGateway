namespace ThingsGateway.Application
{
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
        Task Delete(params string[] category);

        /// <summary>
        /// 访问日志分页查询
        /// </summary>
        /// <param name="input">查询参数</param>
        /// <returns>日志列表</returns>
        Task<SqlSugarPagedList<DevLogVisit>> Page(VisitLogPageInput input);
    }
}