//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;

using ThingsGateway.Admin.Application;
using ThingsGateway.Admin.Razor;
using ThingsGateway.Server;

namespace Microsoft.Extensions.DependencyInjection;

[AppStartup(99999)]
public class Startup : AppStartup
{
    public void ConfigBlazorServer(IServiceCollection services)
    {
        // 增加 BootstrapBlazor 服务
        services.AddBootstrapBlazor(option => option.JSModuleVersion = Random.Shared.Next(10000).ToString());

        //已添加AddOptions
        // 增加多语言支持配置信息
        services.AddRequestLocalization<IOptionsMonitor<BootstrapBlazor.Components.BootstrapBlazorOptions>>((localizerOption, blazorOption) =>
        {
            blazorOption.OnChange(op => Invoke(op));
            Invoke(blazorOption.CurrentValue);

            void Invoke(BootstrapBlazor.Components.BootstrapBlazorOptions option)
            {
                var supportedCultures = option.GetSupportedCultures();
                localizerOption.SupportedCultures = supportedCultures;
                localizerOption.SupportedUICultures = supportedCultures;
            }
        });

        services.AddScoped<BlazorAppContext>();

        // 增加网站服务
        AddWebSiteServices(services);

        services.ConfigureIconThemeOptions(options => options.ThemeKey = "fa");
    }

    /// <summary>
    /// 添加网站服务
    /// </summary>
    /// <param name="services"></param>
    private IServiceCollection AddWebSiteServices(IServiceCollection services)
    {
        // 配置
        services.TryAddSingleton<IConfigureOptions<WebsiteOptions>, ConfigureOptions<WebsiteOptions>>();

        services.AddHttpContextAccessor();

        //添加cookie授权
        var authenticationBuilder = services.AddAuthentication(nameof(ThingsGateway)).AddCookie(nameof(ThingsGateway), a =>
         {
             a.AccessDeniedPath = "/Account/AccessDenied/";
             a.LogoutPath = "/auth/logout";
             a.LoginPath = "/Account/Login/";
         });

        //var authenticationBuilder = services.AddAuthentication(options =>
        //{
        //    options.DefaultAuthenticateScheme = nameof(ThingsGateway);
        //    options.DefaultChallengeScheme = nameof(ThingsGateway);
        //});
        // 添加jwt授权
        authenticationBuilder.AddJwtBearer(options =>
        {
            var jwtSettings = JWTEncryption.GetJWTSettings();
            // 配置 JWT 验证信息
            options.TokenValidationParameters = JWTEncryption.CreateTokenValidationParameters(jwtSettings);
        });

        services.AddAuthorization();
        services.AddCascadingAuthenticationState();
        services.AddScoped<IAuthorizationHandler, BlazorAuthenticationStateProvider>();

        return services;
    }
}