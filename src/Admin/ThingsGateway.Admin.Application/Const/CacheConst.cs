//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Admin.Application;

[ThingsGateway.DependencyInjection.SuppressSniffer]
public class CacheConst
{
    /// <summary>
    /// Token表缓存Key
    /// </summary>
    public const string Cache_HardwareInfo = $"{CacheConst.Cache_Prefix_Admin}Cache_HardwareInfo:";

    public const string Cache_Prefix_Admin = "ThingsGatewayAdmin:";

    /// <summary>
    /// 系统字典表缓存Key
    /// </summary>
    public const string Cache_SysDict = $"{CacheConst.Cache_Prefix_Admin}SysDict:";

    /// <summary>
    /// 关系表缓存Key
    /// </summary>
    public const string Cache_SysRelation = $"{CacheConst.Cache_Prefix_Admin}SysRelation:";

    /// <summary>
    /// 资源表缓存Key
    /// </summary>
    public const string Cache_SysResource = $"{CacheConst.Cache_Prefix_Admin}SysResource:";

    /// <summary>
    /// 角色表缓存Key
    /// </summary>
    public const string Cache_SysRole = $"{CacheConst.Cache_Prefix_Admin}SysRole:";

    /// <summary>
    /// 用户表缓存Key
    /// </summary>
    public const string Cache_SysUser = $"{CacheConst.Cache_Prefix_Admin}SysUser:";

    /// <summary>
    /// 用户账号关系缓存Key
    /// </summary>
    public const string Cache_SysUserAccount = $"{CacheConst.Cache_Prefix_Admin}SysUserAccount:";

    /// <summary>
    /// 职位表缓存Key
    /// </summary>
    public const string Cache_SysPosition = $"{CacheConst.Cache_Prefix_Admin}SysPosition:";

    /// <summary>
    /// 机构表缓存Key
    /// </summary>
    public const string Cache_SysOrg = $"{CacheConst.Cache_Prefix_Admin}SysOrg:";

    /// <summary>
    /// 公司表缓存Key
    /// </summary>
    public const string Cache_SysTenant = $"{CacheConst.Cache_Prefix_Admin}Tenant:";
    /// <summary>
    /// 公司表缓存Key
    /// </summary>
    public const string Cache_SysOrgTenant = $"{CacheConst.Cache_Prefix_Admin}OrgTenant:";

    /// <summary>
    /// Token表缓存Key
    /// </summary>
    public const string Cache_Token = $"{CacheConst.Cache_Prefix_Admin}Token:";

    #region 登录错误次数

    /// <summary>
    ///  登录错误次数缓存Key
    /// </summary>
    public const string Cache_LoginErrorCount = $"{CacheConst.Cache_Prefix_Admin}LoginErrorCount:";

    #endregion 登录错误次数
}
