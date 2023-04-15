namespace ThingsGateway.Web.Foundation
{
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
}