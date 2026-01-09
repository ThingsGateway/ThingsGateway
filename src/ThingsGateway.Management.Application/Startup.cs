//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Reflection;

using ThingsGateway.Authentication;

namespace ThingsGateway.Management.Application;

[AppStartup(-100000)]
public class Startup : AppStartup
{
    public void Configure(IServiceCollection services)
    {
        ProAuthentication.TryGetAuthorizeInfo(out var authorizeInfo);
        //#if !DEBUG
        //        if (!authorizeInfo.Auth)
        //            throw new Exception($"UUID: {authorizeInfo.Uuid} no auth");
        //#endif
        services.AddSingleton<BaseService<ManagementConfig>>(a => a.GetService<ManagementConfigService>());
        services.AddSingleton<ManagementConfigService>();

        services.AddHostedService<ManagementHostedService>();


        services.AddScoped<ManagementRpcServerService>();
        services.AddScoped<IBackendLogService>(a => a.GetService<ManagementRpcServerService>());
        services.AddScoped<IRpcLogService>(a => a.GetService<ManagementRpcServerService>());
        services.AddScoped<IManagementUpgradeRpcServer>(a => a.GetService<ManagementRpcServerService>());
        services.AddScoped<IRedundancyHostedService>(a => a.GetService<ManagementRpcServerService>());
        services.AddScoped<IRedundancyService>(a => a.GetService<ManagementRpcServerService>());
        services.AddScoped<IAuthenticationService>(a => a.GetService<ManagementRpcServerService>());
        services.AddScoped<IRestartService>(a => a.GetService<ManagementRpcServerService>());
        services.AddScoped<IChannelEnableService>(a => a.GetService<ManagementRpcServerService>());

        services.AddScoped<ITextFileReadService>(a => a.GetService<ManagementRpcServerService>());
        services.AddScoped<IPluginPageService>(a => a.GetService<ManagementRpcServerService>());
        services.AddScoped<IRealAlarmService>(a => a.GetService<ManagementRpcServerService>());

        services.AddScoped<IGlobalDataService>(a => a.GetService<ManagementRpcServerService>());
        services.AddScoped<IRulesEngineHostedService>(a => a.GetService<ManagementRpcServerService>());
        services.AddScoped<IRulesPageService>(a => a.GetService<ManagementRpcServerService>());
        services.AddScoped<IChannelPageService>(a => a.GetService<ManagementRpcServerService>());
        services.AddScoped<IDevicePageService>(a => a.GetService<ManagementRpcServerService>());
        services.AddScoped<IVariablePageService>(a => a.GetService<ManagementRpcServerService>());
        services.AddScoped<IChannelModelPageService>(a => a.GetService<ManagementRpcServerService>());
        services.AddScoped<IDeviceModelPageService>(a => a.GetService<ManagementRpcServerService>());
        services.AddScoped<IVariableModelPageService>(a => a.GetService<ManagementRpcServerService>());

        services.AddScoped<IMemoryVariablePageService>(a => a.GetService<ManagementRpcServerService>());
        services.AddScoped<IMemoryVariableModelPageService>(a => a.GetService<ManagementRpcServerService>());

        services.AddScoped<IHardwarePageService>(a => a.GetService<ManagementRpcServerService>());

        services.AddScoped<DmtpActorContext>();

        services.AddScoped<IPluginService, ManagementPluginService>();

        services.AddSingleton<IUpdateZipFileService, UpdateZipFileService>();



    }

    public void Use(IServiceProvider serviceProvider)
    {
        //检查ConfigId
        var configIdGroup = DbContext.DbConfigs.GroupBy(it => it.ConfigId);
        foreach (var configId in configIdGroup)
        {
            if (configId.Count() > 1) throw new($"connect configId: {configId.Key} Duplicate!");
        }

        //遍历配置
        DbContext.DbConfigs?.ForEach(it =>
        {
            var connection = DbContext.GetDB().GetConnection(it.ConfigId);//获取数据库连接对象

            if (it.InitDatabase == true)
                connection.DbMaintenance.CreateDatabase();//创建数据库,如果存在则不创建
        });
        var fullName = Assembly.GetExecutingAssembly().FullName;//获取程序集全名
        CodeFirstUtils.CodeFirst(fullName!);//CodeFirst

    }

}
