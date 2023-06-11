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
    /// 资源服务
    /// </summary>
    public interface IResourceService : ITransient
    {
        /// <summary>
        /// 获取所有的菜单和模块以及单页面列表，并按分类和排序码排序,不会形成树列表
        /// </summary>
        /// <returns>所有的菜单和模块以及单页面列表</returns>
        Task<List<SysResource>> GetaMenuAndSpaList();
        /// <summary>
        /// 获取子资源
        /// </summary>
        /// <param name="sysResources"></param>
        /// <param name="resId"></param>
        /// <param name="isContainOneself"></param>
        /// <returns></returns>
        List<SysResource> GetChildListById(List<SysResource> sysResources, long resId, bool isContainOneself = true);

        /// <summary>
        /// 获取ID获取Code列表
        /// </summary>
        /// <param name="ids">id列表</param>
        /// <param name="category">分类</param>
        /// <returns>Code列表</returns>
        Task<List<string>> GetCodeByIds(List<long> ids, MenuCategoryEnum category);

        /// <summary>
        /// 资源分类列表,如果是空的则获取全部资源
        /// </summary>
        /// <param name="categorys">资源分类列表</param>
        /// <returns></returns>
        Task<List<SysResource>> GetListAsync(List<MenuCategoryEnum> categorys = null);

        /// <summary>
        /// 根据分类获取资源列表
        /// </summary>
        /// <param name="category">分类名称</param>
        /// <returns>资源列表</returns>
        Task<List<SysResource>> GetListByCategory(MenuCategoryEnum category);

        /// <summary>
        /// 刷新缓存
        /// </summary>
        /// <param name="category">分类名称</param>
        /// <returns></returns>
        Task RefreshCache(MenuCategoryEnum category);
        /// <summary>
        /// 资源列表
        /// </summary>
        /// <returns></returns>
        Task<List<RoleGrantResourceMenu>> ResourceTreeSelector();
    }
}