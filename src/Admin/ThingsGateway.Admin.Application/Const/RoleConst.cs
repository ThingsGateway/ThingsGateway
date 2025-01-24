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

/// <summary>
/// 角色常量
/// </summary>
[ThingsGateway.DependencyInjection.SuppressSniffer]
public class RoleConst
{
    /// <summary>
    /// api角色
    /// </summary>
    public const string ApiRole = "ApiRole";

    /// <summary>
    /// 业务管理员
    /// </summary>
    public const string BizAdmin = "BizAdmin";

    /// <summary>
    /// 超级管理员
    /// </summary>
    public const string SuperAdmin = "SuperAdmin";

    /// <summary>
    /// 超级管理员Id
    /// </summary>
    public const long SuperAdminId = 212725263002001;
    /// <summary>
    /// 默认租户Id
    /// </summary>
    public const long DefaultTenantId = 252885263003720;
    /// <summary>
    /// 默认岗位Id
    /// </summary>
    public const long DefaultPositionId = 212725263003001;
    /// <summary>
    /// 超级管理员Id
    /// </summary>
    public const long SuperAdminRoleId = 212725263001001;
}
