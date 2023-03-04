namespace ThingsGateway.Application
{
    /// <summary>
    /// 系统配置服务
    /// </summary>
    public interface IConfigService : ITransient
    {
        /// <summary>
        /// 批量编辑
        /// </summary>
        /// <param name="devConfigs">配置列表</param>
        /// <returns></returns>
        Task EditBatch(List<DevConfig> devConfigs);
        /// <summary>
        /// 新增配置
        /// </summary>
        /// <param name="input">新增参数</param>
        /// <returns></returns>
        Task Add(ConfigAddInput input);

        /// <summary>
        /// 删除配置
        /// </summary>
        /// <param name="input">删除</param>
        /// <returns></returns>
        Task Delete(params ConfigDeleteInput[] input);

        /// <summary>
        /// 修改配置
        /// </summary>
        /// <param name="input">修改参数</param>
        /// <returns></returns>
        Task Edit(ConfigEditInput input);

        /// <summary>
        /// 根据分类和配置键获配置
        /// </summary>
        /// <param name="category">分类</param>
        /// <param name="configKey">配置键</param>
        /// <returns>配置信息</returns>
        Task<DevConfig> GetByConfigKey(string category, string configKey);

        /// <summary>
        /// 根据分类获取配置列表
        /// </summary>
        /// <param name="category">分类名称</param>
        /// <returns>配置列表</returns>
        Task<List<DevConfig>> GetListByCategory(string category);

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="input">查询参数</param>
        /// <returns>其他配置列表</returns>
        Task<SqlSugarPagedList<DevConfig>> Page(ConfigPageInput input);
    }
}