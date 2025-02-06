//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

using System.Net;
using System.Reflection;
using System.Security.Claims;

using UAParser;

namespace ThingsGateway.Admin.Application;

public class AppService : IAppService
{
    public string GetReturnUrl(string returnUrl)
    {
        var url = QueryHelpers.AddQueryString(CookieAuthenticationDefaults.LoginPath, new Dictionary<string, string?>
        {
            ["ReturnUrl"] = returnUrl
        });
        return url;
    }

    public async Task LoginOutAsync()
    {
        try
        {
            await App.HttpContext!.SignOutAsync().ConfigureAwait(false);
            App.HttpContext!.SignoutToSwagger();
        }
        catch
        {
        }
    }

    public ClientInfo? ClientInfo
    {
        get
        {
            var str = App.HttpContext?.Request?.Headers?.UserAgent;
            ClientInfo? clientInfo = null;
            if (!string.IsNullOrEmpty(str))
            {
                clientInfo = Parser.GetDefault().Parse(str);
            }
            return clientInfo;
        }
    }

    public async Task LoginAsync(ClaimsIdentity identity, int expire)
    {
        var diffTime = DateTime.Now + TimeSpan.FromMinutes(expire);
        //var diffTime = DateTime.Now.AddMinutes(expire);
        await App.HttpContext!.SignInAsync(Assembly.GetEntryAssembly().GetName().Name, new ClaimsPrincipal(identity), new AuthenticationProperties()
        {
            IsPersistent = true,
            AllowRefresh = true,
            ExpiresUtc = diffTime,
        }).ConfigureAwait(false);
    }
    public ClaimsPrincipal? User => App.User;

    public IPAddress? RemoteIpAddress => App.HttpContext?.Connection?.RemoteIpAddress;

    public int LocalPort => App.HttpContext.Connection.LocalPort;
}
