namespace ThingsGateway.Application
{
    /// <summary>
    /// AppStartup启动类
    /// </summary>
    public class Startup : AppStartup
    {

        public void ConfigureServices(IServiceCollection services)
        {
            //事件总线
            services.AddEventBus();
            services.AddSingleton<ApplicationCacheService>();

        }
    }
}