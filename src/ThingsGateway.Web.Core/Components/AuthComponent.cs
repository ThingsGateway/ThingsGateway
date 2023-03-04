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