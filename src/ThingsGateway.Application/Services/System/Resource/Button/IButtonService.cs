namespace ThingsGateway.Application
{
    /// <summary>
    /// 权限按钮服务
    /// </summary>
    public interface IButtonService : ITransient
    {
        /// <summary>
        /// 添加按钮
        /// </summary>
        /// <param name="input">添加参数</param>
        /// <returns></returns>
        Task Add(ButtonAddInput input);

        /// <summary>
        /// 删除按钮
        /// </summary>
        /// <param name="input">删除参数</param>
        /// <returns></returns>
        Task Delete(List<BaseIdInput> input);

        /// <summary>
        /// 编辑按钮
        /// </summary>
        /// <param name="input">编辑参数</param>
        /// <returns></returns>
        Task Edit(ButtonEditInput input);

        /// <summary>
        /// 按钮分页查询
        /// </summary>
        /// <param name="input">查询条件</param>
        /// <returns>按钮分页列表</returns>
        Task<SqlSugarPagedList<SysResource>> Page(ButtonPageInput input);
    }
}