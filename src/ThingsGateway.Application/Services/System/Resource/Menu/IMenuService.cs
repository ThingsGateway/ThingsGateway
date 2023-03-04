namespace ThingsGateway.Application
{
    /// <summary>
    /// 菜单服务
    /// </summary>
    public interface IMenuService : ITransient
    {
        /// <summary>
        /// 添加菜单
        /// </summary>
        /// <param name="input">添加参数</param>
        /// <returns></returns>
        Task Add(MenuAddInput input);



        /// <summary>
        /// 删除菜单
        /// </summary>
        /// <param name="input">删除菜单参数</param>
        /// <returns></returns>
        Task Delete(List<BaseIdInput> input);

        /// <summary>
        /// 详情
        /// </summary>
        /// <param name="input">id</param>
        /// <returns>详细信息</returns>
        Task<SysResource> Detail(BaseIdInput input);

        /// <summary>
        /// 编辑菜单
        /// </summary>
        /// <param name="input">菜单编辑参数</param>
        /// <returns></returns>
        Task Edit(MenuEditInput input);

        /// <summary>
        /// 根据模块获取菜单树，为空则为全部模块
        /// </summary>
        /// <param name="input">菜单树查询参数</param>
        /// <returns>菜单树列表</returns>
        Task<List<SysResource>> Tree(MenuPageInput input);
    }
}