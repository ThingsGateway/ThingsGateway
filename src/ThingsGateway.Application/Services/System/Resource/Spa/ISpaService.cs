namespace ThingsGateway.Application
{
    /// <summary>
    /// 单页服务
    /// </summary>
    public interface ISpaService : ITransient
    {
        /// <summary>
        /// 添加单页
        /// </summary>
        /// <param name="input">添加参数</param>
        /// <returns></returns>
        Task Add(SpaAddInput input);

        /// <summary>
        /// 删除单页
        /// </summary>
        /// <param name="input">删除参数</param>
        /// <returns></returns>
        Task Delete(List<BaseIdInput> input);

        /// <summary>
        /// 编辑单页
        /// </summary>
        /// <param name="input">编辑参数</param>
        /// <returns></returns>
        Task Edit(SpaEditInput input);

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<SqlSugarPagedList<SysResource>> Page(SpaPageInput input);
    }
}