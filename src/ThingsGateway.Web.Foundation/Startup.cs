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

namespace ThingsGateway.Web.Foundation;

/// <summary>
/// AppStartup启动类
/// </summary>
[AppStartup(99)]
public class Startup : AppStartup
{
    /// <inheritdoc/>
    public void ConfigureServices(IServiceCollection services)
    {

        //运行日志写入数据库配置
        services.AddDatabaseLogging<TGRunTimeDatabaseLoggingWriter>(options =>
        {
            options.WriteFilter = (logMsg) =>
            {
                return (
                !logMsg.LogName.StartsWith("System") &&
                !logMsg.LogName.StartsWith("Microsoft")
                );
            };
        });

        //添加采集/上传后台服务
        services.AddHostedService<CollectDeviceWorker>();
        services.AddHostedService<AlarmWorker>();
        services.AddHostedService<ValueHisWorker>();
        services.AddHostedService<UploadDeviceWorker>();
    }
}