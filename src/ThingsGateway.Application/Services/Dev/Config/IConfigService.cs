#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

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