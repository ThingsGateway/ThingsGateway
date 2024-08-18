//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;

using ThingsGateway.Admin.Application;
using ThingsGateway.Admin.NetCore;
using ThingsGateway.Admin.Razor;
using ThingsGateway.Razor;

namespace ThingsGateway.Winform;

[AppStartup(-999999)]
public class Startup : AppStartup
{
    public void ConfigBlazorServer(IServiceCollection services)
    {
        ConfigureAdminApp(services);
        // 增加网站服务
        AddWebSiteServices(services);
    }

    /// <summary>
    /// 添加网站服务
    /// </summary>
    /// <param name="services"></param>
    private IServiceCollection AddWebSiteServices(IServiceCollection services)
    {
        services.AddScoped<BlazorAppContext>(a =>
        {
            var sysResourceService = a.GetService<ISysResourceService>();
            var userCenterService = a.GetService<IUserCenterService>();
            var userService = a.GetService<ISysUserService>();
            var appContext = new BlazorAppContext(sysResourceService, userCenterService, userService);
            appContext.TitleLocalizer = a.GetRequiredService<IStringLocalizer<ThingsGateway.Razor.MainLayout>>();
            return appContext;
        });
        services.AddAuthorizationCore();
        services.AddSingleton<IAuthorizationHandler, BlazorAuthorizationHandler>();
        services.AddSingleton<AuthenticationStateProvider, BlazorAuthenticationStateProvider>();
        return services;
    }


    private void ConfigureAdminApp(IServiceCollection services)
    {
        services.AddSingleton<IHostApplicationLifetime, ApplicationLifetime>();
        services.AddSingleton<ApplicationLifetime>();

        services.AddSingleton<IAuthService, AuthService>();

        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<IAuthRazorService, AuthRazorService>();

        var appService = new NetCoreAppService();
        services.AddSingleton<IAppService, NetCoreAppService>(a => appService);
        services.AddSingleton<NetCoreAppService>(a => appService);

        services.AddSingleton<IApiPermissionService, ApiPermissionService>();

        services.AddSingleton<ISignalrNoticeService, SignalrNoticeService>();
        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<HostedServiceExecutor>();

        services.AddScoped<IPlatformService, PhotinoPlatformService>();

#if !Admin
        services.AddScoped<ThingsGateway.Gateway.Application.IGatewayExportService, ThingsGateway.Gateway.Razor.GatewayExportService>();
#endif


    }



}
