namespace ThingsGateway.Application
{
    /// <summary>
    /// 操作日志服务
    /// </summary>
    public interface IOperateLogService : ITransient
    {
        /// <summary>
        /// 根据分类删除操作日志
        /// </summary>
        /// <param name="category">分类名称</param>
        /// <returns></returns>
        Task Delete(params string[] category);

        /// <summary>
        /// 操作日志分页查询
        /// </summary>
        /// <param name="input">查询参数</param>
        /// <returns>分页列表</returns>
        Task<SqlSugarPagedList<DevLogOperate>> Page(OperateLogPageInput input);
    }
}