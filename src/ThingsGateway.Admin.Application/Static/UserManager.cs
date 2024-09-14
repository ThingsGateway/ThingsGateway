//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;

using ThingsGateway.NewLife.X;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 当前登录用户信息
/// </summary>
public static class UserManager
{
    private static readonly IAppService _appService;
    static UserManager()
    {
        _appService = NetCoreApp.RootServices.GetService<IAppService>();
    }
    /// <summary>
    /// 是否超级管理员
    /// </summary>
    public static bool SuperAdmin => (_appService.User?.FindFirst(ClaimConst.SuperAdmin)?.Value).ToBoolean(false);

    /// <summary>
    /// 当前用户账号
    /// </summary>
    public static string UserAccount => _appService.User?.FindFirst(ClaimConst.Account)?.Value;

    /// <summary>
    /// 当前用户Id
    /// </summary>
    public static long UserId => (_appService.User?.FindFirst(ClaimConst.UserId)?.Value).ToLong();

    /// <summary>
    /// 当前验证Id
    /// </summary>
    public static long VerificatId => (_appService.User?.FindFirst(ClaimConst.VerificatId)?.Value).ToLong();
}
