
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
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

        var fullName = Assembly.GetExecutingAssembly().FullName;//获取程序集全名
        CodeFirstUtils.CodeFirst(fullName!);//CodeFirst

        TypeAdapterConfig.GlobalSettings.Scan(App.Assemblies.ToArray());
        // 配置默认全局映射（支持覆盖）
        TypeAdapterConfig.GlobalSettings.Default
              .NameMatchingStrategy(NameMatchingStrategy.Flexible)
              .PreserveReference(true);

        // 配置默认全局映射（忽略大小写敏感）
        TypeAdapterConfig.GlobalSettings.Default
              .NameMatchingStrategy(NameMatchingStrategy.IgnoreCase)
              .PreserveReference(true);

        //运行日志写入数据库配置
        services.AddDatabaseLogging<BackendLogDatabaseLoggingWriter>(options =>
        {
            options.WriteFilter = (logMsg) =>
            {
                return (
                !logMsg.LogName.StartsWith("System") &&
                !logMsg.LogName.StartsWith("Microsoft") &&
                !logMsg.LogName.StartsWith("Blazor")
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

        services.AddHostedService<AdminTaskService>();
        services.AddHostedService<CollectDeviceHostedService>();
        services.AddHostedService<BusinessDeviceHostedService>();
        services.AddHostedService<AlarmHostedService>();
        services.AddHostedService<ManagementHostedService>();
    }
}