namespace ThingsGateway.Application
{
    /// <summary>
    /// AppStartup启动类
    /// </summary>
    public class Startup : AppStartup
    {
        /// <summary>
        /// 配置
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
            //事件总线
            services.AddEventBus();
            services.AddSingleton<ApplicationCacheService>();

        }
    }
}