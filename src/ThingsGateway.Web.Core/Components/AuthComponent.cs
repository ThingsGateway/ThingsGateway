#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/dotnetchina/ThingsGateway
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
        /// <inheritdoc/>
        public void Load(IServiceCollection services, ComponentContext componentContext)
        {
            //共用Cookie和JWT，查看Furion文档(http://furion.baiqian.ltd/docs/auth-control?_highlight=authenticationscheme#1523-%E6%B7%B7%E5%90%88%E8%BA%AB%E4%BB%BD%E9%AA%8C%E8%AF%81)
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
            services.AddJwt<BlazorAuthorizeHandler>(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });
        }
    }
}