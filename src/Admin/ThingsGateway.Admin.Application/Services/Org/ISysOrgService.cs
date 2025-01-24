// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using SqlSugar;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 机构服务
/// </summary>
public interface ISysOrgService
{
    /// <summary>
    /// 复制组织
    /// </summary>
    /// <param name="input">机构复制参数</param>
    /// <returns></returns>
    Task CopyAsync(SysOrgCopyInput input);
    /// <summary>
    /// 保存机构
    /// </summary>
    /// <param name="input">机构</param>
    /// <param name="type">保存类型</param>
    Task<bool> SaveOrgAsync(SysOrg input, ItemChangedType type);

    /// <summary>
    /// 删除机构
    /// </summary>
    /// <param name="ids">id列表</param>
    Task<bool> DeleteOrgAsync(IEnumerable<long> ids);

    /// <summary>
    /// 报表查询
    /// </summary>
    /// <param name="option">查询条件</param>
    /// <param name="queryFunc">额外条件</param>
    Task<QueryData<SysOrg>> PageAsync(QueryPageOptions option, Func<ISugarQueryable<SysOrg>, ISugarQueryable<SysOrg>>? queryFunc = null);

    /// <summary>
    /// 获取全部机构
    /// </summary>
    /// <returns></returns>
    Task<List<SysOrg>> GetAllAsync(bool showDisabled = true);
    /// <summary>
    /// 获取机构及下级ID列表
    /// </summary>
    /// <param name="orgId"></param>
    /// <param name="isContainOneself"></param>
    /// <param name="sysOrgList">组织列表</param>
    /// <returns></returns>
    Task<HashSet<long>> GetOrgChildIdsAsync(long orgId, bool isContainOneself = true, List<SysOrg> sysOrgList = null);


    /// <summary>
    /// 根据组织ID获取租户ID
    /// </summary>
    /// <param name="orgId">组织id</param>
    /// <param name="sysOrgList">租户id</param>
    /// <returns></returns>
    Task<long?> GetTenantIdByOrgIdAsync(long orgId, List<SysOrg> sysOrgList = null);
    Task<List<SysOrg>> GetChildListByIdAsync(long orgId, bool isContainOneself = true, List<SysOrg> sysOrgList = null);

    /// <summary>
    /// 获取组织信息
    /// </summary>
    /// <param name="id">组织id</param>
    /// <returns>组织信息</returns>
    Task<SysOrg> GetSysOrgByIdAsync(long id);
    /// <summary>
    /// 获取租户列表
    /// </summary>
    /// <returns></returns>
    Task<List<SysOrg>> GetTenantListAsync();
    /// <summary>
    /// 获取机构选择器
    /// </summary>
    /// <returns></returns>
    Task<List<SysOrg>> SelectorAsync();
}
