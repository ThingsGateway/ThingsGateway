//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Mapster;

using System.Reflection;

namespace ThingsGateway.Gateway.Application;

[AppStartup(-100)]
public class Startup : AppStartup
{
    public void ConfigureAdminApp(IServiceCollection services)
    {
        //底层多语言配置
        //Foundation.LocalizerUtil.SetLocalizerFactory((a) => App.CreateLocalizerByType(a));

        TypeAdapterConfig.GlobalSettings.Scan(App.Assemblies.ToArray());
        // 配置默认全局映射（支持覆盖）
        TypeAdapterConfig.GlobalSettings.Default
              .PreserveReference(true);

        //运行日志写入数据库配置
        services.AddDatabaseLogging<BackendLogDatabaseLoggingWriter>(options =>
        {
            options.WriteFilter = (logMsg) =>
            {
                return (
                !logMsg.LogName.StartsWith("System") &&
                !logMsg.LogName.StartsWith("Microsoft") &&
                !logMsg.LogName.StartsWith("Blazor")&&
                !logMsg.LogName.StartsWith("BootstrapBlazor")
                );
            };
        });


        services.AddSingleton<IChannelService, ChannelService>();
        services.AddSingleton<IVariableService, VariableService>();
        services.AddSingleton<IDeviceService, DeviceService>();
        services.AddSingleton<IPluginService, PluginService>();
        services.AddSingleton<IBackendLogService, BackendLogService>();
        services.AddSingleton<IRpcLogService, RpcLogService>();
        services.AddSingleton<IRpcService, RpcService>();
        services.AddScoped<IGatewayExportService, GatewayExportService>();

        services.AddGatewayHostedService<ICollectDeviceHostedService, CollectDeviceHostedService>();
        services.AddGatewayHostedService<IBusinessDeviceHostedService, BusinessDeviceHostedService>();
        services.AddGatewayHostedService<IAlarmHostedService, AlarmHostedService>();
    }

    public void UseAdminCore(IServiceProvider serviceProvider)
    {
        //检查ConfigId
        var configIdGroup = DbContext.DbConfigs.GroupBy(it => it.ConfigId);
        foreach (var configId in configIdGroup)
        {
            if (configId.Count() > 1) throw new($"Sqlsugar connect configId: {configId.Key} Duplicate!");
        }

        //遍历配置
        DbContext.DbConfigs?.ForEach(it =>
        {
            var connection = DbContext.Db.GetConnection(it.ConfigId);//获取数据库连接对象
            connection.DbMaintenance.CreateDatabase();//创建数据库,如果存在则不创建
        });
        var fullName = Assembly.GetExecutingAssembly().FullName;//获取程序集全名
        CodeFirstUtils.CodeFirst(fullName!);//CodeFirst
    }
}
