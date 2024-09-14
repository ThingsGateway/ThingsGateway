//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Admin.Application;
using ThingsGateway.Core;

namespace ThingsGateway.Admin.NetCore;


public class AuthRazorService : IAuthRazorService
{

    private IAuthService AuthService { get; set; }
    public AuthRazorService(IAuthService authService)
    {
        AuthService = authService;
    }
    /// <summary>
    /// 用户登录
    /// </summary>
    public async Task<UnifyResult<LoginOutput>> LoginAsync(LoginInput input)
    {

        var ret = await AuthService.LoginAsync(input).ConfigureAwait(false);
        return new UnifyResult<LoginOutput>
        {
            Code = 200,
            Msg = "Success",
            Data = ret,
            Time = DateTime.Now,
        };

    }

    /// <summary>
    /// 注销当前用户
    /// </summary>
    public async Task<UnifyResult<object>> LoginOutAsync()
    {
        await AuthService.LoginOutAsync().ConfigureAwait(false);
        return new UnifyResult<object>
        {
            Code = 200,
            Msg = "Success",
            Data = default,
            Time = DateTime.Now,
        };
    }
}
