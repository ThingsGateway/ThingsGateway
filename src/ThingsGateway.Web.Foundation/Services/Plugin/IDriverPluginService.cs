using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation
{
    /// <summary>
    /// 插件服务
    /// </summary>
    public interface IDriverPluginService : ITransient
    {
        /// <summary>
        /// 添加/更新插件
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task Add(DriverPluginAddInput input);
        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <returns></returns>
        List<DriverPlugin> GetCacheListAsync();
        /// <summary>
        /// 根据ID获取插件信息
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        DriverPlugin GetDriverPluginById(long Id);
        /// <summary>
        /// 根据分类获取插件树
        /// </summary>
        /// <param name="driverTypeEnum"></param>
        /// <returns></returns>
        List<DriverPluginCategory> GetDriverPluginChildrenList(DriverEnum driverTypeEnum);
        /// <summary>
        /// 根据ID获取名称
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        long? GetIdByName(string name);
        /// <summary>
        /// 根据名称获取ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        string GetNameById(long id);
        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<SqlSugarPagedList<DriverPlugin>> Page(DriverPluginPageInput input);
    }
}