//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using ThingsGateway.Core;
using ThingsGateway.Core.Json.Extension;

namespace ThingsGateway.Admin.Application;


public class AuthRazorService : IAuthRazorService
{

    private AjaxService AjaxService { get; set; }
    public AuthRazorService(AjaxService ajaxService)
    {
        AjaxService = ajaxService;
    }
    /// <summary>
    /// 用户登录
    /// </summary>
    public async Task<UnifyResult<LoginOutput>> LoginAsync(LoginInput input)
    {
        var ajaxOption = new AjaxOption
        {
            Url = "/api/auth/login",
            Method = "POST",
            Data = input,
        };
        var str = await AjaxService.InvokeAsync(ajaxOption);
        if (str != null)
        {
            var ret = str.RootElement.GetRawText().FromSystemTextJsonString<UnifyResult<LoginOutput>>();
            return ret;
        }
        else
        {
            throw new ArgumentNullException();
        }
    }

    /// <summary>
    /// 注销当前用户
    /// </summary>
    public async Task<UnifyResult<object>> LoginOutAsync()
    {
        var ajaxOption = new AjaxOption
        {
            Url = "/api/auth/logout",
            Method = "POST",
        };
        using var str = await AjaxService.InvokeAsync(ajaxOption);
        if (str != null)
        {
            var ret = str.RootElement.GetRawText().FromSystemTextJsonString<UnifyResult<object>>();
            return ret;
        }
        else
        {
            throw new ArgumentNullException();
        }
    }
}
