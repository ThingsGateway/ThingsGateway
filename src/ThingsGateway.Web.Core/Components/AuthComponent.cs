#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using Microsoft.AspNetCore.Authentication.Cookies;

namespace ThingsGateway.Web.Core
{
    /// <summary>
    /// 认证相关组件，需要在AddRazorPages前注入
    /// </summary>
    public sealed class AuthComponent : IServiceComponent
    {
        public void Load(IServiceCollection services, ComponentContext componentContext)
        {
            //services.AddAppAuthorization<BlazorAuthorizeHandler>();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
            services.AddJwt<BlazorAuthorizeHandler>(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });
        }
    }
}