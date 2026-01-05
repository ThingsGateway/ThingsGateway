//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Reflection;

using ThingsGateway.Authentication;

namespace ThingsGateway.Gateway.Application;

[AppStartup(-100)]
public class Startup : AppStartup
{
    public void Configure(IServiceCollection services)
    {
        #region R

        services.AddSingleton<IRulesService, RulesService>();
        services.AddSingleton<IRulesPageService>(a => a.GetService<IRulesService>());
        services.AddGatewayHostedService<IRulesEngineHostedService, RulesEngineHostedService>();

        #endregion

        #region M

        services.AddSingleton<IRedundancyService, RedundancyService>();
        services.AddGatewayHostedService<IRedundancyHostedService, RedundancyHostedService>();

        services.AddHostedService<ManagementHostedService>();
        services.AddHostedService<WebApiHostedService>();

        services.AddSingleton<GatewayRedundantSerivce>();
        services.AddSingleton<IGatewayRedundantSerivce>(provider => provider.GetRequiredService<GatewayRedundantSerivce>());

        #endregion
        services.AddSingleton<ITextFileReadService, TextFileReadService>();

        ProAuthentication.TryGetAuthorizeInfo(out var authorizeInfo);

        services.AddConfigurableOptions<ChannelThreadOptions>();
        services.AddConfigurableOptions<GatewayLogOptions>();
        services.AddConfigurableOptions<RpcLogOptions>();

        services.AddConfigurableOptions<RemoteClientManagementOptions>();
        services.AddConfigurableOptions<RemoteServerManagementOptions>();
        services.AddConfigurableOptions<WebApiOptions>();

        //运行日志写入数据库配置
        services.AddDatabaseLogging<BackendLogDatabaseLoggingWriter>(options =>
        {
            options.NameFilter = (name) =>
            {
                return (
                !name.StartsWith("System") &&
                !name.StartsWith("Microsoft") &&
                !name.StartsWith("Blazor") &&
                !name.StartsWith("BootstrapBlazor")
                );
            };
        });

        services.AddSingleton<IChannelThreadManage, ChannelThreadManage>();
        services.AddSingleton<IChannelService, ChannelService>();
        services.AddSingleton<IChannelPageService>(a => a.GetService<IChannelRuntimeService>());
        services.AddSingleton<IChannelModelPageService>(a => a.GetService<IChannelRuntimeService>());
        services.AddSingleton<IChannelRuntimeService, ChannelRuntimeService>();
        services.AddSingleton<IVariableService, VariableService>();
        services.AddSingleton<IVariableRuntimeService, VariableRuntimeService>();
        services.AddSingleton<IVariablePageService>(a => a.GetService<IVariableRuntimeService>());
        services.AddSingleton<IVariableModelPageService>(a => a.GetService<IVariableRuntimeService>());
        services.AddSingleton<IDeviceService, DeviceService>();
        services.AddSingleton<IDeviceRuntimeService, DeviceRuntimeService>();
        services.AddSingleton<IDevicePageService>(a => a.GetService<IDeviceRuntimeService>());
        services.AddSingleton<IDeviceModelPageService>(a => a.GetService<IDeviceRuntimeService>());
        services.AddSingleton<IPluginService, PluginService>();
        services.AddSingleton<IPluginPageService>(a => a.GetService<IPluginService>());

        services.AddSingleton<IBackendLogService, BackendLogService>();
        services.AddSingleton<IRpcLogService, RpcLogService>();
        services.AddSingleton<IRpcService, RpcService>();
        services.AddSingleton<IAuthenticationService, AuthenticationService>();
        services.AddSingleton<IRestartService, RestartService>();
        services.AddSingleton<IChannelEnableService, ChannelEnableService>();
        services.AddSingleton<IRealAlarmService, RealAlarmService>();
        services.AddSingleton<IGlobalDataService, GlobalDataService>();

        services.AddGatewayHostedService<IAlarmHostedService, AlarmHostedService>();
        services.AddHostedService<GatewayMonitorHostedService>();
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

        //兼容变量名称唯一键处理
        try
        {
            using var db = DbContext.GetDB<Variable>();
            if (db.DbMaintenance.IsAnyIndex("unique_variable_name"))
            {
                DropIndex(db, "unique_variable_name", "variable");
            }
        }
        catch { }

        var fullName = Assembly.GetExecutingAssembly().FullName;//获取程序集全名
        CodeFirstUtils.CodeFirst(fullName!);//CodeFirst

        //10.4.9 删除logenable
        try
        {
            using var db = DbContext.GetDB<Channel>();
            if (db.DbMaintenance.IsAnyColumn(nameof(Channel), "LogEnable", false))
            {
                var tables = db.DbMaintenance.DropColumn(nameof(Channel), "LogEnable");
            }
        }
        catch { }
        try
        {
            using var db = DbContext.GetDB<Device>();
            if (db.DbMaintenance.IsAnyColumn(nameof(Device), "LogEnable", false))
            {
                var tables = db.DbMaintenance.DropColumn(nameof(Device), "LogEnable");
            }
        }
        catch { }

        try
        {
            using var db = DbContext.GetDB<BackendLog>();
            if (db.CurrentConnectionConfig.DatabaseType == DatabaseType.Sqlite)
            {
                if (!db.DbMaintenance.IsAnyIndex("idx_backendlog_logtime_date"))
                {
                    var indexsql = "CREATE INDEX idx_backendlog_logtime_date ON backend_log(strftime('%Y-%m-%d', LogTime));";
                    db.Ado.ExecuteCommand(indexsql);
                }
            }
        }
        catch { }

        try
        {
            using var db = DbContext.GetDB<RpcLog>();
            if (db.CurrentConnectionConfig.DatabaseType == DatabaseType.Sqlite)
            {
                if (!db.DbMaintenance.IsAnyIndex("idx_rpclog_logtime_date"))
                {
                    var indexsql = "CREATE INDEX idx_rpclog_logtime_date ON rpc_log(strftime('%Y-%m-%d', LogTime));";
                    db.Ado.ExecuteCommand(indexsql);
                }
            }
        }
        catch { }

        serviceProvider.GetService<IHostApplicationLifetime>().ApplicationStarted.Register(() => serviceProvider.GetService<ILoggerFactory>().CreateLogger(nameof(ThingsGateway)).LogInformation("ThingsGateway is started..."));
        serviceProvider.GetService<IHostApplicationLifetime>().ApplicationStopping.Register(() => serviceProvider.GetService<ILoggerFactory>().CreateLogger(nameof(ThingsGateway)).LogInformation("ThingsGateway is stopping..."));
    }
    /// <summary>
    /// 删除指定表上的索引（自动根据数据库类型生成正确的 DROP INDEX SQL）
    /// </summary>
    /// <param name="db">数据库连接</param>
    /// <param name="indexName">索引名</param>
    /// <param name="tableName">表名（部分数据库需要）</param>
    private static void DropIndex(ISqlClient db, string indexName, string tableName)
    {
        if (string.IsNullOrWhiteSpace(indexName))
            throw new ArgumentNullException(nameof(indexName));

        string dropIndexSql;

        switch (db.CurrentConnectionConfig.DatabaseType)
        {
            case DatabaseType.MySql:
            case DatabaseType.SqlServer:
                dropIndexSql = $"DROP INDEX {indexName} ON {tableName};";
                break;

            default:
                dropIndexSql = $"DROP INDEX {indexName};";
                break;
        }
        db.Ado.ExecuteCommand(dropIndexSql);
    }
}
