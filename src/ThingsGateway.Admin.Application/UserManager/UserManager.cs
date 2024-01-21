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
/// 当前登录用户信息
/// </summary>
public class UserManager
{
    /// <summary>
    /// 是否超级管理员
    /// </summary>
    public static bool SuperAdmin => (App.User?.FindFirst(ClaimConst.SuperAdmin)?.Value).ToBoolean();

    /// <summary>
    /// 当前用户账号
    /// </summary>
    public static string UserAccount => App.User?.FindFirst(ClaimConst.Account)?.Value;

    /// <summary>
    /// 当前用户Id
    /// </summary>
    public static long UserId => (App.User?.FindFirst(ClaimConst.UserId)?.Value).ToLong();

    /// <summary>
    /// 当前VerificatId
    /// </summary>
    public static long VerificatId => (App.User?.FindFirst(ClaimConst.VerificatId)?.Value).ToLong();
}