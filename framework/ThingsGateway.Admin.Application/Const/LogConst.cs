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
/// 日志常量
/// </summary>
public class LogConst
{
    #region 日志表

    /// <summary>
    /// 登录
    /// </summary>
    public const string LOG_LOGIN = "LOGIN";

    /// <summary>
    /// 登出
    /// </summary>
    public const string LOG_LOGOUT = "LOGOUT";


    /// <summary>
    /// 第三方登录
    /// </summary>
    public const string LOG_OPENAPILOGIN = "OPENAPILOGIN";

    /// <summary>
    /// 第三方登出
    /// </summary>
    public const string LOG_OPENAPILOGOUT = "OPENAPILOGOUT";

    /// <summary>
    /// 第三方操作来源
    /// </summary>
    public const string LOG_OPENAPIOPERATE = "OPENAPIOPERATE";

    /// <summary>
    /// 操作分类
    /// </summary>
    public const string LOG_OPERATE = "OPERATE";

    /// <summary>
    /// 内部操作来源
    /// </summary>
    public const string LOG_REQMETHOD = "BLAZORSERVER";


    /// <summary>
    /// 操作成功
    /// </summary>
    public const string LOG_SUCCESS = "SUCCESS";


    /// <summary>
    /// 操作失败
    /// </summary>
    public const string LOG_FAIL = "FAIL";
    #endregion 日志表
}