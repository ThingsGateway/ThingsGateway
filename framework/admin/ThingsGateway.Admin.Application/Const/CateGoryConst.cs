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

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 其他分类常量
/// </summary>
public static class CateGoryConst
{
    /// <summary>
    /// ThingsGateway.Admin
    /// </summary>
    public const string ThingsGatewayAdmin = "ThingsGateway.Admin";

    /// <summary>
    /// ThingsGateway.OpenApi
    /// </summary>
    public const string ThingsGatewayOpenApi = "ThingsGateway.OpenApi";

    #region 关系表

    /// <summary>
    /// 用户主页
    /// </summary>
    public const string Relation_SYS_USER_DEFAULTRAZOR = "Relation_SYS_USER_DEFAULTRAZOR";

    /// <summary>
    /// 用户工作台数据
    /// </summary>
    public const string Relation_SYS_USER_WORKBENCH_DATA = "SYS_USER_WORKBENCH_DATA";

    /// <summary>
    /// 角色有哪些权限
    /// </summary>
    public const string Relation_SYS_ROLE_HAS_PERMISSION = "SYS_ROLE_HAS_PERMISSION";

    /// <summary>
    /// 角色有哪些资源
    /// </summary>
    public const string Relation_SYS_ROLE_HAS_RESOURCE = "SYS_ROLE_HAS_RESOURCE";

    /// <summary>
    /// 用户有哪些角色
    /// </summary>
    public const string Relation_SYS_USER_HAS_ROLE = "SYS_USER_HAS_ROLE";

    #endregion 关系表
}