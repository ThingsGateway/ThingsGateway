//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 分类常量
/// </summary>
public class CateGoryConst
{
    #region 系统配置

    /// <summary>
    /// 系统基础
    /// </summary>
    public const string Config_SYS_BASE = "SYS_BASE";

    /// <summary>
    /// 业务定义
    /// </summary>
    public const string Config_BIZ_DEFINE = "BIZ_DEFINE";

    /// <summary>
    /// 登录策略
    /// </summary>
    public const string LOGIN_POLICY = "LOGIN_POLICY";

    /// <summary>
    /// 密码策略
    /// </summary>
    public const string Config_PWD_POLICY = "PWD_POLICY";

    #endregion 系统配置

    #region 资源表

    /// <summary>
    /// 菜单
    /// </summary>
    public const string Resource_MENU = "MENU";

    /// <summary>
    /// 单页
    /// </summary>
    public const string Resource_SPA = "SPA";

    /// <summary>
    /// 按钮
    /// </summary>
    public const string Resource_BUTTON = "BUTTON";

    #endregion 资源表

    #region 关系表

    /// <summary>
    /// 用户有哪些角色
    /// </summary>
    public const string Relation_SYS_USER_HAS_ROLE = "SYS_USER_HAS_ROLE";

    /// <summary>
    /// 角色有哪些资源
    /// </summary>
    public const string Relation_SYS_ROLE_HAS_RESOURCE = "SYS_ROLE_HAS_RESOURCE";

    /// <summary>
    ///用户有哪些资源
    /// </summary>
    public const string Relation_SYS_USER_HAS_RESOURCE = "SYS_USER_HAS_RESOURCE";

    /// <summary>
    /// 角色有哪些权限
    /// </summary>
    public const string Relation_SYS_ROLE_HAS_PERMISSION = "SYS_ROLE_HAS_PERMISSION";

    /// <summary>
    /// 角色有哪些OPENAPI权限
    /// </summary>
    public const string Relation_SYS_ROLE_HAS_OPENAPIPERMISSION = "SYS_ROLE_HAS_OPENAPIPERMISSION";

    /// <summary>
    /// 用户有哪些权限
    /// </summary>
    public const string Relation_SYS_USER_HAS_PERMISSION = "SYS_USER_HAS_PERMISSION";

    /// <summary>
    /// 用户有哪些OPENAPI权限
    /// </summary>
    public const string Relation_SYS_USER_HAS_OPENAPIPERMISSION = "Relation_SYS_USER_HAS_OPENAPIPERMISSION";

    /// <summary>
    /// 用户工作台数据
    /// </summary>
    public const string Relation_SYS_USER_WORKBENCH_DATA = "SYS_USER_WORKBENCH_DATA";

    /// <summary>
    /// 用户主页数据
    /// </summary>
    public const string Relation_SYS_USER_DEFAULT_RAZOR = "Relation_SYS_USER_DEFAULT_RAZOR";

    #endregion 关系表

    #region 日志表

    /// <summary>
    /// 登录
    /// </summary>
    public const string Log_LOGIN = "LOGIN";

    /// <summary>
    /// 登出
    /// </summary>
    public const string Log_LOGOUT = "LOGOUT";

    /// <summary>
    /// 操作
    /// </summary>
    public const string Log_OPERATE = "OPERATE";

    /// <summary>
    /// 异常
    /// </summary>
    public const string Log_EXCEPTION = "EXCEPTION";

    #endregion 日志表

    #region 角色表

    /// <summary>
    /// 全局
    /// </summary>
    public const string Role_GLOBAL = "GLOBAL";

    /// <summary>
    /// Api
    /// </summary>
    public const string Role_API = "API";

    #endregion 角色表

    #region Api分组

    public const string ThingsGatewayAdmin = "ThingsGateway.Admin";
    public const string ThingsGatewayApi = "ThingsGateway.OpenApi";

    #endregion Api分组
}