namespace ThingsGateway.Core
{
    /// <summary>
    /// AppStartup启动类
    /// </summary>
    [AppStartup(99)]
    public class Startup : AppStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddComponent<LoggingConsoleComponent>();//启动控制台日志格式化组件
            services.AddComponent<LoggingFileComponent>();//启动日志写入文件组件

            // 配置雪花Id算法机器码
            YitIdHelper.SetIdGenerator(new IdGeneratorOptions
            {
                WorkerId = 4// 取值范围0~63
            });

            // 缓存注册
            services.AddCache();
        }
    }
}